using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameData.Models.TicTacToe
{
    public class TicTacToeState : State
    {
        public string[] Board { get; set; }

        public bool IsXTurn { get; set; }
    }
}
