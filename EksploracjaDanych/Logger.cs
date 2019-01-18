using System;

namespace EksploracjaDanych
{
    public static class Logger
    {
        public static event Action<string> Logging;

        public static void Log(string message)
            => Logging?.Invoke(message);
    }
}