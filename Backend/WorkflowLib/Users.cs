namespace WorkflowLib;

public static class Users
{
    public static readonly List<User> Data = new()
    {
        new User {Name = "Peter", Roles = new List<string> {"User", "Manager"}, Division = "IT Department"},
        new User {Name = "Margaret", Roles = new List<string> {"User"},  Division = "First Line"},
        new User {Name = "John", Roles = new List<string> {"Manager"},  Division = "Accounting"},
        new User {Name = "Sam", Roles = new List<string> {"Manager"}, Division = "Law Department"},
    };

    public static readonly Dictionary<string, User> UserDict = Data.ToDictionary(u => u.Name);
}