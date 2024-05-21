using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Futzim
{
    public partial class MatchResolver : Node
    {
        //public List<object> matchDTOs = new List<object>();
        public event Action<List<Match>> Done;
        List<Match> matches;
        List<MatchOTD> matchOTDs;

        public MatchOTD PlaySingle(Match match)
        {
            //GD.Print($"home {match.Home.Slots.Count} x away {match.Away.Slots.Count}");
            while (match.State != MatchState.Post)
            {
                match.Advance(false);
            }

            return new MatchOTD(match);
        }

        public void Play(List<Match> m)
        {
            matches = m;
            matchOTDs = new List<MatchOTD>();

            foreach(var match in matches)
            {
                GD.Print($"home {match.Home.Slots.Count} x away {match.Away.Slots.Count}");
                while (match.State != MatchState.Post)
                {
                    match.Advance(false);
                }

                matchOTDs.Add(new MatchOTD(match));
            }

            Done?.Invoke(matches);
        }
    }
}
