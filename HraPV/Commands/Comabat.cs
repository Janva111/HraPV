using HraPV.Loaders;

namespace HraPV.Commands;

public class Combat
{
    private readonly Engine _engine;
    public Combat(Engine engine) => _engine = engine;

    public async Task HandleAttack(Player player1, string player2Name, List<Player> players, object lockObj)
    {
        if (string.IsNullOrWhiteSpace(player2Name))
        {
            var others = players
                .Where(p => p.Location == player1.Location && p != player1)
                .Select(p => p.Name)
                .ToList();

            if (others.Any())
            {
                await player1.Send($"(Usage: attack <player_name>)\nAvailable targets: {string.Join(", ", others)}");
            }
            else
            {
                await player1.Send("(Usage: attack <player_name>)\nThere is no one else here to fight.");
            }
            return;
        }

        Player player2;
        lock (lockObj)
        {
            player2 = players.FirstOrDefault(p => p.Name.ToLower() == player2Name.ToLower() && p.Location == player1.Location && p != player1);
        }

        if (player2 == null) { await player1.Send($"Knight '{player2Name}' is not here."); return; }

        int damage = new Random().Next(10, 31);
        player2.Health -= damage;

        await player1.Send($"You attack {player2.Name} and dealed {damage} damage!");
        await player2.Send($"*** {player1.Name} attacks and deals you {damage} damage! ***");
        await _engine.BroadcastToLocation(player1.Location, $"{player1.Name} viciously attacks {player2.Name}!", player1, player2);

        if (player2.Health <= 0) await HandleDeath(player2, player1);
    }

    private async Task HandleDeath(Player player2, Player player1)
    {
        await player2.Send("\n[ YOU HAVE DIED ]\nYour vision fades to black...");
        await _engine.Broadcast($"[SYSTEM] The brave knight {player2.Name} has been defeated in combat against {player1.Name}.");

        if (World.Rooms.TryGetValue(player2.Location, out var room))
        {
            foreach (var item in player2.Inventory) room.Items.Add(item);
            player2.Inventory.Clear();
        }

        player2.RestoreHealth();
        player2.Location = "courtyard";
        player1.Gold += player2.Gold / 4;
        await player1.Send($"You received {player2.Gold / 4} Gold for defeating {player2.Name}!");
        await _engine.ShowRoom(player2);
    }
}