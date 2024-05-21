using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.WebSocketPeer;

namespace Futzim
{
    public partial class MatchDebugger : Node
    {
        Match m;

        [Export] Control dlCtr;
        [Export] Control dlPanel;

        [Export] Control squadPiece;
        [Export] Control pitch;
        [Export] Control playersList;
        [Export] Control popup;
        [Export] Control eventsCtr;

        public bool IsPlaying {  get; set; }

        Dictionary<MatchBreakpoint, bool> breakpoints = new Dictionary<MatchBreakpoint, bool>()
        {
            { MatchBreakpoint.Goal, false },
            { MatchBreakpoint.Shot, false },
            { MatchBreakpoint.ThroughBall, false }
        };

        public void ClearDL()
        {
            foreach (var item in dlCtr.GetChildren())
            {
                item.QueueFree();
            }
        }

        public void ShowPopup(bool enable, MatchSlot player)
        {
            popup.Visible = enable;

            if (enable)
            {
                popup.GetNode<Label>("Box/Passes/Value").Text = $"{ player.Stats.Passes } ({(((float)player.Stats.Passes / Mathf.Max(1, player.Stats.PassesAttempted))).RdPerc(0)}, att: {player.Stats.PassesAttempted})";
                popup.GetNode<Label>("Box/Interceptions/Value").Text = $"{player.Stats.Interceptions}";
                popup.GetNode<Label>("Box/Tackles/Value").Text = $"{player.Stats.Tackles}";
                popup.GetNode<Label>("Box/Goals/Value").Text = $"{player.Stats.Goals}";
                popup.GetNode<Label>("Box/Shots/Value").Text = $"{player.Stats.Shots} (on target: {player.Stats.ShotsOnTarget})";
                popup.GetNode<Label>("Box/Dribbles/Value").Text = $"{player.Stats.Dribbles} ({(((float)player.Stats.Dribbles / Mathf.Max(1, player.Stats.DribblesAttempted))).RdPerc(0)}, att: {player.Stats.DribblesAttempted})";
                popup.GetNode<Label>("Box/Aerials/Value").Text = $"{player.Stats.AerialsWon} ({(((float)player.Stats.AerialsWon / Mathf.Max(1, player.Stats.Aerials))).RdPerc(0)}, att: {player.Stats.Aerials})";

            }
        }

        public void PrintDL(string message, bool right = false, Color? color = null)
        {
            var l = dlPanel.Duplicate() as Control;
            l.Set("theme_override_font_sizes", 12);

            l.GetNode<Label>("Title").Text = message;

            l.GetNode<Label>("Title").HorizontalAlignment = right ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            l.GetNode<Control>("Weight").Hide();

            if (right)
            {
                l.Modulate = new Color("959595");
            }
            else
            {
                l.Modulate = color ?? new Color(1, 1, 1, 1);
            }

            dlCtr.AddChild(l);
        }

        public void SectionDL(string message, int randomNo, int cW)
        {
            var l = dlPanel.Duplicate() as Control;

            dlCtr.AddChild(l);

            l.Modulate = new Color("ff9e6d");
            l.GetNode<Label>("Title").Text = $"{message}";
            l.GetNode<Label>("Weight").Text = $"{randomNo} / {cW}  ";


            l.Show();
        }

        Club crf, sep;

        public override void _Ready()
        {
            crf = new Club("Flamengo", new Color(1, 0, 0));
            sep = new Club("Palmeiras", new Color(0, 1, 0));


            crf.ProSquad = new Squad(crf);
            sep.ProSquad = new Squad(sep);

            crf.ProSquad.AddPlayers(
                new Player("Rossi"),

                new Player("Varela"),
                new Player("Fabrício Bruno"),
                new Player("Léo Pereira"),
                new Player("Ayrton Lucas"),

                new Player("Gerson"),
                new Player("Pulgar"),
                new Player("Arrascaeta"),
                new Player("De La Cruz"),

                new Player("Pedro", new PlayerAttrib(7, 7, 7, 1)),
                new Player("Bruno Henrique", new PlayerAttrib(7, 7, 7, 10))
            );

            sep.ProSquad.AddPlayers(
                new Player("Weverton"),

                new Player("Mayke"),
                new Player("Gustavo Gómez"),
                new Player("Murilo"),
                new Player("Piquerez"),

                new Player("Richard Rios"),
                new Player("Zé Rafael"),
                new Player("Raphael Veiga"),
                new Player("Dudu"),

                new Player("Endrick"),
                new Player("Rony")
            );

            m = new Match(crf.ProSquad, sep.ProSquad, this);

            m.HasEnded += OnMatchEnded;
            m.HasAdvanced += OnMatchAdvanced;
            m.PreAdvance += OnBeforeMatchAdvanced;
            m.Breakpoint += OnBreakpoint;

            CreatePieces();

            UpdateBasicInfo();
            UpdatePlayersList(true);
            UpdateAttrib();

            Controls.GetNode<Button>("PlayOnce").Pressed += OnPressPlayOnce;
            Controls.GetNode<Button>("Play").Pressed += OnPressPlay;

            foreach (CheckBox b in Controls.GetNode("Breakpoints/Box").GetChildren())
            {
                Enum.TryParse(b.Name, out MatchBreakpoint bp);

                b.Toggled += (enable) => OnToggledBP(enable, bp);
            }

            foreach (Button b in HUD.GetNode("Actions").GetChildren())
            {
                b.Pressed += () => OnPressAction(b);
            }

            m.Home.Scored += OnGoal;
            m.Away.Scored += OnGoal;

            m.Home.YellowCard += OnYC;
            m.Away.YellowCard += OnYC;

            m.Home.RedCard += OnRC;
            m.Away.RedCard += OnRC;
        }

        void OnGoal(MatchSlot player, float minutes)
        {
            var lb = new Label();
            lb.Text = $"{player.Source.Player.KnownAs} ({MathF.Floor(minutes)}'')";
            eventsCtr.GetNode("Box").AddChild(lb);

            lb.Set("theme_override_font_sizes/font_size", 12);

            lb.CustomMinimumSize = new Vector2(150, 0);

            lb.HorizontalAlignment = player.Team.IsHome ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        }

        void OnYC(MatchSlot player, float minutes)
        {
            var lb = new Label();
            lb.Text = $"{player.Source.Player.KnownAs} ({MathF.Floor(minutes)}'')";
            eventsCtr.GetNode("Box").AddChild(lb);

            if (player.CardState == MatchSlotCardState.YY)
            {
                lb.Modulate = new Color(1, 0.5f, 0);
            }
            else
            {
                lb.Modulate = new Color(1, 1, 0);
            }

            lb.Set("theme_override_font_sizes/font_size", 12);

            lb.CustomMinimumSize = new Vector2(150, 0);

            lb.HorizontalAlignment = player.Team.IsHome ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        }
        void OnRC(MatchSlot player, float minutes)
        {
            var lb = new Label();
            lb.Text = $"{player.Source.Player.KnownAs} ({MathF.Floor(minutes)}'')";
            eventsCtr.GetNode("Box").AddChild(lb);

            lb.Modulate = new Color(1, 0, 0);

            lb.Set("theme_override_font_sizes/font_size", 12);

            lb.CustomMinimumSize = new Vector2(150, 0);

            lb.HorizontalAlignment = player.Team.IsHome ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        }

        void UpdateAttrib()
        {
            foreach (var t in m.Teams)
            {
                var n = t.IsHome ? "Home" : "Away";

                var box = playersList.GetNode($"{n}/Attrib/Grid");

                box.GetNode<Label>("Dri/Value").Text = $"{t.Attrib.Dribbling.RdNo(2)}";
                box.GetNode<Label>("Pac/Value").Text = $"{t.Attrib.Pace.RdNo(2)}";
                box.GetNode<Label>("Pas/Value").Text = $"{t.Attrib.Passing.RdNo(2)}";
            }
        }

        void UpdatePlayersList(bool connect = false)
        {
            foreach(var t in m.Teams)
            {
                var n = t.IsHome ? "Home" : "Away";

                var box = playersList.GetNode($"{n}/Box");

                playersList.GetNode<Label>($"{n}/KnownAs").Text = t.Name;

                for (var i = 0; i < 11; i++)
                {
                    var slot = t.Slots[i];

                    var it = box.GetChild<Control>(i + 1);

                    it.GetNode<Label>("KnownAs").Text = slot.Source.Player.KnownAs;
                    it.GetNode<Label>("Ptc").Text = $" {slot.Ptc.RdPerc(0) }";
                    it.GetNode<Label>("Ptc").Modulate = new Color(1, 1, 1, 1).Lerp(new Color(0, 1, 0), (slot.Ptc - 0.33f) * 1.5f);
                    it.GetNode<Label>("Ovr").Text = $"{slot.Attrib.GetAvg().RdNo(1)}";

                    if (connect)
                    {
                        it.MouseEntered += () => OnHoverPlayerFromList(slot);
                        it.MouseExited += () => OnUnhoverPlayerFromList();
                    }

                }
            }
        }

        void OnHoverPlayerFromList(MatchSlot slot)
        {
            ShowPopup(true, slot);
        }

        void OnUnhoverPlayerFromList()
        {
            popup.Hide();
        }

        void CreatePieces()
        {
            var pitchSize = pitch.Size;

            foreach(var team in m.Teams)
            {
                var start = team.IsHome ? 0f : 1f;
                var mul = team.IsHome ? 1f : -1f;

                for (var i = 0; i < 11; i++)
                {
                    var piece = squadPiece.Duplicate() as Control;

                    var h = pitch.GetNode("Players/Home");
                    h.AddChild(piece);

                    piece.GetNode<Label>("BG/No").Text = $"{m.Home.Slots[i].Source.No}";

                    if (i == 0)
                    {
                        team.Slots[i].Source.Pos = new Vector2(0.5f, 0.04f);
                        piece.Position = pitch.Size * new Vector2(start + 0.04f * mul, 0.5f);
                    }
                    else
                    {
                        var f = Squad.Formations["4-4-2"][i - 1];

                        team.Slots[i].Source.Pos = f;

                        piece.Position = pitch.Size * new Vector2(start + f.Y * 0.5f * mul, start + f.X * mul);

                        piece.GetNode<Control>("BG").SelfModulate = team.Squad.Club.ColorA;
                    }
                    piece.Show();
                }
            }
        }

        void OnToggledBP(bool enable, MatchBreakpoint bp)
        {
            breakpoints[bp] = enable;
        }

        void OnBreakpoint(MatchBreakpoint bp)
        {
            if (breakpoints[bp])
            {
                IsPlaying = !IsPlaying;

                Controls.GetNode<Button>("Play").Text = IsPlaying ? "||" : ">";
            }
        }

        void OnPressAction(Button b)
        {
            switch (b.Name)
            {
                case "Restart":
                    {
                        GetTree().ReloadCurrentScene();
                    }
                    break;

                case "End":
                    {
                        while (m.State != MatchState.Post)
                        {
                            m.Debugger = null;
                            m.Advance(false);

                            m.Debugger = this;

                            OnMatchAdvanced();
                        }
                    }
                    break;

                case "100x":
                    {
                        int W = 0, D = 0, L = 0;
                        int goalsH = 0, goalsA = 0;

                        for(var i = 0; i < 100; i++)
                        {
                            var _m = new Match(crf.ProSquad, sep.ProSquad);
                            _m.AdvanceTillEnd();

                            goalsH += _m.Home.Stats.Goals;
                            goalsA += _m.Away.Stats.Goals;

                            if (_m.Home.Stats.Goals > _m.Away.Stats.Goals)
                                W++;
                            else if (_m.Home.Stats.Goals < _m.Away.Stats.Goals)
                                L++;
                            else
                                D++;
                        }


                        GD.Print($"CRF: {W}; draw: {D}; SEP: {L} -- goals: {goalsH} ({ (goalsH / 100f).RdNo(2) }/g) x {goalsA} ({(goalsA / 100f).RdNo(2)}/g)");
                    }
                    break;
            }
        }


        Control HUD => GetNode<Control>("HUD");
        Control Controls => GetNode<Control>("HUD/Controls");

        void UpdateBasicInfo()
        {
            HUD.GetNode<Label>("Basic/Home").Text = m.Home.Name;
            HUD.GetNode<Label>("Basic/Home/Score").Text = $"{m.Home.Stats.Goals}";
            HUD.GetNode<Label>("Basic/Away").Text = m.Away.Name;
            HUD.GetNode<Label>("Basic/Away/Score").Text = $"{m.Away.Stats.Goals}";
        }

        void UpdateTeamsStats()
        {
            var totalPasses = (float)m.Home.Stats.Passes + m.Away.Stats.Passes;
            var totalDribbles = (float)m.Home.Stats.Dribbles + m.Away.Stats.Dribbles;


            Stat("Possession", m.Home.Possession, m.Away.Possession, true);
            Stat("Shots", m.Home.Stats.Shots, m.Away.Stats.Shots, false);
            Stat("Blocks", m.Home.Stats.Blocks, m.Away.Stats.Blocks, false);
            Stat("GKSaves", m.Home.Stats.GKSaves, m.Away.Stats.GKSaves, false);
            Stat("Cleared", m.Home.Stats.Cleared, m.Away.Stats.Cleared, false);
            Stat("Fouls", m.Home.Stats.FoulsCommitted, m.Away.Stats.FoulsCommitted, false);


            HUD.GetNode<Label>("TeamStats/Passes/Home").Text = $"{ Mathf.Round(m.Home.Passes) } ({ Mathf.Round((m.Home.Passes / (float)Mathf.Max(1, m.Home.PassesAttempted)) * 100) }%)";
            HUD.GetNode<Label>("TeamStats/Passes/Away").Text = $"{ Mathf.Round(m.Away.Passes) } ({ Mathf.Round((m.Away.Passes / (float)Mathf.Max(1, m.Away.PassesAttempted)) * 100) }%)";

            var str = "";

            str += $"LoseTime: {Mathf.Round(m.Home.PassesByType[MatchTeamDecisionOutcome.LoseTime])} ({Mathf.Round((m.Home.PassesByType[MatchTeamDecisionOutcome.LoseTime] / (float)Mathf.Max(1, m.Home.PassesAttemptedByType[MatchTeamDecisionOutcome.LoseTime])) * 100)}%)\n";
            str += $"Pass: {Mathf.Round(m.Home.PassesByType[MatchTeamDecisionOutcome.Advance])} ({Mathf.Round((m.Home.PassesByType[MatchTeamDecisionOutcome.Advance] / (float)Mathf.Max(1, m.Home.PassesAttemptedByType[MatchTeamDecisionOutcome.Advance])) * 100)}%)\n";
            str += $"ThroughBall: {Mathf.Round(m.Home.PassesByType[MatchTeamDecisionOutcome.ThroughBall])} ({Mathf.Round((m.Home.PassesByType[MatchTeamDecisionOutcome.ThroughBall] / (float)Mathf.Max(1, m.Home.PassesAttemptedByType[MatchTeamDecisionOutcome.ThroughBall])) * 100)}%)";


            HUD.GetNode<ProgressBar>("TeamStats/Passes/Bar").Value = m.Home.Passes / totalPasses;
            HUD.GetNode<Control>("TeamStats/Passes/Title").TooltipText = $"{str}";


            HUD.GetNode<ProgressBar>("TeamStats/Dribbles/Bar").Value = m.Home.Stats.Dribbles / totalDribbles;
            HUD.GetNode<Label>("TeamStats/Dribbles/Home").Text = $"{Mathf.Round(m.Home.Stats.Dribbles)} ({Mathf.Round((m.Home.Stats.Dribbles / (float)Mathf.Max(1, m.Home.Stats.DribblesAttempted)) * 100)}%)";
            HUD.GetNode<Label>("TeamStats/Dribbles/Away").Text = $"{Mathf.Round(m.Away.Stats.Dribbles)} ({Mathf.Round((m.Away.Stats.Dribbles / (float)Mathf.Max(1, m.Away.Stats.DribblesAttempted)) * 100)}%)";


            void Stat(string stat, float home, float away, bool percent)
            {
                var total = home + away;

                var h = percent ? $"{Mathf.Round((home / total) * 100)}%" : $"{home}";
                var a = percent ? $"{Mathf.Round((away / total) * 100)}%" : $"{away}";

                HUD.GetNode<Label>($"TeamStats/{ stat }/Home").Text = $"{ h }";
                HUD.GetNode<Label>($"TeamStats/{ stat }/Away").Text = $"{ a }";

                HUD.GetNode<ProgressBar>($"TeamStats/{ stat }/Bar").Value = home / total;
            }
        }

        void UpdateTime()
        {
            var minutes = Mathf.Floor(m.Minutes);
            var seconds = Mathf.Floor((m.Minutes % 1) * 60);
            
            HUD.GetNode<Label>("Basic/Time").Text = $"{minutes.ToString("00")}:{ seconds.ToString("00")}";
        }

        void OnPressPlayOnce()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
            }
            else
            {
                m.Advance();
            }
        }

        void OnMatchEnded()
        {
            Controls.GetNode<Button>("PlayOnce").Disabled = true;
            Controls.GetNode<Button>("Play").Disabled = true;

        }

        void OnPressPlay()
        {
            IsPlaying = !IsPlaying;

            Controls.GetNode<Button>("Play").Text = IsPlaying ? "||" : ">";
        }

        void OnBeforeMatchAdvanced()
        {
            ClearDL();
        }

        void OnMatchAdvanced()
        {

            UpdateTime();

            HUD.GetNode<ProgressBar>("Basic/Ball").Value =m.Ball;

            UpdateTeamsStats();
            UpdateBasicInfo();
            UpdatePlayersList();
            UpdateAttrib();
            //HUD.GetNode<ProgressBar>("Basic/Ball").FillMode = (int)(m.PT.IsHome ? ProgressBar.FillModeEnum.BeginToEnd : ProgressBar.FillModeEnum.EndToBegin);
        }

        public override void _Process(double dT)
        {
            if (IsPlaying)
            {
                //for(var i = 0; i < 3; i++)
                {
                    m.Advance();
                }
            }
        }
    }
}
