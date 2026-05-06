using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HraPV
{
    public class Npc
    {
        public string Dialog { get; set; } = "";
        public Dictionary<string, int> Shop { get; set; } = new();
    }
}
