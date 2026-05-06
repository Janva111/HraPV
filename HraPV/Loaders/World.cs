using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HraPV.Loaders
{
    public static class World
    {
        public static Dictionary<string, Room> Rooms { get; private set; } = new();

        public static void LoadWorld(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                };

                Rooms = JsonSerializer.Deserialize<Dictionary<string, Room>>(jsonString, options) ?? new();
                Console.WriteLine($"Uspěšně načteno {Rooms.Count} místností.");
            }
            catch (JsonException jex)
            {
                Console.WriteLine($"Chyba ve struktuře JSONu: {jex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Obecná chyba: {ex.Message}");
            }
        }
    }
}
