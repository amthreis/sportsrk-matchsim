using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Futzim
{
    public enum MatchTeamDecisionOutcome { Hold, LoseTime, Advance, ThroughBall, Dribble, Clear }
    public enum MatchTeamFinishOutcome { OnTarget, Block, Miss }

    public enum PassType { LoseTime, Advance, ThroughBall, Clear }
    public enum MatchTeamPassOutcome { Success, Miss, Interception, InterceptionWithCA, GetFouled }
    public enum MatchTeamDribbleOutcome { Success, OppTackle, OppTackleWithCA, GetFouled }
    public enum MatchTeamAerialOutcome { Win, Lose, GetFouled }
    public enum MatchTeamFoulOutcome { None, YellowCard, RedCard }

    public class MatchTeam
    {
        public Match Match { get; }
        public string Name;
        public MatchTeam Opp { get; set; }


        public List<MatchSlot> Slots { get; } = new List<MatchSlot>();
        public IEnumerable<MatchSlot> PlayingSlots => Slots.Where(s => !s.IsOut);

        public bool IsHome { get; }

        public int Possession { get; set; }

        public int PassesAttempted { get; set; }
        public int Passes { get; set; }


        public Dictionary<MatchTeamDecisionOutcome, int> PassesAttemptedByType { get; private set; } = new Dictionary<MatchTeamDecisionOutcome, int>()
        {
            { MatchTeamDecisionOutcome.ThroughBall, 0 },
            { MatchTeamDecisionOutcome.Advance, 0 },
            { MatchTeamDecisionOutcome.LoseTime, 0 }
        };
        public Dictionary<MatchTeamDecisionOutcome, int> PassesByType { get; private set; } = new Dictionary<MatchTeamDecisionOutcome, int>()
        {
            { MatchTeamDecisionOutcome.ThroughBall, 0 },
            { MatchTeamDecisionOutcome.Advance, 0 },
            { MatchTeamDecisionOutcome.LoseTime, 0 }
        };

        public PlayerAttrib Attrib;

        public float Ptc;

        public void CalcAttrib()
        {
            foreach (var s in PlayingSlots)
            {
                s.CalcAttrib(Match.Ball);
            }

            Attrib.Finishing = PlayingSlots.Sum(s => s.Attrib.Finishing);
            Attrib.Dribbling = PlayingSlots.Sum(s => s.Attrib.Dribbling);
            Attrib.Passing = PlayingSlots.Sum(s => s.Attrib.Passing);

            Attrib.Pace = PlayingSlots.Sum(s => s.Attrib.Pace);

            Attrib.Marking = PlayingSlots.Sum(s => s.Attrib.Marking);
            Attrib.Aerial = PlayingSlots.Sum(s => s.Attrib.Aerial);

            Ptc = PlayingSlots.Average(s => s.Ptc);

           // GD.Print("dr ", Attrib.Dribbling);
        }

        public MatchStats Stats;

        public Squad Squad { get; }

        public MatchTeam(Match m, Squad squad, bool isHome)
        {
            Squad = squad;
            Name = squad.Club.KnownAs;
            Match = m;
            IsHome = isHome;

            //Slots = new List<MatchSlot>(Squad.Slots);

            foreach (var s in Squad.Slots)
            {
                Slots.Add(new MatchSlot(this, s));
            }

            //GD.Print("slots ", Slots.Count);
        }

        bool GetPassOutcome(MatchSlot player, PassType type)
        {
            switch (type)
            {
                case PassType.LoseTime:
                    {
                        return GetOutcome("lose time",
                            (true, 1200 + Mathf.Pow(25f * player.Attrib.Passing, 1.67f)),
                            (false, 300 + Mathf.Pow(Opp.Attrib.Pace + Opp.Attrib.Marking * 2f, 1.25f))
                        );
                    }

                case PassType.Advance:
                    {
                        return GetOutcome("pass",
                            (true, 1200 + Mathf.Pow(20f * player.Attrib.Passing * 8f, 1.67f)),
                            (false, 300 + Mathf.Pow(Opp.Attrib.Pace + Opp.Attrib.Marking, 1.25f))
                        );
                    }

                case PassType.ThroughBall:
                    {
                        return GetOutcome("through ball",
                            (true, 200 + Mathf.Pow(15f * player.Attrib.Passing, 1.6f)),
                            (false, 300 + Mathf.Pow(Opp.Attrib.Pace * 2f + Opp.Attrib.Marking, 1.25f))
                        );
                    }

                case PassType.Clear:
                    {
                        return GetOutcome("clear",
                            (true, 200 + Mathf.Pow(15f * player.Attrib.Passing, 1.6f)),
                            (false, 300 + Mathf.Pow(Opp.Attrib.Pace * 1f + Opp.Attrib.Marking, 0.65f))
                        );
                    }
            }

            return false;
        }

        float passMx = 0.4f;

        float GetPassAdvance(MatchSlot player, PassType type)
        {
            switch (type)
            {
                case PassType.LoseTime:
                    {
                        return App.RNG.RandfRange(-0.03f, 0.03f) * passMx;
                    }

                case PassType.Advance:
                    {
                        return App.RNG.RandfRange(0.08f, 0.2f) * 1.2f * passMx;
                    }

                case PassType.ThroughBall:
                    {
                        return App.RNG.RandfRange(0.15f, 0.3f) * passMx;
                    }

                case PassType.Clear:
                    {
                        return App.RNG.RandfRange(0.2f, 0.35f) * passMx;
                    }
            }

            return 0f;
        }

        void ResolvePass(PassType type)
        {


            PassesAttempted++;
            PassesAttemptedByType[MatchTeamDecisionOutcome.Advance]++;

            var playersByPassingChance = PlayingSlots.ToDictionary(s => s, s => Mathf.Pow(2 + s.Attrib.Passing * s.Ptc, 2.25f));

            var whoPassed = GetPlayerFromList("who passed", playersByPassingChance);

            whoPassed.Stats.PassesAttempted++;

            var passed = GetPassOutcome(whoPassed, type);


            if (passed)
            {
                var value = GetPassAdvance(whoPassed, type);

                DLPrint($"Advance {value.RdPerc()}");

                Passes++;
                PassesByType[MatchTeamDecisionOutcome.Advance]++;

                whoPassed.Stats.Passes++;

                var isGameOn = GoAhead(value);

                if (type == PassType.Clear)
                {
                    Stats.Cleared++;

                    if (isGameOn)
                        ResolveAerial();
                }
            }
            else
            {
                var playersByMarkingAbility = Opp.PlayingSlots.ToDictionary(s => s, s => Mathf.Pow((5 + s.Attrib.Marking * 1.25f + s.Attrib.Pace) * s.Ptc, 2.25f));

                var whoIntercepted = GetPlayerFromList("who intercepted", playersByMarkingAbility);

                Opp.Stats.Interceptions++;
                whoIntercepted.Stats.Interceptions++;

                Match.GiveBallToOpp();
                DLPrint($"Missed pass({type}). Intercepted by {whoIntercepted.Source.Player.KnownAs}");
            }
        }

        void ResolveDribble()
        {
            var playersByPassingChance = PlayingSlots.ToDictionary(s => s, s => Mathf.Pow(2 + s.Attrib.Dribbling * s.Ptc, 2.25f));
            var oppPlayersByMarkingAbility = Opp.PlayingSlots.ToDictionary(s => s, s => Mathf.Pow(2 + s.Attrib.Marking * s.Ptc, 2.25f));

            var whoDribbled = GetPlayerFromList("who dribbled", playersByPassingChance);
            var whoMarked = GetPlayerFromList("who marked", oppPlayersByMarkingAbility);

            Stats.DribblesAttempted++;
            whoDribbled.Stats.DribblesAttempted++;

            var outcome = GetOutcome("dribbled past",
                (MatchTeamDribbleOutcome.Success, Mathf.Pow(whoDribbled.Attrib.Dribbling, 1.25f)),
                (MatchTeamDribbleOutcome.OppTackle, Mathf.Pow((whoMarked.Attrib.Marking + whoMarked.Attrib.Pace) * 0.7f, 1.1f)),
                (MatchTeamDribbleOutcome.GetFouled, Mathf.Pow((15f - whoMarked.Attrib.Mentality) * 0.25f, 1.2f))
            );

            switch(outcome)
            {
                case MatchTeamDribbleOutcome.Success:
                    {
                        var value = App.RNG.RandfRange(0.1f, 0.2f) * Mathf.Pow(whoDribbled.Attrib.Pace, 1.5f) * 0.1f;

                        DLPrint($"{whoDribbled.Source.Player.KnownAs} dribbled past {whoMarked.Source.Player.KnownAs}, advancing {value.RdPerc()}");

                        Stats.Dribbles++;
                        whoDribbled.Stats.Dribbles++;

                        GoAhead(value);
                    }
                    break;

                case MatchTeamDribbleOutcome.OppTackle:
                    {
                        Opp.Stats.Tackles++;
                        whoMarked.Stats.Tackles++;

                        Match.GiveBallToOpp();
                        DLPrint($"{whoMarked.Source.Player.KnownAs} tackled {whoDribbled.Source.Player.KnownAs}");
                    }
                    break;

                case MatchTeamDribbleOutcome.GetFouled:
                    {
                        ResolveFoul(whoDribbled, whoMarked);
                    }
                    break;
            }
        }

        void ResolveFoul(MatchSlot whoWon, MatchSlot whoCommitted)
        {
            Stats.FoulsWon++;
            whoWon.Stats.FoulsWon++;

            Opp.Stats.FoulsCommitted++;
            whoCommitted.Stats.FoulsCommitted++;

            var foulOutcome = GetOutcome("dribbled past",
                (MatchTeamFoulOutcome.None, Mathf.Pow(whoWon.Attrib.Mentality * 3f + whoCommitted.Attrib.Pace + whoCommitted.Attrib.Marking, 1.25f)),
                (MatchTeamFoulOutcome.YellowCard, Mathf.Pow(whoWon.Attrib.Mentality * 3f + whoCommitted.Attrib.Pace + whoCommitted.Attrib.Marking, 1.25f) * 0.75f - (whoCommitted.CardState == MatchSlotCardState.Y ? 0.7f : 0f)),
                (MatchTeamFoulOutcome.RedCard, Mathf.Pow(whoWon.Attrib.Mentality * 3f + whoCommitted.Attrib.Pace + whoCommitted.Attrib.Marking, 1.25f) * 0.05f)
            );

            switch (foulOutcome)
            {
                case MatchTeamFoulOutcome.None:
                    {

                    }
                    break;

                case MatchTeamFoulOutcome.YellowCard:
                    {
                        if (PlayingSlots.Count() > 7)
                        {
                            whoCommitted.YellowCard();

                            Match.Events.Add(new MatchEvent((int)Match.Minutes, whoCommitted.Source.Player.UserId,
                                Opp.IsHome ? "HOME" : "AWAY", whoCommitted.CardState == MatchSlotCardState.YY ? MatchEventType.YELLOWREDCARD : MatchEventType.YELLOWCARD));
                            YellowCard?.Invoke(whoCommitted, Match.Minutes);
                        }
                    }
                    break;

                case MatchTeamFoulOutcome.RedCard:
                    {
                        if (PlayingSlots.Count() > 7)
                        {
                            whoCommitted.RedCard();
                            Match.Events.Add(new MatchEvent((int)Match.Minutes, whoCommitted.Source.Player.UserId,
                                Opp.IsHome ? "HOME" : "AWAY", MatchEventType.REDCARD));
                            RedCard?.Invoke(whoCommitted, Match.Minutes);
                        }
                    }
                    break;
            }

            DLPrint($"{whoCommitted.Source.Player.KnownAs} fouled {whoWon.Source.Player.KnownAs}");

            if (whoCommitted.IsOutOfGame())
            {
                DLPrint($"{whoCommitted.Source.Player.KnownAs} got sent off: {whoCommitted.CardState}");
                whoCommitted.IsOut = true;
            }
        }

        void ResolveHold()
        {
            var held = GetOutcome("hold",
                (true, 5000),
                (false, 300 + Mathf.Pow(Opp.Attrib.Pace, 1.25f))
            );

            if (!held)
            {
                var playersByMarkingAbility = Opp.PlayingSlots.ToDictionary(s => s, s => Mathf.Pow((5 + s.Attrib.Marking * 1.25f + s.Attrib.Pace * 0.4f) * s.Ptc, 2.25f));

                var whoStole = GetPlayerFromList("who stole it", playersByMarkingAbility);

                Opp.Stats.Tackles++;
                whoStole.Stats.Tackles++;

                DLPrint($"Lost the ball holding. Stolen by {whoStole}");
                Match.GiveBallToOpp();
            }
        }

        public void Play()
        {
            DLPrint($"{Name}", color: new Color("959595"));
            Possession++;

            var decision = GetOutcome("decision",
                (MatchTeamDecisionOutcome.Hold, 1500),
                (MatchTeamDecisionOutcome.LoseTime, 1200),
                (MatchTeamDecisionOutcome.Advance, 500 + Mathf.Pow(Attrib.Passing, 1.5f) * 0.25f),
                (MatchTeamDecisionOutcome.ThroughBall, 100 + Mathf.Pow(Attrib.Passing, 1.75f) * 0.25f),
                (MatchTeamDecisionOutcome.Dribble, 20 + Mathf.Pow(Attrib.Dribbling, 1.6f)),
                (MatchTeamDecisionOutcome.Clear, 250 * (1f - HowAhead()))
            );

            switch (decision)
            {
                case MatchTeamDecisionOutcome.Hold:
                    {
                        ResolveHold();
                    }
                    break;

                case MatchTeamDecisionOutcome.LoseTime:
                    {
                        ResolvePass(PassType.LoseTime);
                    }
                    break;

                case MatchTeamDecisionOutcome.Advance:
                    {
                        ResolvePass(PassType.Advance);
                    }
                    break;

                case MatchTeamDecisionOutcome.ThroughBall:
                    {
                        ResolvePass(PassType.ThroughBall);
                    }
                    break;

                case MatchTeamDecisionOutcome.Dribble:
                    {
                        ResolveDribble();
                    }
                    break;

                case MatchTeamDecisionOutcome.Clear:
                    {
                        ResolvePass(PassType.Clear);
                    }
                    break;
            }
        }

        [Conditional("DEBUG")]
        void DLClear()
        {
            //if (Match.IsDebugging)
            Match.Debugger?.ClearDL();
        }

        [Conditional("DEBUG")]
        void DLPrint(string message, bool right = false, Color? color = null)
        {
            Match.Debugger?.PrintDL(message, right, color);
        }

        [Conditional("DEBUG")]
        void DLSection(string message, int randomNo, int combinedWeight)
        {
            Match.Debugger?.SectionDL(message, randomNo, combinedWeight);
        }

        [Conditional("DEBUG")]
        void Log(params object[] what)
        {
            if (Match.Debugger != null)
                GD.Print(what);
        }

        float HowAhead()
        {
            return IsHome ? Match.Ball : 1f - Match.Ball;
        }

        bool GoAhead(float value, bool isClear = false)
        {
            var prev = Match.Ball;
            Match.Ball = Mathf.Clamp(Match.Ball + value * (IsHome ? 1f : -1f), 0, 1);

            Log($"{prev.RdPerc()} {(IsHome ? "+" : "-")} {value.RdPerc()} = {Match.Ball.RdPerc()}");

            if (IsHome)
            {
                if (Match.Ball >= 1f)
                {
                    if (isClear)
                    {
                        Match.GiveBallToOpp();
                        GoAhead(.2f);
                        return false;
                    }

                    var isSideAttack = GetOutcome("isSideAttack",
                        (true, Attrib.Pace),
                        (false, Attrib.Passing * 1.25f)
                    );

                    ResolveAttack(isSideAttack);
                    return true;
                }
            }
            else
            {
                if (Match.Ball <= 0f)
                {
                    if (isClear)
                    {
                        Match.GiveBallToOpp();
                        GoAhead(.2f);
                        return false;
                    }
                    var isSideAttack = GetOutcome("isSideAttack",
                        (true, Attrib.Pace),
                        (false, Attrib.Passing * 1.25f)
                    );

                    ResolveAttack(isSideAttack);
                    return true;
                }
            }

            return true;
        }

        public event Action<MatchSlot, float> Scored;
        public event Action<MatchSlot, float> YellowCard, RedCard;

        void ResolveShotOnTarget(MatchSlot shooter)
        {

            Stats.ShotsOnTarget++;

            var gkDef = GetOutcome("gk defense",
                (true, shooter.Attrib.Finishing),
                (false, Opp.Slots[0].Attrib.Goalkeeping * 2f)
            );

            if (gkDef)
            {
                Opp.Stats.GKSaves++;
                Opp.Slots[0].Stats.GKSaves++;

                GoAhead(-0.4f);
                DLPrint("GK saved");
            }
            else
            {
            //    var wasAssisted = GetOutcome("was assisted",
            //        (true, shooter.Attrib.Finishing),
            //        (false, Opp.Slots[0].Attrib.Goalkeeping * 2f)
            //    );

                shooter.Stats.Goals++;
                Stats.Goals++;

                Match.Events.Add(new MatchEvent((int)Match.Minutes, shooter.Source.Player.UserId, IsHome ? "HOME" : "AWAY", MatchEventType.GOAL));
                Match.EmitSignal(MatchBreakpoint.Goal);

                DLPrint("GOAL!");

                Match.Ball = 0.5f;
                Match.GiveBallToOpp();

                Scored?.Invoke(shooter, Match.Minutes);
            }
        }

        void ResolveAttack(bool isHeader, MatchSlot whoShot = null)
        {
            Stats.Shots++;

            var shotOutcome = isHeader ?
                 GetOutcome("header",
                    (MatchTeamFinishOutcome.OnTarget, Attrib.Aerial * 100),
                    (MatchTeamFinishOutcome.Block, Opp.Attrib.Marking * 60 + Opp.Attrib.Aerial * 50),
                    (MatchTeamFinishOutcome.Miss, 1500))
                : GetOutcome("shot",
                    (MatchTeamFinishOutcome.OnTarget, Attrib.Finishing * 100),
                    (MatchTeamFinishOutcome.Block, Opp.Attrib.Marking * 60 + Opp.Attrib.Pace * 50),
                    (MatchTeamFinishOutcome.Miss, 1500));


            if (whoShot == null)
            {
                var playersByAerial = PlayingSlots.ToDictionary(s => s, s => Mathf.Pow(2 + s.Attrib.Aerial * s.Ptc, 2.25f));
                whoShot = GetPlayerFromList("who shot", playersByAerial);
            }

            whoShot.Stats.Shots++;

            Match.EmitSignal(MatchBreakpoint.Shot);

            switch (shotOutcome)
            {
                case MatchTeamFinishOutcome.OnTarget:
                    {
                        Stats.ShotsOnTarget++;
                        whoShot.Stats.ShotsOnTarget++;

                        ResolveShotOnTarget(whoShot);
                    }
                    break;

                case MatchTeamFinishOutcome.Miss:
                    {
                        GoAhead(-0.25f);
                        DLPrint("Missed the shot");
                    }
                    break;

                case MatchTeamFinishOutcome.Block:
                    {
                        Opp.Stats.Blocks++;
                        DLPrint("Shot blocked by the defense");

                        var playersByMarking = Opp.PlayingSlots.ToDictionary(s => s, s => Mathf.Pow(2 + s.Attrib.Marking * s.Attrib.Pace * s.Ptc, 2.25f));
                        var whoBlocked = GetPlayerFromList("who blocked", playersByMarking);

                        whoBlocked.Stats.Blocks++;


                        var ckOutcome = GetOutcome("got a corner kick",
                            (false, Opp.Ptc),
                            (true, Ptc)
                        );

                        if (ckOutcome)
                        {
                            ResolveCornerKick();
                        }
                        else
                        {
                            GoAhead(-0.12f);
                            Match.GiveBallToOpp();
                        }
                    }
                    break;
            }


        }

        void ResolveAerial()
        {
            var playersByAA = PlayingSlots.ToDictionary(p => p, p => 10 + p.Attrib.Aerial);
            var myJumper = GetPlayerFromList("who jumped", playersByAA);

            var oppPlayersByAA = Opp.PlayingSlots.ToDictionary(p => p, p => 12 + p.Attrib.Aerial * 2f);
            var oppJumper = GetPlayerFromList("opp who jumped", oppPlayersByAA);

            var aerialOutcome = GetOutcome("aerial",
                (MatchTeamAerialOutcome.Win, Mathf.Pow(myJumper.Attrib.Aerial, 2f)),
                (MatchTeamAerialOutcome.Lose, Mathf.Pow(oppJumper.Attrib.Aerial, 2f)),
                (MatchTeamAerialOutcome.GetFouled, 2 + Mathf.Pow(Math.Max(0, myJumper.Attrib.Mentality - myJumper.Attrib.Mentality * 0.35f) * 6f, 1.4f))
                );


            myJumper.Stats.Aerials++;
            oppJumper.Stats.Aerials++;

            switch (aerialOutcome)
            {
                case MatchTeamAerialOutcome.GetFouled:
                    {
                        ResolveFoul(myJumper, oppJumper);
                    }
                    break;

                case MatchTeamAerialOutcome.Win:
                    {
                        Stats.AerialsWon++;
                        myJumper.Stats.AerialsWon++;

                        ResolveAttack(true);
                        //Shoot(true, myJumper);
                    }
                    break;

                case MatchTeamAerialOutcome.Lose:
                    {
                        Opp.Stats.AerialsWon++;
                        oppJumper.Stats.AerialsWon++;

                        Match.GiveBallToOpp();
                        Opp.GoAhead(.15f);
                    }
                    break;
            }
        }

        void ResolveCornerKick()
        {
            Stats.CornerKicks++;

            ResolveAerial();
        }

        MatchSlot GetPlayerFromList(string otcof, Dictionary<MatchSlot, float> ocs)
        {
            var combinedWeight = (int)ocs.Sum(p => p.Value);

            var odd = App.RNG.RandiRange(0, combinedWeight);

            DLSection($"{otcof}", odd, combinedWeight);

            MatchSlot slot = null;

            var sum = 0f;

            var isDefined = false;

            foreach (var p in ocs)
            {
                var chance = Mathf.Round((float)p.Value / combinedWeight * 100f);

                DLPrint($"{p.Key.Source.Player.KnownAs} <= {Mathf.Round(sum + p.Value)} ({chance}%)");

                if (odd <= sum + p.Value && !isDefined)
                {
                    slot = p.Key;
                    isDefined = true;

                    if (Match.Debugger == null)
                        break;
                }

                sum += p.Value;
            }

            DLPrint($"--- Chose {slot.Source.Player.KnownAs} ---", true);

            return slot;
        }


        T GetOutcome<T>(string otcof, params (T Outcome, float Weight)[] ocs)
        {
            var combinedWeight = ocs.Sum(p => p.Weight);

            var odd = App.RNG.RandiRange(0, (int)combinedWeight);

            T outcome = default;

            var sum = 0f;

            var isDefined = false;

            DLSection($"{otcof}", odd, (int)combinedWeight);

            foreach (var p in ocs)
            {
                var chance = Mathf.Round(p.Weight / combinedWeight * 100f);

                DLPrint($"{p.Item1} <= {Math.Round(sum + p.Item2)} ({chance}%)");

                if (!isDefined && odd <= sum + p.Weight)
                {
                    outcome = p.Outcome;
                    isDefined = true;

                    //if (!Match.IsDebugging)
                    //    break;
                }

                sum += p.Weight;
            }

            DLPrint($"--- Outcome: {outcome} ---", true);

            return outcome;
        }
    }
}
