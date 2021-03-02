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
        public static event Action<LogMessage[]> OnEnd;

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
                    Exiled.API.Features.Log.Warn($"{Type} is not registered type");
                if (!Modules.Contains(Module))
                    Exiled.API.Features.Log.Warn($"{Module} is not registered module");
            }

            public override string ToString()
            {
                string tmpType = Type;
                while (tmpType.Length < TypesMaxLength)
                    tmpType += " ";
                string tmpModule = Module;
                while (tmpModule.Length < ModulesMaxLength)
                    tmpModule += " ";
                return $"{Time:HH:mm:ss:fff} | {tmpType} | {tmpModule} | {Message}";
            }
        }

        public static void Log(string type, string module, string message)
        {
            Logs.Add(new LogMessage
            {
                Time = DateTime.Now,
                Type = type,
                Module = module,
                Message = message
            });
        }

        public static void RegisterTypes(params string[] types)
        {
            foreach (var item in types)
                Types.Add(item);
            TypesMaxLength = (byte)Types.Max(i => i.Length);
        }

        public static void RegisterModules(params string[] modules)
        {
            foreach (var item in modules)
                Modules.Add(item);
            ModulesMaxLength = (byte)Modules.Max(i => i.Length);
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
            Log("INFORMATION", "ROUND LOGGER", "Start of log");
        }

        private static readonly List<LogMessage> Logs = new List<LogMessage>();

        private static byte TypesMaxLength = 0;
        private static byte ModulesMaxLength = 0;
        private static readonly HashSet<string> Types = new HashSet<string>()
        {
            "INFORMATION"
        };
        private static readonly HashSet<string> Modules = new HashSet<string>()
        {
            "ROUND LOGGER"
        };


        private static void Server_RestartingRound() => _ = Server_RestartingRoundTask();
        
        private static async Task Server_RestartingRoundTask()
        {
            await Task.Delay(1000);
            Log("INFORMATION", "ROUND LOGGER", "End of log");
            var logsArray = Logs.ToArray();
            Logs.Clear();
            Log("INFORMATION", "ROUND LOGGER", "Start of log");
            OnEnd?.Invoke(logsArray);
        }
    }
}
