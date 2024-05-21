namespace SRkMatchSimAPI.Framework.DTO;

public enum PlayerDTOSide { HOME, AWAY }

public struct PlayerDTO
{
    public UserDTO User { get; set; }
    public PlayerAttrib Attrib { get; set; }
    public PlayerPos Pos { get; set; }
    public PlayerDTOSide Side { get; set; }
    public int MMR { get; set; }

    public PlayerDTO(string id, string name, PlayerPos pos, PlayerDTOSide side, PlayerAttrib attrib, int mmr)
    {
        User = new UserDTO(id, name);

        Pos = pos;
        Attrib = attrib;
        Side = side;
        MMR = mmr;
    }
}
