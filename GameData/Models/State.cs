using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace GameData.Models
{
    [KnownType(typeof(TicTacToe.TicTacToeState))]
    public abstract class State
    {
    }
}
