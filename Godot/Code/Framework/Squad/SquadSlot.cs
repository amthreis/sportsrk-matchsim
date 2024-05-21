using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public struct SquadSlot
    {
        public Player Player { get; }
        public int No { get; }
        public Godot.Vector2 Pos { get; set; }
        public int Index { get; }

        public SquadSlot(int idx, Player player, int no, Godot.Vector2 pos)
        {
            Index = idx;
            Player = player;
            No = no;
            Pos = pos;
        }

        public override int GetHashCode()
        {
            return Player.GetHashCode();
        }
    }
}
