using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public struct UserDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }

        public UserDTO(string userId, string email)
        {
            Email = email;
            Id = userId;
        }
    }
}
