using HraPV.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HraPV.Commands;

public class Engine
{
    private readonly List<Player> _players = new();
    private readonly object _lock = new();

    // Specializovaní manažeři pro rozdělení logiky
    private readonly Combat _combat;
    private readonly Interaction _interaction;

    public Engine()
    {
        _combat = new Combat(this);
        _interaction = new Interaction();
    }

    public void AddPlayer(Player player) { lock (_lock) _players.Add(player); }
    public void RemovePlayer(Player player) { lock (_lock) _players.Remove(player); }

    public async Task ProcessCommand(Player player, string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        string cmd = parts[0].ToLower();
        string args = string.Join(" ", parts.Skip(1));

        switch (cmd)
        {
            case "look": await ShowRoom(player); break;
            case "go": await Move(player, args); break;
            case "take": await Take(player, args); break;
            case "drop": await Drop(player, args); break;
            case "inventory": case "inv": await ShowInventory(player); break;
            case "use":
                await _interaction.HandleUse(player, args);
                break;
            case "craft": await _interaction.HandleCraft(player, parts.Skip(1).ToList()); break;
            case "talk": await Talk(player, args); break;
            case "shop": await _interaction.HandleShop(player); break;
            case "buy": await _interaction.HandleBuy(player, args); break;
            case "attack": case "kill": await _combat.HandleAttack(player, args, _players, _lock); break;
            case "say": await Say(player, args); break;
            case "shout": await Shout(player, args); break;
            case "whisper": await PrivateMessage(player, args); break;

            case "help": await ShowHelp(player); break;

            default:
                await player.Send("Wrong command. Type 'help' for options.");
                break;
        }
    }

    public async Task ShowRoom(Player player)
    {
        if (World.Rooms.TryGetValue(player.Location, out var room))
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\n[ {room.Name.ToUpper()} ]");
            sb.AppendLine(room.Description);
            sb.AppendLine($"> Exits: {string.Join(", ", room.Exits.Keys)}");

            if (room.Items.Any()) sb.AppendLine($"> Items: {string.Join(", ", room.Items)}");
            if (room.NPCs.Any()) sb.AppendLine($"> Residents: {string.Join(", ", room.NPCs.Keys)}");

            var others = _players.Where(p => p.Location == player.Location && p != player).Select(p => p.Name);
            if (others.Any()) sb.AppendLine($"> Other knights here: {string.Join(", ", others)}");

            await player.Send(sb.ToString());
        }
    }

    private async Task Move(Player player, string dir)
    {
        if (World.Rooms.TryGetValue(player.Location, out var currentRoom))
        {
            if (currentRoom.Exits.TryGetValue(dir, out var next))
            {
                await BroadcastToLocation(player.Location, $"{player.Name} left towards the {dir}", player);
                player.Location = next;
                await BroadcastToLocation(player.Location, $"{player.Name} has arrived to {player.Location}", player);

                await ShowRoom(player);
                await CheckWinCondition(player);
            }
            else await player.Send("You can't go there.");
        }
    }

    private async Task Take(Player player, string item)
    {
        if (World.Rooms.TryGetValue(player.Location, out var room))
        {
            if (room.Items.Contains(item))
            {
                if (player.Inventory.Count < player.MaxInventory)
                {
                    room.Items.Remove(item);
                    player.Inventory.Add(item);
                    await player.Send($"You took the {item}.");
                }
                else await player.Send("Your inventory is full.");
            }
            else await player.Send("You can´t do that here.");
        }
    }

    private async Task Drop(Player player, string item)
    {
        if (player.Inventory.Remove(item))
        {
            if (World.Rooms.TryGetValue(player.Location, out var room))
            {
                room.Items.Add(item);
                await player.Send($"You dropped the {item}.");
                await CheckWinCondition(player);
            }
        }
        else await player.Send($"You don't have {item}.");
    }

    private async Task Say(Player player, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        await BroadcastToLocation(player.Location, $"{player.Name} says: {message}", player);
        await player.Send($"You say: {message}");
    }

    private async Task Shout(Player player, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        await Broadcast($"[SHOUT] {player.Name} yells: {message}");
    }

    private async Task PrivateMessage(Player player, string args)
    {
        var parts = args.Split(' ', 2);
        if (parts.Length < 2) { await player.Send("Usage: tell <name> <message>"); return; }

        Player target;
        lock (_lock) { target = _players.FirstOrDefault(p => p.Name.Equals(parts[0], StringComparison.OrdinalIgnoreCase)); }

        if (target != null)
        {
            await target.Send($"{player.Name} whispers to you: {parts[1]}");
            await player.Send($"You whisper to {target.Name}: {parts[1]}");
        }
        else await player.Send("That knight is not online.");
    }

    public async Task Broadcast(string msg, Player exclude = null)
    {
        List<Player> snapshot;
        lock (_lock) snapshot = _players.ToList();
        foreach (var p in snapshot) if (p != exclude) await p.Send(msg);
    }

    public async Task BroadcastToLocation(string loc, string msg, params Player[] exclude)
    {
        List<Player> targets;
        lock (_lock) { targets = _players.Where(p => p.Location == loc).ToList(); }
        foreach (var p in targets) if (!exclude.Contains(p)) await p.Send(msg);
    }

    private async Task ShowInventory(Player player)
    {
        await player.Send($"--- INVENTORY ({player.Inventory.Count}/{player.MaxInventory}) ---");
        await player.Send(player.Inventory.Any() ? string.Join(", ", player.Inventory) : "Empty.");
        await player.Send($"Gold: {player.Gold} | Health: {player.Health}/{player.MaxHealth}");
    }

    private async Task Talk(Player player, string npcName)
    {
        if (World.Rooms.TryGetValue(player.Location, out var room) && room.NPCs.TryGetValue(npcName, out var dialog))
            await player.Send($"{npcName} says: \"{dialog}\"");
        else await player.Send("Nobody here by that name.");
    }

    private async Task ShowHelp(Player player)
    {
        await player.Send("\nAvailable commands:");
        await player.Send("GO <north/south...>, LOOK, TAKE <item>, DROP <item>, USE <item>, CRAFT <items>");
        await player.Send("SAY <msg>, SHOUT <msg>, TELL <player> <msg>, ATTACK/KILL <player>");
        await player.Send("SHOP, BUY <item>, INVENTORY, HELP, QUIT");
    }

    private async Task CheckWinCondition(Player player)
    {
        if (player.Location == "throne_room" && player.Inventory.Contains("holy_grail"))
        {
            await player.Send("\n*** VICTORY! You have delivered the Grail! ***\n");
            Logger.Log($"[WIN] {player.Name} won!");
        }
    }
}