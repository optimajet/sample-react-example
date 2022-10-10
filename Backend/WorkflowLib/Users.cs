namespace WorkflowLib;

public static class Users
{
    public static readonly List<User> Data = new()
    {
        new User {Name = "Peter", Roles = new List<string> {"User", "Manager"}},
        new User {Name = "Margaret", Roles = new List<string> {"User"}},
        new User {Name = "John", Roles = new List<string> {"Manager"}},
        new User {Name = "Sam", Roles = new List<string> {"Manager"}},
    };

    public static readonly Dictionary<string, User> UserDict = Data.ToDictionary(u => u.Name);
}