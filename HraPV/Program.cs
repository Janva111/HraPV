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
        TcpListener listener = new(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine($"[SERVER] Rytířský MUD spuštěn na portu {Port}...");

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
        using (var writer = new StreamWriter(stream, Encoding.UTF8))
        {
            Player? player = null;
            try
            {
                await writer.WriteLineAsync("Vítej v online středověku! Zadej své jméno:");
                await writer.FlushAsync();

                string? name = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(name)) return;

                player = new Player(client, writer) { Name = name.Trim() };
                Engine.AddPlayer(player);

                await Engine.Broadcast($"*** Rytíř {player.Name} vjel do hradu ***", player);
                await Engine.ShowRoom(player);

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.Trim().ToLower() == "konec") break;
                    await Engine.ProcessCommand(player, line);
                }
            }
            catch { /* Odpojení */ }
            finally
            {
                if (player != null)
                {
                    Engine.RemovePlayer(player);
                    await Engine.Broadcast($"*** {player.Name} odcválal pryč ***");
                }
            }
        }
    }
}