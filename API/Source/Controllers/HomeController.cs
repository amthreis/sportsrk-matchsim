using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SRkMatchSimAPI.Framework;
using SRkMatchSimAPI.Framework.DTO;
using StackExchange.Redis;
using Visus.Cuid;

namespace SRkMatchSimAPI.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(new string[] { "This", "is", "Sim" });
    }

    [HttpPost]
    public IActionResult Sim(StartRequestDTO srDTO)
    {
        Task.Run(() => Work(srDTO));

        return Ok();
    }

    [HttpPost("fake")]
    public IActionResult SimFake()
    {
        var m = FakeMatch();

        m.AdvanceTillEnd();

        return Ok(new MatchOTD(m));
    }

    [HttpPost("fake/work")]
    public IActionResult WorkFake()
    {
        FakeWork();

        return Ok();
    }

    async void Work(StartRequestDTO srDTO)
    {
        Thread.Sleep(2000);

        await using(var connection = ConnectionMultiplexer.Connect($"{srDTO.RedisHost}:{srDTO.RedisPort}"))
        {
            while(true)
            {
                var match = await connection.GetDatabase().ListRightPopAsync("football:matches:tbp");

                if (!match.HasValue)
                {
                    Console.WriteLine("No more matches to sim.");
                    return;
                }

                var dsr = Deserialized<MatchDTO>(match);
                var m = Match.FromDTO(dsr);

                var matchOTD = MatchResolver.PlaySingle(m);

                Console.WriteLine("-------------------------------------");
                Console.WriteLine(matchOTD);
            }
        }
    }

    public static T Deserialized<T>(string data)
    {
        return JsonConvert.DeserializeObject<T>(data,
                new JsonSerializerSettings()
                {
                    Converters = { new StringEnumConverter() }
                });
    }

    public static string Serialized(object data)
    {
        return JsonConvert.SerializeObject(data,
            new JsonSerializerSettings()
            {
                Converters = { new StringEnumConverter() }
            });
    }

    MatchDTO FakeMatchDTO()
    {
        Faker.RandomNumber.SetSeed(1224);
        
        var players = new List<PlayerDTO>();
        
        for(var i = 0; i < 22; i++)
        {
            players.Add(new PlayerDTO
            {
                User = new UserDTO {
                    Email = Faker.Internet.Email(),
                    Id = new Cuid2().ToString()
                },
                Attrib = new PlayerAttrib {
                    Finishing = RNG.RandiRange(1, 10),
                    Aerial = RNG.RandiRange(1, 10),
                    Dribbling = RNG.RandiRange(1, 10),
                    Goalkeeping = RNG.RandiRange(1, 10),
                    Marking = RNG.RandiRange(1, 10),
                    Mentality = RNG.RandiRange(1, 10),
                    Pace = RNG.RandiRange(1, 10),
                    Passing = RNG.RandiRange(1, 10),
                    Resistance = RNG.RandiRange(1, 10),
                    WorkRate = RNG.RandiRange(1, 10)
                },
                MMR = RNG.RandiRange(1000, 10_0000),
                Pos = PlayerPos.CB,
                Side = i <= 10 ? PlayerDTOSide.HOME : PlayerDTOSide.AWAY
            });
        }

        return new MatchDTO(new Cuid2().ToString(), players);
    }

    Match FakeMatch()
    {
        var m = Match.FromDTO(FakeMatchDTO());

        return m;
    }

    async void FakeWork()
    {
        var connection = await ConnectionMultiplexer.ConnectAsync($"localhost:6379,allowAdmin=true");
        
        await connection.GetServer("localhost:6379").FlushAllDatabasesAsync();

        for(var i = 0; i < 1; i++)
        {
            await connection.GetDatabase().ListLeftPushAsync("football:matches:tbp", Serialized(FakeMatchDTO()));
        }

        while(true)
        {
            var match = await connection.GetDatabase().ListRightPopAsync("football:matches:tbp");

            if (!match.HasValue)
            {
                Console.WriteLine("No more matches to sim.");
                break;
            }

            var dsr = Deserialized<MatchDTO>(match);
            var m = Match.FromDTO(dsr);

            var matchOTD = MatchResolver.PlaySingle(m, true);

            var options = new System.Text.Json.JsonSerializerOptions{
                Converters = { 
                    new JsonStringEnumConverter()
                    },
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            using (var client = new HttpClient())
            {
                var post = await client.PostAsJsonAsync(
                    "http://localhost:5757/common/football/matchsim-resolve",
                    matchOTD,
                    options
                );

                var result = post.StatusCode;
                Console.WriteLine("-------------------------------------");
                Console.WriteLine(post.StatusCode);
                Console.WriteLine(await post.Content.ReadAsStringAsync());
                
            }
            //Console.WriteLine("-------------------------------------");
            //Console.WriteLine(matchOTD);
        }

        await connection.CloseAsync();
    }
}