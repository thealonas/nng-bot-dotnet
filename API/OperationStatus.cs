using nng_bot.Models;

namespace nng_bot.API;

public class OperationStatus
{
    public OperationStatus()
    {
        UsersAskedForEditor = new Dictionary<long, int>();
        UsersToEditorGiving = new HashSet<EditorRequest>();
        GroupsHistory = new Dictionary<long, long>();
    }

    public HashSet<EditorRequest> UsersToEditorGiving { get; }
    public Dictionary<long, int> UsersAskedForEditor { get; }
    public Dictionary<long, long> GroupsHistory { get; }

    public void AddEditorRequest(EditorRequest request)
    {
        UsersToEditorGiving.RemoveWhere(x => x.User == request.User);
        UsersToEditorGiving.Add(request);
    }

    public void AddUserAskedForEditor(long id)
    {
        if (UsersAskedForEditor.ContainsKey(id)) UsersAskedForEditor[id]++;
        else UsersAskedForEditor.Add(id, 1);
    }

    public void ClearRequests(long user)
    {
        UsersToEditorGiving.RemoveWhere(x => x.User.Equals(user));
    }
}
