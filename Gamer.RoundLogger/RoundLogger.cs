using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamer.RoundLoggerSystem
{
    public static class RoundLogger
    {
        #region Public
        public static event Action<LogMessage[], DateTime> OnEnd;
        public static DateTime BeginLog = DateTime.Now;

        public struct LogMessage
        {
            public DateTime Time;
            public string Type;
            public string Module;
            public string Message;

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

        public static void Log(string module, string type, string message)
        {
            Logs.Add(new LogMessage(DateTime.Now, type, module, message.Replace("\n", "\\n")));
        }

        public static string PlayerToString(this Player player) => player == null ? null : $"{player.Nickname} (ID: {player.Id}|UID: {player.UserId}|Class: {player.Role})";

        public static void RegisterTypes(string type)
        {
            Types.Add(type);
            TypesMaxLength = (byte)Math.Max(TypesMaxLength, type.Length);
        }

        public static void RegisterModules(string module)
        {
            Modules.Add(module);
            ModulesMaxLength = (byte)Math.Max(ModulesMaxLength, module.Length);
        }

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
