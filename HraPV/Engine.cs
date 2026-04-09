using HraPV;
using System.Text;

namespace HraPV;

public class Engine
{
    private readonly List<Player> _players = new();
    private readonly object _lock = new();

    public void AddPlayer(Player player) { lock (_lock) _players.Add(player); }
    public void RemovePlayer(Player player) { lock (_lock) _players.Remove(player); }

    public async Task Broadcast(string message, Player? exclude = null)
    {
        List<Player> snapshot;
        lock (_lock) snapshot = _players.ToList();
        foreach (var p in snapshot) if (p != exclude) await p.Send(message);
    }

    public async Task ProcessCommand(Player player, string input)
    {
        var parts = input.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        string cmd = parts[0];
        string args = string.Join(" ", parts.Skip(1));

        switch (cmd)
        {
            case "prozkoumej": await ShowRoom(player); break;
            case "jdi": await Move(player, args); break;
            case "vezmi": await Take(player, args); break;
            case "odlož": await Drop(player, args); break;
            case "mluv": await Talk(player, args); break;
            case "inventář":
                await player.Send($"Inventář ({player.Inventory.Count}/{player.MaxInventory}): {string.Join(", ", player.Inventory)}");
                break;
            case "pomoc":
                await player.Send("Příkazy: jdi <směr>, prozkoumej, vezmi <věc>, odlož <věc>, mluv <npc>, inventář, konec");
                break;
            default: await player.Send("Tento příkaz neznám."); break;
        }
    }

    public async Task ShowRoom(Player player)
    {
        var room = World.Rooms[player.Location];
        var sb = new StringBuilder();
        sb.AppendLine($"\n[ {room.Name.ToUpper()} ]");
        sb.AppendLine(room.Description);
        sb.AppendLine($"> Východy: {string.Join(", ", room.Exits.Keys)}");
        if (room.Items.Any()) sb.AppendLine($"> Předměty: {string.Join(", ", room.Items)}");
        if (room.NPCs.Any()) sb.AppendLine($"> Postavy: {string.Join(", ", room.NPCs.Keys)}");

        var others = _players.Where(p => p.Location == player.Location && p != player).Select(p => p.Name);
        if (others.Any()) sb.AppendLine($"> Ostatní rytíři: {string.Join(", ", others)}");

        await player.Send(sb.ToString());
    }

    private async Task Move(Player player, string dir)
    {
        if (World.Rooms[player.Location].Exits.TryGetValue(dir, out var next))
        {
            await Broadcast($"{player.Name} odešel na {dir}.", player);
            player.Location = next;
            await Broadcast($"{player.Name} vstoupil do místnosti.", player);
            await ShowRoom(player);
        }
        else await player.Send("Tudy cesta nevede.");
    }

    private async Task Take(Player player, string item)
    {
        var items = World.Rooms[player.Location].Items;
        if (items.Contains(item))
        {
            if (player.Inventory.Count < player.MaxInventory)
            {
                items.Remove(item);
                player.Inventory.Add(item);
                await player.Send($"Sebral jsi {item}.");
            }
            else await player.Send("Máš plné brašny!");
        }
        else await player.Send("Nic takového tu nevidíš.");
    }

    private async Task Drop(Player player, string item)
    {
        if (player.Inventory.Remove(item))
        {
            World.Rooms[player.Location].Items.Add(item);
            await player.Send($"Odložil jsi {item}.");
        }
        else await player.Send("To u sebe nemáš.");
    }

    private async Task Talk(Player player, string npc)
    {
        if (World.Rooms[player.Location].NPCs.TryGetValue(npc, out var text))
            await player.Send($"{npc} říká: \"{text}\"");
        else await player.Send("Nikdo takový tu není.");
    }
}