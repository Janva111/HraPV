using System.Net.Sockets;
using System.Text;

namespace HraPvClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var client = new TcpClient("127.0.0.1", 8888);
            using var stream = client.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            _ = Task.Run(async () => {
                while (true)
                {
                    string? msg = await reader.ReadLineAsync();
                    if (msg == null) break;
                    Console.WriteLine(msg);
                }
            });

            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;
                await writer.WriteLineAsync(input);
            }
        }
    }
}
