using LanerAvaApp.Data;
using LanerAvaApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Services
{
    public interface IAuthorizationService
    {
        bool IsInRole(string role);
        bool IsAdmin { get; }
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUserSessionProvider _sessionService;
        private UserSession _session =>
            _sessionService.Current ?? throw new InvalidOperationException("Не удалась аутентифицировать пользователя!");

        public AuthorizationService(IUserSessionProvider sessionService)
        {
            _sessionService = sessionService;
        }

        public bool IsInRole(string role) =>
            string.Equals(_session.Role, role, StringComparison.OrdinalIgnoreCase);

        public bool IsAdmin => IsInRole(Roles.Admin);

        
    }
}
