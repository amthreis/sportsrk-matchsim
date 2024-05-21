
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace SRkMatchSimAPI.Framework;

public enum MatchEventType { GOAL, REDCARD, YELLOWCARD, YELLOWREDCARD }

public struct MatchEvent
{
    public int Time { get; }
    public string PlayerId { get; }
    public string Side { get; }

    public MatchEventType Type {get;}

    public MatchEvent(int time, string playerId, string side, MatchEventType type)
    {
        Time = time;
        PlayerId = playerId;
        Side = side;
        Type = type;
    }

    public override string ToString()
    {
        return $"{(Side == "HOME" ?  "" : "     ")}{PlayerId} {Time}'' {Type}";
    }
}