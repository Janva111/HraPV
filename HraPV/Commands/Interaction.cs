using HraPV.Loaders;
using System.Text;

namespace HraPV.Commands;

public class Interaction
{
        public async Task HandleShop(Player player)
    {
        // 1. Získáme aktuální místnost z tvého statického Worldu
        if (World.Rooms.TryGetValue(player.Location, out var room))
        {
            var merchantPair = room.NPCs.FirstOrDefault(n => n.Value.Shop != null && n.Value.Shop.Count > 0);

            if (merchantPair.Value != null)
            {
                var name = merchantPair.Key;
                var npc = merchantPair.Value;

                var sb = new StringBuilder();
                sb.AppendLine($"\n=== {name.ToUpper()}'S TRADING POST ===");
                sb.AppendLine($"Your Purse: {player.Gold} gold");
                sb.AppendLine("--------------------------------");

                foreach (var item in npc.Shop)
                {
                    sb.AppendLine($"{item.Key} | {item.Value} gold");
                }

                sb.AppendLine("--------------------------------");
                sb.AppendLine("Type 'buy <item>' to purchase.");

                await player.Send(sb.ToString());
            }
            else
            {
                await player.Send("There's no one here to trade with.");
            }
        }
    }

    public async Task HandleBuy(Player player, string itemName)
    {
        var catalog = Shop.GetCatalog();
        if (catalog.TryGetValue(itemName.ToLower(), out var item))
        {
            if (player.Gold >= item.Price && player.Inventory.Count < player.MaxInventory)
            {
                player.Gold -= item.Price;
                player.Inventory.Add(itemName.ToLower());
                await player.Send($"You bought {itemName} for {item.Price} Gold.");
            }
            else await player.Send("Not enough gold or inventory is full.");
        }
        else await player.Send("The merchant doesn't sell that.");
    }

    public async Task HandleCraft(Player player, List<string> ingredients)
    {
        var recipe = Crafting.FindRecipe(ingredients.Select(i => i.ToLower()).ToList());
        if (recipe != null && ingredients.All(i => player.Inventory.Contains(i.ToLower())))
        {
            foreach (var ing in recipe.Ingredients) player.Inventory.Remove(ing);
            player.Inventory.Add(recipe.Result);
            await player.Send($"[ CRAFTING SUCCESS ] Created {recipe.Result}!");
        }
        else await player.Send("You can't craft that at this moment.");
    }

    public async Task HandleUse(Player player, string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            await player.Send("Usage: use <item_name>");
            return;
        }

        itemName = itemName.ToLower();

        if (!player.Inventory.Contains(itemName))
        {
            await player.Send($"You don't have {itemName} in your inventory.");
            return;
        }

        bool consumed = false;

        switch (itemName)
        {
            case "healing_salve":
            case "blue_potion":
                if (player.Health >= player.MaxHealth)
                {
                    await player.Send("You are already at full health!");
                    return;
                }
                player.Health = Math.Min(player.MaxHealth, player.Health + 50);
                await player.Send($"You use the {itemName}. It feels so good! (HP: {player.Health}/{player.MaxHealth})");
                consumed = true;
                break;

            case "apple":
            case "bread":
                player.Health = Math.Min(player.MaxHealth, player.Health + 10);
                await player.Send($"You eat the {itemName}. (HP: {player.Health}/{player.MaxHealth})");
                consumed = true;
                break;

            case "wine_bottle":
                await player.Send("You drink the wine. Everything looks a bit more colorful, doesn't it?");
                consumed = true;
                break;

            case "old_key":
                if (player.Location == "dungeon")
                {
                    await player.Send("You use the old key. The heavy iron door creaks open!");
                }
                else await player.Send("The key doesn't fit anything here.");
                break;

            case "torch":
                await player.Send("You light the torch. The shadows retreat.");
                break;

            default:
                await player.Send($"The {itemName} doesn't seem to have a use right now.");
                break;
        }

        if (consumed)
        {
            player.Inventory.Remove(itemName);
        }
    }
}