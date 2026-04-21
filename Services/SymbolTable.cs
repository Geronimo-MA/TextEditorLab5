using System.Collections.Generic;

namespace TextEditorLab.Services
{
    public class SymbolTable
    {
        private readonly Dictionary<string, string> _symbols = new();

        public bool Declare(string name, string type)
        {
            if (_symbols.ContainsKey(name))
                return false;

            _symbols[name] = type;
            return true;
        }

        public bool Contains(string name)
        {
            return _symbols.ContainsKey(name);
        }

        public string? GetTypeOf(string name)
        {
            return _symbols.TryGetValue(name, out var type) ? type : null;
        }
    }
}