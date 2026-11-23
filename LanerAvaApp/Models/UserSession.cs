using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class UserSession
    {
        public int UserId { get; }
        public string Login { get; }
        public string Role { get; }

        public UserSession(int userId, string login, string role)
        {
            UserId = userId;
            Login = login;
            Role = role;
        }
    }
}
