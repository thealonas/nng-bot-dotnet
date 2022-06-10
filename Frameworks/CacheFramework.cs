using System.Text;
using Newtonsoft.Json;
using nng.Enums;
using nng.Exceptions;
using nng.Misc;
using nng.VkFrameworks;
using nng_bot.API;
using nng_bot.Configs;
using nng_bot.Enums;
using nng_bot.Exceptions;
using nng_bot.Models;

namespace nng_bot.Frameworks;

public class CacheFramework
{
    public const string CacheFilePath = "cache/cache.json";
    public const string BannedUserFilePath = "cache/banned_users_requests.json";

    private readonly ILogger<CacheFramework> _logger;

    private readonly Dictionary<long, string> _names;
    private readonly int _validDatetime;
    private readonly VkFramework _vkFramework;

    public CacheFramework(IConfiguration configuration, OperationStatus status,
        ILogger<CacheFramework> logger, VkFramework vkFramework)
    {
        Status = status;
        _logger = logger;
        _vkFramework = vkFramework;
        _names = new Dictionary<long, string>();
        _validDatetime = configuration.GetSection("Cache").GetValue<int>("UpdatePerHours");
    }

    private OperationStatus Status { get; }

    public static IEnumerable<UnbanRequest> BannedRequestsFull
    {
        get
        {
            var output = JsonConvert.DeserializeObject<UnbanRequest[]>(File.ReadAllText(BannedUserFilePath));
            if (output == null) return new List<UnbanRequest>();
            output = output.OrderBy(x => x.CreatedOn).Reverse().ToArray();
            return output;
        }
    }

    private static DateTime? GetAccountAge(long id)
    {
        try
        {
            return VkAccountAgeChecker.GetAccountAge(id);
        }
        catch (VkFrameworkMethodException)
        {
            return null;
        }
    }

    public static void SaveBannedRequests(IEnumerable<UnbanRequest> requests)
    {
        var output = JsonConvert.SerializeObject(requests);
        File.WriteAllText(BannedUserFilePath, output);
    }

    public static void AddBannedRequest(UnbanRequest request)
    {
        var output = BannedRequestsFull.ToList();
        output.Add(request);
        File.WriteAllText(BannedUserFilePath, JsonConvert.SerializeObject(output));
    }

    public static void OverrideBannedRequest(long userId, UnbanRequest request)
    {
        var output = BannedRequestsFull.ToList();
        output.RemoveAll(x => x.UserId == userId);
        output.Add(request);
        File.WriteAllText(BannedUserFilePath, JsonConvert.SerializeObject(output));
    }

    public static void SetBannedRequestStatus(UnbanRequest request, UnbanRequestStatus status)
    {
        var requests = BannedRequestsFull.ToList();
        foreach (var req in requests.Where(req => req.Equals(request))) req.Status = status;
        File.WriteAllText(BannedUserFilePath, JsonConvert.SerializeObject(requests));
    }

    public static void SetBannedWatchedStatus(UnbanRequest request, bool status)
    {
        var requests = BannedRequestsFull.ToList();
        foreach (var req in requests.Where(req => req.Equals(request))) req.IsWatched = status;
        File.WriteAllText(BannedUserFilePath, JsonConvert.SerializeObject(requests));
    }

    public static bool TryGetBannedRequestFromFullList(long user, out UnbanRequest request)
    {
        var requests = BannedRequestsFull.ToList();
        try
        {
            request = requests.First(x => x.UserId == user);
            return true;
        }
        catch (InvalidOperationException)
        {
            request = new UnbanRequest(0, DateTime.Now);
            return false;
        }
    }

    public static void CheckCache(string path)
    {
        if (File.Exists(path)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                                  throw new DataException($"Path {path} does not exist"));
        var file = File.Create(path);
        file.Close();
    }

    public static void DeleteCache(string path)
    {
        if (!File.Exists(path)) return;
        File.Delete(path);
    }

    private static void SaveData(object data, string path)
    {
        CheckCache(path);
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };
        var serializeObject = JsonConvert.SerializeObject(data, settings);
        File.WriteAllText(CacheFilePath, serializeObject, Encoding.Default);
    }

    private static T LoadData<T>(string path)
    {
        var data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        if (data == null) throw new DataException($"Cache with assigned path {path} is null");
        return data;
    }

    private bool TryGetName(long id, out string name)
    {
        name = string.Empty;
        if (!_names.ContainsKey(id)) return false;
        name = _names[id];
        return true;
    }

    private static IEnumerable<long> GetBannedInConfiguration()
    {
        return UsersConfigurationProcessor.UsersConfiguration.BannedUsers;
    }

    public static void SaveCache(CacheData data)
    {
        SaveData(data, CacheFilePath);
    }

    public static CacheData LoadCache()
    {
        return LoadData<CacheData>(CacheFilePath);
    }

    public bool IfCacheValid()
    {
        try
        {
            return File.Exists(CacheFilePath) &&
                   Math.Ceiling((DateTime.Now - LoadCache().CreatedOn).TotalHours) < _validDatetime;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool IsBanned(long user)
    {
        try
        {
            return (LoadCache().BannedUsers.Any(x => x.Id == user && x.Deleted == null) ||
                    GetBannedInConfiguration().Any(x => x == user)) && !Status.IfUserIsUnbanned(user);
        }
        catch (ArgumentNullException)
        {
            return false;
        }
    }

    public bool TryGetBannedPriority(long user, out BanPriority priority)
    {
        try
        {
            priority = BanPriority.White;

            var bannedInBot = GetBannedInConfiguration().ToList();
            if (bannedInBot.Contains(user))
            {
                priority = BanPriority.Local;
                return true;
            }

            if (Status.IfUserIsUnbanned(user)) return false;

            var bannedUser = LoadCache().BannedUsers.FirstOrDefault(x => x.Id == user && x.Deleted == null);

            if (!bannedUser.Id.Equals(0))
            {
                priority = bannedUser.Priority;
                return true;
            }

            return false;
        }
        catch (ArgumentNullException)
        {
            priority = BanPriority.White;
            return false;
        }
    }

    private string GetName(long id)
    {
        string profileName;
        if (!TryGetName(id, out var name))
            try
            {
                var user = _vkFramework.GetUser(id);
                var userName = $"{user.FirstName} {user.LastName}";
                profileName = userName;
            }
            catch (VkFrameworkMethodException e)
            {
                _logger.LogWarning("Can't get user name {@E}", e.Message);
                profileName = "❌";
            }
        else
            profileName = name;

        return profileName;
    }

    public UserProfile LoadProfile(long id)
    {
        var profile = new UserProfile {Id = id, CreatedOn = GetAccountAge(id)};
        var cache = LoadCache();
        try
        {
            var userObject = cache.BannedUsers.First(x => x.Id == id);
            profile.Banned = true;
            profile.BanPriority = userObject.Priority;
            profile.Deleted = userObject.Deleted != null;
            profile.Name = userObject.Name;
        }
        catch (Exception)
        {
            profile.Banned = false;
            profile.Deleted = false;
            profile.BanPriority = 0;
            profile.Name = GetName(id);
            if (!_names.ContainsKey(id)) _names.Add(id, profile.Name);
        }

        if (GetBannedInConfiguration().Any(x => x == id))
        {
            profile.Banned = true;
            profile.BanPriority = BanPriority.Local;
            profile.Deleted = false;
        }

        profile.Warnings = 0;
        var relativeUsers = cache.BannedUsers.Where(x => x.Id == id).ToList();
        if (relativeUsers.Any()) profile.Warnings = relativeUsers.First().Warnings;

        profile.EditorGroups = cache.Data.Where(group => group.Managers.Any(x => x.Equals(profile.Id)))
            .Select(x => x.Id).ToArray();
        return profile;
    }
}
