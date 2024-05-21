using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace Futzim
{
    public class Club
    {
        public string KnownAs { get; set; }

        public Squad ProSquad { get; set; }
        public Color ColorA { get; set; }

        public Club(string knownAs, Color colorA) 
        {
            KnownAs = knownAs;
            ColorA = colorA;

            ProSquad = new Squad(this);
        }
    }
}
