using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameData.Models
{
    public class Game
    {
        public State State { get; set; }

        public Dictionary<string, string> UserRoles { get; set; }
    }
}
