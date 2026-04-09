using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HraPV
{
    public static class World
    {
        public static readonly Dictionary<string, Room> Rooms = new()
        {
            ["nádvoří"] = new Room("Hradní nádvoří", "Stojíš na dlážděném nádvoří. Nad tebou se tyčí věže hradu.")
            {
                Exits = new() { ["sever"] = "trůnní sál", ["východ"] = "zbrojnice" },
                Items = new() { "rezavý_meč" },
                NPCs = new() { ["strážný"] = "Vítej! Hlídej si svůj měšec." }
            },
            ["trůnní sál"] = new Room("Trůnní sál", "Velkolepá síň ozářená svícny.")
            {
                Exits = new() { ["jih"] = "nádvoří" },
                Items = new() { "zlatý_pohár" }
            },
            ["zbrojnice"] = new Room("Zbrojnice", "Místnost plná prázdných brnění a stojanů.")
            {
                Exits = new() { ["západ"] = "nádvoří" },
                Items = new() { "štít" }
            }
        };
    }
}
