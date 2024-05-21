namespace SRkMatchSimAPI.Framework.DTO;

public struct StartRequestDTO
{
    public string RedisPort { get; set; }
    public string RedisHost { get; set; }
    public string RedisProperty { get; set; }
    
    public StartRequestDTO(string host, string port, string property) 
    {
        RedisHost = host;
        RedisPort = port;
        RedisProperty = property;
    }
}