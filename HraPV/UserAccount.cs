using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HraPV;

public class UserAccount
{
    public string PasswordHash { get; set; } = "";
    public string CurrentLocation { get; set; } = "courtyard";
    public List<string> Inventory { get; set; } = new();
}

public static class AuthService
{
    private static readonly string UsersFile = "users.json";
    private static Dictionary<string, UserAccount> _users = new();

    static AuthService()
    {
        if (File.Exists(UsersFile))
        {
            try
            {
                string json = File.ReadAllText(UsersFile);
                _users = JsonSerializer.Deserialize<Dictionary<string, UserAccount>>(json) ?? new();
            }
            catch { _users = new(); }
        }
    }

    public static bool Authenticate(string name, string password, out UserAccount account)
    {
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

        if (_users.TryGetValue(name, out account))
        {
            return account.PasswordHash == hash;
        }

        account = new UserAccount { PasswordHash = hash };
        _users[name] = account;
        Save();
        return true;
    }

    public static void SaveProgress(string name, string loc, List<string> inv)
    {
        if (_users.ContainsKey(name))
        {
            _users[name].CurrentLocation = loc;
            _users[name].Inventory = inv.ToList();
            Save();
        }
    }

    private static void Save()
    {
        string json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(UsersFile, json);
    }
}