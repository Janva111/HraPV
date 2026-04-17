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
            case "look": await ShowRoom(player); break;
            case "go": await Move(player, args); break;
            case "take": await Take(player, args); break;
            case "drop": await Drop(player, args); break;
            case "talk": await Talk(player, args); break;
            case "inventory":
            case "inv":
                await player.Send($"Inventory ({player.Inventory.Count}/{player.MaxInventory}): {string.Join(", ", player.Inventory)}");
                break;
            case "help":
                await player.Send("Available commands: go <direction>, look, take <item>, drop <item>, talk <npc>, inventory, quit");
                break;
            default:
                await player.Send("I don't understand that command.");
                break;
        }
    }

    public async Task ShowRoom(Player player)
    {
        if (World.Rooms.TryGetValue(player.CurrentRoom, out var room))
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\n[ {room.Name.ToUpper()} ]");
            sb.AppendLine(room.Description);
            sb.AppendLine($"> Exits: {string.Join(", ", room.Exits.Keys)}");

            if (room.Items.Any())
                sb.AppendLine($"> Items: {string.Join(", ", room.Items)}");

            if (room.NPCs.Any())
                sb.AppendLine($"> NPCs: {string.Join(", ", room.NPCs.Keys)}");

            var others = _players.Where(p => p.CurrentRoom == player.CurrentRoom && p != player).Select(p => p.Name);
            if (others.Any())
                sb.AppendLine($"> Other knights here: {string.Join(", ", others)}");

            await player.Send(sb.ToString());
        }
        else
        {
            await player.Send($"[ERROR]: Room '{player.CurrentRoom}' not found in World.json!");
            Console.WriteLine($"[LOG]: Player {player.Name} is in a non-existent room: {player.CurrentRoom}");
        }
    }

    private async Task Move(Player player, string dir)
    {
        if (World.Rooms.TryGetValue(player.CurrentRoom, out var currentRoom))
        {
            if (currentRoom.Exits.TryGetValue(dir, out var next))
            {
                await Broadcast($"{player.Name} left to the {dir}.", player);
                player.CurrentRoom = next;
                await Broadcast($"{player.Name} has entered the room.", player);
                await ShowRoom(player);
            }
            else await player.Send("You can't go that way.");
        }
    }

    private async Task Take(Player player, string item)
    {
        if (World.Rooms.TryGetValue(player.CurrentRoom, out var room))
        {
            if (room.Items.Contains(item))
            {
                if (player.Inventory.Count < player.MaxInventory)
                {
                    room.Items.Remove(item);
                    player.Inventory.Add(item);
                    await player.Send($"You took the {item}.");
                }
                else await player.Send("Your bags are full!");
            }
            else await player.Send("You don't see anything like that here.");
        }
    }

    private async Task Drop(Player player, string item)
    {
        if (player.Inventory.Remove(item))
        {
            if (World.Rooms.TryGetValue(player.CurrentRoom, out var room))
            {
                room.Items.Add(item);
                await player.Send($"You dropped the {item}.");
            }
        }
        else await player.Send("You don't have that in your inventory.");
    }

    private async Task Talk(Player player, string npc)
    {
        if (World.Rooms.TryGetValue(player.CurrentRoom, out var room))
        {
            if (room.NPCs.TryGetValue(npc, out var text))
                await player.Send($"{npc} says: \"{text}\"");
            else await player.Send("There is no one by that name here.");
        }
    }
}