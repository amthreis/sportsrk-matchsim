using System.Numerics;
namespace SRkMatchSimAPI.Framework;

public struct SquadSlot
{
    public Player Player { get; }
    public int No { get; }
    public Vector2 Pos { get; set; }
    public int Index { get; }

    public SquadSlot(int idx, Player player, int no, Vector2 pos)
    {
        Index = idx;
        Player = player;
        No = no;
        Pos = pos;
    }

    public override int GetHashCode()
    {
        return Player.GetHashCode();
    }
}
