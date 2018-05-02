using System;

namespace Dash.Engine.IO
{
    public class ConfigParseException : Exception
    {
        public string FilePath { get; private set; }

        public ConfigParseException(string message, string filePath, int line, int character)
            : base(String.Format("{0} [Line: {1}, Char: {2}]", message, line, character))
        {
            FilePath = filePath;
        }
    }
}
