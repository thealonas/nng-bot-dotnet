using nng_bot.API;
using nng.DatabaseModels;
using nng.DatabaseProviders;
using nng.VkFrameworks;

namespace nng_bot.Frameworks;

public class UserFramework
{
    private readonly VkController _vkController;
    public readonly UsersDatabaseProvider UsersDatabase;

    public UserFramework(UsersDatabaseProvider usersDatabase, VkController vkController)
    {
        UsersDatabase = usersDatabase;
        _vkController = vkController;
    }

    public User GetById(long id)
    {
        return UsersDatabase.Collection.ToList().First(x => x.UserId.Equals(id));
    }

    public bool TryGetById(long id, out User user)
    {
        user = new User();
        try
        {
            user = GetById(id);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void AddUser(long id)
    {
        var name = VkFrameworkExecution.ExecuteWithReturn(() =>
        {
            var user = _vkController.VkFramework.GetUser(id);
            return $"{user.FirstName} {user.LastName}";
        });

        var user = new User
        {
            Admin = false,
            App = true,
            Banned = false,
            UserId = id,
            Name = name,
            LastUpdated = DateTime.Now,
            Thanks = false
        };

        UsersDatabase.Collection.Insert(user);
    }
}
