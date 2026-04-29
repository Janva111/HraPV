using System.Text.Json;

namespace HraPV.Loaders;

public class ShopItem
{
    public int Price { get; set; }
    public string Description { get; set; } = "";
}

public static class Shop
{
    private static Dictionary<string, ShopItem> _catalog = new();

    public static void LoadCatalog()
    {
        if (File.Exists("shop_items.json"))
        {
            string json = File.ReadAllText("shop_items.json");
            _catalog = JsonSerializer.Deserialize<Dictionary<string, ShopItem>>(json) ?? new();
        }
    }

    public static Dictionary<string, ShopItem> GetCatalog() => _catalog;
}