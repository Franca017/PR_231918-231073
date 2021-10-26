using System;

namespace Domain.Exceptions
{
    public class PathInexistente : Exception
    {
        public PathInexistente()
            : base("El archivo no pudo ser encontrado en ese path.") { }

        public PathInexistente(string message)
            : base(message) { }

        public PathInexistente(string message, Exception inner)
            : base(message, inner) { }
    }
}