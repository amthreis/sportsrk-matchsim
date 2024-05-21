
namespace SRkMatchSimAPI.Framework;

public class Club
{
    public string KnownAs { get; set; }

    public Squad ProSquad { get; set; }
    public string ColorA { get; set; }

    public Club(string knownAs, string colorA) 
    {
        KnownAs = knownAs;
        ColorA = colorA;

        ProSquad = new Squad(this);
    }
}
