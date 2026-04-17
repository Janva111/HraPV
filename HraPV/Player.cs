using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HraPV;

public class Player
{
    public string Name { get; set; } = "";
    public string CurrentRoom { get; set; } = "courtyard";
    public List<string> Inventory { get; } = new();
    public int MaxInventory { get; } = 3;

    private readonly StreamWriter _writer;
    private readonly TcpClient _client;

    public Player(TcpClient client, StreamWriter writer)
    {
        _client = client;
        _writer = writer;
    }

    public async Task Send(string message)
    {
        try
        {
            await _writer.WriteLineAsync(message);
            await _writer.FlushAsync();
        }
        catch {}
    }
}
