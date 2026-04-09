using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HraPV
{
        public class Room
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public Dictionary<string, string> Exits { get; set; } = new();
            public List<string> Items { get; set; } = new();
            public Dictionary<string, string> NPCs { get; set; } = new();

            public Room(string name, string description)
            {
                Name = name;
                Description = description;
            }
        }
}
