using System.Net.Sockets;

namespace HraPV;

public class Player
{
    public string Name { get; set; } = "";
    public string Location { get; set; } = "courtyard";
    public List<string> Inventory { get; set; } = new();
    public int MaxInventory { get; } = 10;

    public int Gold { get; set; } = 0;

    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;

    public bool IsAlive => Health > 0;

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
            if (_writer != null)
            {
                await _writer.WriteLineAsync(message);
                await _writer.FlushAsync();
            }
        }
        catch{}
    }

    public void RestoreHealth()
    {
        Health = MaxHealth;
    }
}