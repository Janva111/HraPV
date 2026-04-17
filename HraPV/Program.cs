using HraPV;
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

        TcpListener listener = new(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"[SERVER] Knightly MUD started on port {Port}...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            // Handle each client in a separate thread (Task)
            _ = Task.Run(() => HandleClient(client));
        }
    }

    private static async Task HandleClient(TcpClient client)
    {
        using (client)
        using (var stream = client.GetStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8))
        using (var writer = new StreamWriter(stream, Encoding.UTF8))
        {
            Player? player = null;
            try
            {
                await writer.WriteLineAsync("Welcome to the Medieval Online! Enter your name:");
                await writer.FlushAsync();

                string? name = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(name)) return;

                player = new Player(client, writer) { Name = name.Trim() };
                Engine.AddPlayer(player);

                await Engine.Broadcast($"*** Knight {player.Name} has entered the castle ***", player);
                await Engine.ShowRoom(player);

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    string command = line.Trim().ToLower();

                    if (command == "quit" || command == "exit") break;

                    await Engine.ProcessCommand(player, line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Connection lost with a player: {ex.Message}");
            }
            finally
            {
                if (player != null)
                {
                    Engine.RemovePlayer(player);
                    await Engine.Broadcast($"*** {player.Name} has ridden away ***");
                }
            }
        }
    }
}