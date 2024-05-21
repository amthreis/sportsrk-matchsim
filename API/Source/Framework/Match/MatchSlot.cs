
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SRkMatchSimAPI.Framework;

public enum MatchSlotCardState { None, Y, YY, YR, R }

public class MatchSlot
{
    public SquadSlot Source;

    public MatchStats Stats;

    public PlayerAttrib Attrib;

    public float Ptc;

    public MatchTeam Team;

    public MatchSlotCardState CardState;
    public bool IsOut = false;

    public bool IsOutOfGame()
    {
        return CardState != MatchSlotCardState.None && CardState != MatchSlotCardState.Y;
    }

    public void YellowCard()
    {
        switch(CardState)
        {
            case MatchSlotCardState.None:
                {
                    CardState = MatchSlotCardState.Y;
                }
                break;

            case MatchSlotCardState.Y:
                {
                    CardState = MatchSlotCardState.YY;
                }
                break;

            default:
                throw new Exception($"{Source.Player.KnownAs} can't get a yellow card in current state: {CardState}.");
        }


        Stats.YellowCards++;
        Team.Stats.YellowCards++;
    }

    public void RedCard()
    {

        switch (CardState)
        {
            case MatchSlotCardState.None:
                {
                    CardState = MatchSlotCardState.R;
                }
                break;

            case MatchSlotCardState.Y:
                {
                    CardState = MatchSlotCardState.YR;
                }
                break;

            default:
                throw new Exception($"{Source.Player.KnownAs} can't get a red card in current state: {CardState}.");
        }

        Stats.RedCards++;
        Team.Stats.RedCards++;
    }

    public MatchSlot(MatchTeam team, SquadSlot source)
    {
        Team = team;
        Source = source;
    }

    public void CalcAttrib(float ball)
    {
        var pos = Team.IsHome ? Source.Pos.Y : 1f - Source.Pos.Y;

        var diff = MathF.Abs(pos - ball);

        var wkr = Source.Player.Attrib.WorkRate / 10f;


        Ptc =  MathF.Pow(1f - diff, 2f);
        Ptc = MathF.Pow(Ptc, 1f - wkr * 0.5f + (Source.Index == 0 ? 1 : 0));

        Attrib = new PlayerAttrib(
            Source.Player.Attrib.Dribbling * Ptc,
            Source.Player.Attrib.Pace * Ptc, 
            Source.Player.Attrib.Passing * Ptc,
            Source.Player.Attrib.WorkRate
            );

    }

}
