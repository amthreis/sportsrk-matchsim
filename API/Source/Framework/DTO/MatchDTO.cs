namespace SRkMatchSimAPI.Framework.DTO;

public struct MatchDTO
{
    public string Id { get; set; }
    public List<PlayerDTO> Players { get; set; }

    public MatchDTO(string id, List<PlayerDTO> players)
    {
        Id = id;
        Players = players;
    }
}
