using System;

namespace Domain.Exceptions
{
    public class ClientDisconnected : Exception
    {
        public ClientDisconnected()
            : base("Connection was shut down.") { }

        public ClientDisconnected(string message)
            : base(message) { }

        public ClientDisconnected(string message, Exception inner)
            : base(message, inner) { }
    }
}