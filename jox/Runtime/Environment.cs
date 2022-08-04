
using Jox.Parsing;
using System;
using System.Collections.Generic;

namespace Jox.Runtime
{
    public sealed class Environment
    {
        private Dictionary<string, object> values = new();
        private Environment enclosing = null;

        public Environment()
        {
            enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public void DefineVariable(string name, object value)
        {
            values[name] = value; //  Covers definition *and* redefinition!
        }

        public object GetVariableValue(Token identifier)
        {
            if (values.TryGetValue(identifier.lexeme, out object value))
                return value;

            if (enclosing != null) return enclosing.GetVariableValue(identifier);

            throw new RuntimeError(identifier, $"Undefined variable '{identifier.lexeme}'.");
        }

        public void AssignVariableValue(Token identifier, object value)
        {
            if (values.ContainsKey(identifier.lexeme))
            {
                values[identifier.lexeme] = value;
                return;
            }

            if(enclosing != null)
            {
                enclosing.AssignVariableValue(identifier, value);
                return;
            }

            throw new RuntimeError(identifier, $"Undefined variable '{identifier.lexeme}'.");
        }
    }
}
