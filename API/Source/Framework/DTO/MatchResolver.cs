using System.Text.RegularExpressions;

namespace SRkMatchSimAPI.Framework.DTO;

public partial class MatchResolver
{
    //public List<object> matchDTOs = new List<object>();
    public event Action<List<Match>> Done;
    List<Match> matches;
    List<MatchOTD> matchOTDs;

    public static MatchOTD PlaySingle(Match match, bool parseOnly = false)
    {
        match.AdvanceTillEnd();

        return new MatchOTD(match, parseOnly);
    }

    public void Play(List<Match> m)
    {
        matches = m;
        matchOTDs = new List<MatchOTD>();

        foreach(var match in matches)
        {
            Console.WriteLine($"home {match.Home.Slots.Count} x away {match.Away.Slots.Count}");
            while (match.State != MatchState.Post)
            {
                match.Advance(false);
            }

            matchOTDs.Add(new MatchOTD(match));
        }

        Done?.Invoke(matches);
    }
}
