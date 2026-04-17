using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HraPV
{
    public static class World
    {
        public static Dictionary<string, Room> Rooms { get; private set; } = new();

        public static void LoadWorld(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                string jsonString = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loadedRooms = JsonSerializer.Deserialize<Dictionary<string, Room>>(jsonString, options);

                if (loadedRooms != null)
                {
                    Rooms = loadedRooms;
                    Console.WriteLine("World successfully loaded from JSON.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while loading the world: {ex.Message}");
            }
        }
    }
}
