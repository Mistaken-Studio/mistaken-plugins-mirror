using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamer.RoundLoggerSystem
{
    /// <summary>
    /// Round Logger
    /// </summary>
    public static class RoundLogger
    {
        #region Public
        /// <summary>
        /// Action event invoked on round saving
        /// </summary>
        public static event Action<LogMessage[], DateTime> OnEnd;
        private static DateTime BeginLog = DateTime.Now;

        /// <summary>
        /// Log object
        /// </summary>
        public struct LogMessage
        {
            /// <summary>
            /// Log time
            /// </summary>
            public DateTime Time;
            /// <summary>
            /// Log Type
            /// </summary>
            public string Type;
            /// <summary>
            /// Log Module
            /// </summary>
            public string Module;
            /// <summary>
            /// Log Message
            /// </summary>
            public string Message;
            /// <summary>
            /// Consturctor
            /// </summary>
            public LogMessage(DateTime time, string type, string module, string message)
            {
                Time = time;
                Type = type;
                Module = module;
                Message = message;

                if (!Types.Contains(Type))
                    RegisterTypes(Type);
                if (!Modules.Contains(Module))
                    RegisterModules(Module);
            }
            /// <inheritdoc/>
            public override string ToString()
            {
                string tmpType = Type;
                while (tmpType.Length < TypesMaxLength)
                    tmpType += " ";
                string tmpModule = Module;
                while (tmpModule.Length < ModulesMaxLength)
                    tmpModule += " ";
                return $"{Time:HH:mm:ss.fff} | {tmpModule} | {tmpType} | {Message}";
            }
        }
        /// <summary>
        /// Used to log message
        /// </summary>
        /// <param name="module">Module</param>
        /// <param name="type">Type</param>
        /// <param name="message">Message</param>
        public static void Log(string module, string type, string message)
        {
            Exiled.API.Features.Log.SendRaw($"[ROUND LOG] [{module}: {type}] {message}", ConsoleColor.DarkYellow);
            Logs.Add(new LogMessage(DateTime.Now, type, module, message.Replace("\n", "\\n")));
        }
        /// <summary>
        /// Converts player to string version
        /// </summary>
        /// <param name="player">Player</param>
        /// <returns><paramref name="player"/> string version</returns>
        public static string PlayerToString(this Player player) => player == null ? null : $"{player.Nickname} (ID: {player.Id}|UID: {player.UserId}|Class: {player.Role})";

        private static void RegisterTypes(string type)
        {
            Types.Add(type);
            TypesMaxLength = (byte)Math.Max(TypesMaxLength, type.Length);
        }

        private static void RegisterModules(string module)
        {
            Modules.Add(module);
            ModulesMaxLength = (byte)Math.Max(ModulesMaxLength, module.Length);
        }
        /// <summary>
        /// Calls <see cref="Ini"/> if <see cref="Initiated"/> is <see langword="false"/>
        /// </summary>
        public static void IniIfNotAlready()
        {
            if (Initiated)
                return;
            Ini();
        }
        #endregion

        private static bool Initiated = false;
        private static void Ini()
        {
            Initiated = true;
            Exiled.API.Features.Log.Debug("Initiated RoundLogger");
            Exiled.Events.Handlers.Server.RestartingRound += Server_RestartingRound;
            Log("INFO", "LOGGER", "Start of log");
            BeginLog = DateTime.Now;
        }

        private static readonly List<LogMessage> Logs = new List<LogMessage>();

        private static byte TypesMaxLength = 0;
        private static byte ModulesMaxLength = 0;
        private static readonly HashSet<string> Types = new HashSet<string>();
        private static readonly HashSet<string> Modules = new HashSet<string>();


        private static void Server_RestartingRound() => _ = Server_RestartingRoundTask();

        private static async Task Server_RestartingRoundTask()
        {
            await Task.Delay(10);
            Log("LOGGER", "INFO", "End of log");
            var logsArray = Logs.ToArray();
            Logs.Clear();
            Log("LOGGER", "INFO", "Start of log");
            OnEnd?.Invoke(logsArray, BeginLog);
            BeginLog = DateTime.Now;
        }
    }
}
