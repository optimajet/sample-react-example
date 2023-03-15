namespace WorkflowLib;

public static class Users
{
    public static readonly List<User> Data = new()
    {
        new User {Name = "Peter", Roles = new List<string> {"User", "Manager"}, Division = "IT Department"},
        new User {Name = "Paula", Roles = new List<string> {"User"}, Division = "IT Department"},
        new User {Name = "Margaret", Roles = new List<string> {"User"},  Division = "First Line"},
        new User {Name = "Mike", Roles = new List<string> {"User", "Manager"},  Division = "First Line"},
        new User {Name = "John", Roles = new List<string> {"Manager"},  Division = "Accounting"},
        new User {Name = "Janis", Roles = new List<string> {"User", "Manager"},  Division = "Accounting"},
        new User {Name = "Sam", Roles = new List<string> {"User"}, Division = "Law Department"},
        new User {Name = "Samantha", Roles = new List<string> {"User", "Manager"}, Division = "Law Department"},
    };

    public static readonly Dictionary<string, User> UserDict = Data.ToDictionary(u => u.Name);
}