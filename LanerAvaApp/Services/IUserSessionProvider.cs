using LanerAvaApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanerAvaApp.Services
{
    public interface IUserSessionProvider
    {
        UserSession? Current { get; }
        event EventHandler? SessionChanged;
        void Set(UserSession session);
        void Clear();
    }

    public class UserSessionProvider : IUserSessionProvider
    {
        public UserSession? Current { get; private set; }
        public event EventHandler? SessionChanged;
        public void Set(UserSession session)
        {
            Current = session;
            SessionChanged?.Invoke(this, EventArgs.Empty);
        }
        public void Clear() => Current = null;
    }
}
