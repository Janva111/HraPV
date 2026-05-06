using HraPV;
using HraPV.Commands;
using HraPV.Loaders;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    private static readonly Engine Engine = new();
    private const int Port = 8888;

    static async Task Main()
    {
        World.LoadWorld("World.json");
        Crafting.LoadRecipes();

        TcpListener listener = new(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"[SERVER] Server started on port {Port}...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClient(client));
        }
    }

    private static async Task HandleClient(TcpClient client)
{
    using (client)
    using (var stream = client.GetStream())
    using (var reader = new StreamReader(stream, Encoding.UTF8))
    using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true, NewLine = "\r\n" })
    {
        Player? player = null;
        try
        {
            await writer.WriteLineAsync("Welcome to the Medeavle game");
            await writer.WriteLineAsync("Please log in to continue.");
                await writer.WriteLineAsync("--- LOGIN ---");
            await writer.WriteLineAsync("Enter Username:");
            string? username = await reader.ReadLineAsync();
            
            await writer.WriteLineAsync("Enter Password:");
            string? password = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) 
                return;

            username = username.Trim();

            if (!AuthService.Authenticate(username, password, out var account))
            {
                await writer.WriteLineAsync("Invalid password! Access denied.");
                Logger.Log($"[AUTH] Failed login attempt for user: {username}");
                return;
            }

            await writer.WriteLineAsync($"Welcome back, {username}! Your task is to get the Holy Grail and bring it back to your king.");

                player = new Player(client, writer) 
            { 
                Name = username,
                Location = account!.CurrentLocation,
                Inventory = account.Inventory 
            };

            Engine.AddPlayer(player);

            Logger.Log($"[PLAYER] {player.Name} joined the game at {player.Location}.");

            await Engine.Broadcast($"*** Knight {player.Name} has entered the castle ***", player);
            await Engine.ShowRoom(player);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                string command = line.Trim().ToLower();
                
                Logger.Log($"[COMMAND] {player.Name}: {command}");

                if (command == "quit" || command == "exit") break;
                
                await Engine.ProcessCommand(player, line);
                AuthService.SaveProgress(player.Name, player.Location, player.Inventory);
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"[ERROR] Connection error with {player?.Name ?? "Unknown"}: {ex.Message}");
        }
        finally
        {
            if (player != null)
            {
                AuthService.SaveProgress(player.Name, player.Location, player.Inventory);
                
                Engine.RemovePlayer(player);
                await Engine.Broadcast($"*** {player.Name} has ridden away ***");
                Logger.Log($"[PLAYER] {player.Name} disconnected.");
            }
        }
    }
}
}