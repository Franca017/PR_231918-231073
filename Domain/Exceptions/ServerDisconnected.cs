using System;

namespace Domain.Exceptions
{
    public class ServerDisconnected : Exception
    {
        public ServerDisconnected()
            : base("Server was shutdown.") { }

        public ServerDisconnected(string message)
            : base(message) { }

        public ServerDisconnected(string message, Exception inner)
            : base(message, inner) { }
    }
}