using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public struct MatchesOTD
    {
        public List<MatchOTD> Matches { get; set; }

        public MatchesOTD(List<Match> matches)
        {
            //Matches = new List<MatchOTD>();

            Matches = matches.Select(m => new MatchOTD(m)).ToList();

            //foreach (var m in matches)
            //{
            //    Matches.Add(new MatchOTD(m));
            //}
            //this.Matches = matches;
        }
    }
}
