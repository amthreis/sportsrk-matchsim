using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public struct MatchStats
    {
        public int Goals { get; set; }
        public int Assists { get; set; }

        public int FoulsCommitted { get; set; }
        public int FoulsWon { get; set; }

        public int YellowCards { get; set; }
        public int RedCards { get; set; }


        public int Shots { get; set; }
        public int ShotsOnTarget { get; set; }
        //public int Goals { get; set; }
        public int Blocks { get; set; }
        public int GKSaves { get; set; }
        public int PassesAttempted { get; set; }
        public int Passes { get; set; }

        public int Interceptions { get; set; }
        public int Tackles { get; set; }

        public int Aerials { get; set; }
        public int AerialsWon { get; set; }

        public int DribblesAttempted { get; set; }
        public int Dribbles { get; set; }

        public int CornerKicks { get; set; }
        public int Cleared { get; set; }

    }
}
