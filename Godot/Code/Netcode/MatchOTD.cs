using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public struct MatchOTD
    {
        public string Id { get; }
        public MatchStats Home { get; }
        public MatchStats Away { get; }
        public List<MatchEvent> Events { get; }

        public List<PlayerOTD> Players { get;  }

        public MatchOTD(Match m)
        {
            Id = m.Id;

            Home = m.Home.Stats;
            Away = m.Away.Stats;

            Players = new List<PlayerOTD>();

            var homeAvg = m.Home.Slots.Average(s => s.Source.Player.MMR);
            var awayAvg = m.Home.Slots.Average(s => s.Source.Player.MMR);
            var avg = (homeAvg + awayAvg) /2f;

            var maxH = m.Home.Slots.Max(s => s.Source.Player.MMR);
            var minH = m.Home.Slots.Min(s => s.Source.Player.MMR);

            var maxA = m.Away.Slots.Max(s => s.Source.Player.MMR);
            var minA = m.Away.Slots.Min(s => s.Source.Player.MMR);



            var max = Mathf.Max(maxH, maxA);
            var min = Mathf.Min(minH, minA);

            var lowestAmp = Mathf.Abs(max - avg) > Mathf.Abs(min - avg) ? min : max;
            var isLowestMin = lowestAmp < avg;
            
            var ampRef = (avg - lowestAmp) * 2;
            var ampMin = isLowestMin ? lowestAmp : avg + ampRef * 0.5;
            var ampMax = isLowestMin ? avg + ampRef * 0.5 : lowestAmp;

            var refo = ampRef * 0.25f;

            foreach (var p in m.Home.Slots)
            {
                var pos = (Mathf.InverseLerp(ampMin, ampMax, p.Source.Player.MMR) - 0.5) * 2;
                //var diff = (int)(avg - p.Source.Player.MMR);
                var mmrIncr = 0;


                var w = refo *Mathf.Pow(1.5f, (0.5 - pos) * 1.25f);
                var l = refo *Mathf.Pow(1.5f, (pos + 0.5) * 1.25f);

                var d = (w - l) / 2;
                
                
                //home wins
                if (m.Home.Stats.Goals > m.Away.Stats.Goals)
                {
                    mmrIncr = (int)w;
                }
                //home loses
                else if (m.Home.Stats.Goals < m.Away.Stats.Goals)
                {
                    mmrIncr = (int)l;
                }
                //draw
                else
                {
                    mmrIncr = (int)d;
                }


                //var ratio = p.Source.Player.MMR / avg;

                // W = +100
                // D =    0
                // L = -100

                Players.Add(new PlayerOTD(p, mmrIncr));
            }
            foreach (var p in m.Away.Slots)
            {
                var pos = (Mathf.InverseLerp(ampMin, ampMax, p.Source.Player.MMR) - 0.5) * 2;
                //var diff = (int)(avg - p.Source.Player.MMR);
                


                var w = refo * Mathf.Pow(1.5f, (0.5 - pos) * 1.25f);
                var l = refo * Mathf.Pow(1.5f, (pos + 0.5) * 1.25f);

                var d = (w - l) / 2;
                var mmrIncr = 0;

                //away wins
                if (m.Away.Stats.Goals > m.Home.Stats.Goals)
                {
                    mmrIncr = (int)w;
                }
                //away loses
                else if (m.Away.Stats.Goals < m.Home.Stats.Goals)
                {
                    mmrIncr = (int)l;
                }
                //draw
                else
                {
                    mmrIncr = (int)d;
                }

                Players.Add(new PlayerOTD(p, mmrIncr));
            }

            Events = m.Events;
        }
    }
}
