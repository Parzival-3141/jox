using Jox.Parsing;
using System;

namespace Jox.Runtime
{
    [Serializable]
    internal class RuntimeError : Exception
    {
        public readonly Token token;
        
        public RuntimeError(Token token, string message) : base (message)
        {
            this.token = token;
        }

    }
}