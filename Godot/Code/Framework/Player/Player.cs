using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public enum PlayerPos { GK, FB, CB, DM, LM, AM, WG, ST }

    public class Player
    {
        public string UserId { get; set; }
        public string KnownAs {  get; set; }
        public PlayerAttrib Attrib { get; set; }
        public PlayerPos Pos { get; set; }
        public int MMR { get; set; }

        public Player(string knownAs)
        {
            KnownAs = knownAs;
            Attrib = new PlayerAttrib();
        }
        public Player(string knownAs,  PlayerAttrib attrib)
        {
            KnownAs = knownAs;
            Attrib = attrib;
        }
        public Player(string id, string knownAs, PlayerPos pos, PlayerAttrib attrib, int mmr)
        {
            UserId = id;    
            KnownAs = knownAs;
            Pos = pos;
            Attrib = attrib;
            MMR = mmr;
        }
    }
}
