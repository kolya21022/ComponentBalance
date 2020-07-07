using ComponentBalanceSqlv2.Data;

namespace ComponentBalanceSqlv2.Utils
{
    public class AuthenticationEventArgs : System.EventArgs
    {
        public WorkGuild User { get; }

        public bool IsAuthorized { get; }

        public AuthenticationEventArgs(WorkGuild user, bool isAuthorized)
        {
            User = user;
            IsAuthorized = isAuthorized;
        }
    }
}