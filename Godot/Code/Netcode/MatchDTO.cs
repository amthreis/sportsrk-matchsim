
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
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
}
