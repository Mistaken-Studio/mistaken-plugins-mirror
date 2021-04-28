﻿using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Gamer.Diagnostics
{
    /// <summary>
    /// Master module handler
    /// </summary>
    public static class MasterHandler
    {
        /// <summary>
        /// Handlers bound to Module
        /// </summary>
        public static readonly Dictionary<Module, Dictionary<string, Exiled.Events.Events.CustomEventHandler>> Handlers = new Dictionary<Module, Dictionary<string, Exiled.Events.Events.CustomEventHandler>>();
        /// <summary>
        /// Handles event and mesures time that took to handle
        /// </summary>
        /// <param name="module">Modue</param>
        /// <param name="action">Handler</param>
        /// <param name="Name">Handler Name</param>
        /// <returns>Event Handler</returns>
        public static Exiled.Events.Events.CustomEventHandler Handle(this Module module, Action action, string Name)
        {
            if (!Handlers.ContainsKey(module))
                Handlers[module] = new Dictionary<string, Exiled.Events.Events.CustomEventHandler>();
            if (Handlers[module].ContainsKey(Name))
                return Handlers[module][Name];
            DateTime start;
            DateTime end;
            TimeSpan diff;
            Exiled.Events.Events.CustomEventHandler tor = () =>
            {
                start = DateTime.Now;
                try
                {
                    action();
                }
                catch (System.Exception ex)
                {
                    Log.Error($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] [{module.Name}: {Name}] Caused Exception");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                    ErrorBacklog.Add($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] [{module.Name}: {Name}] Caused Exception");
                    ErrorBacklog.Add(ex.Message);
                    ErrorBacklog.Add(ex.StackTrace);
                    RoundLoggerSystem.RoundLogger.Log("DIAGNOSTICS", "ERROR", $"[{module.Name}: {Name}] Caused Exception | {ex.Message}");
                }
                end = DateTime.Now;
                diff = end - start;
                Backlog.Add($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] [{module.Name}: {Name}] {diff.TotalMilliseconds}");
            };
            Handlers[module][Name] = tor;
            return tor;
        }
        /// <summary>
        /// Handles event and mesures time that took to handle
        /// </summary>
        /// <typeparam name="T">Event Args Type</typeparam>
        /// <param name="module">Modue</param>
        /// <param name="action">Handler</param>
        /// <returns>Event Handler</returns>
        public static Exiled.Events.Events.CustomEventHandler<T> Handle<T>(this Module module, Action<T> action) where T : EventArgs => Generic<T>.Handle(module, action);
        private static class Generic<T> where T : EventArgs
        {
            public static readonly Dictionary<Module, Dictionary<string, Exiled.Events.Events.CustomEventHandler<T>>> TypedHandlers = new Dictionary<Module, Dictionary<string, Exiled.Events.Events.CustomEventHandler<T>>>();

            public static Exiled.Events.Events.CustomEventHandler<T> Handle(Module module, Action<T> action)
            {
                if (!TypedHandlers.ContainsKey(module))
                    TypedHandlers[module] = new Dictionary<string, Exiled.Events.Events.CustomEventHandler<T>>();
                string name = typeof(T).Name;
                if (TypedHandlers[module].ContainsKey(name))
                    return TypedHandlers[module][name];
                DateTime start;
                DateTime end;
                TimeSpan diff;
                Exiled.Events.Events.CustomEventHandler<T> tor = (ev) =>
                {
                    /*if(ev is Exiled.Events.EventArgs.InteractingDoorEventArgs)
                    {
                        Log.Warn($"[{module.Name}: {ev.GetType().Name}] Denied running {ev}");
                        return;
                    }*/
                    start = DateTime.Now;
                    try
                    {
                        action(ev);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error($"[{module.Name}: {ev.GetType().Name}] Caused Exception");
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                        ErrorBacklog.Add($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] [{module.Name}: {ev.GetType().Name}] Caused Exception");
                        ErrorBacklog.Add(ex.Message);
                        ErrorBacklog.Add(ex.StackTrace);
                        RoundLoggerSystem.RoundLogger.Log("DIAGNOSTICS", "ERROR", $"[{module.Name}: {ev.GetType().Name}] Caused Exception | {ex.Message}");
                    }
                    end = DateTime.Now;
                    diff = end - start;
                    Backlog.Add($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] [{module.Name}: {ev.GetType().Name}] {diff.TotalMilliseconds}");
                };
                TypedHandlers[module][name] = tor;
                return tor;
            }
        }

        /// <summary>
        /// Logs Time
        /// </summary>
        /// <param name="moduleName">Module Name</param>
        /// <param name="name">Handler Name</param>
        /// <param name="start">Handling start time</param>
        /// <param name="end">Handling end time</param>
        public static void LogTime(string moduleName, string name, DateTime start, DateTime end) =>
            Backlog.Add($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] [{moduleName}: {name}] {(end - start).TotalMilliseconds}");


        private static bool Initiated = false;

        private readonly static List<string> Backlog = new List<string>();
        private readonly static List<string> ErrorBacklog = new List<string>();
        internal static void Ini()
        {
            Log.Debug($"Called Ini");
            if (Initiated)
                return;
            Timing.RunCoroutine(SaveLoop());
            Initiated = true;
            RoundLoggerSystem.RoundLogger.IniIfNotAlready();
        }
        private static IEnumerator<float> SaveLoop()
        {
            Log.Debug($"Starting Loop");
            if (!Directory.Exists($"{Paths.Configs}/{Server.Port}/"))
            {
                Directory.CreateDirectory($"{Paths.Configs}/{Server.Port}/");
                Log.Debug($"{Paths.Configs}/{Server.Port}/ Created");
            }
            else
            {
                Log.Debug($"{Paths.Configs}/{Server.Port}/ Exists");
            }
            string lastDay = DateTime.Now.ToString("yyyy-MM-dd");
            string day;
            while (true)
            {
                try
                {
                    day = DateTime.Now.ToString("yyyy-MM-dd");
                    if (lastDay != day)
                    {
                        Compress($"{Paths.Configs}/{Server.Port}/{lastDay}");
                        lastDay = day;
                    }
                    if (!Directory.Exists($"{Paths.Configs}/{Server.Port}/{day}/"))
                    {
                        Directory.CreateDirectory($"{Paths.Configs}/{Server.Port}/{day}/");
                        Log.Debug($"Created {Paths.Configs}/{Server.Port}/{day}/");
                    }
                    //Log.Debug($"{Paths.Configs}/{Server.Port}/{day}/{DateTime.Now.ToString("yyyy-MM-dd_HH")}.log");
                    string path = $"{Paths.Configs}/{Server.Port}/{day}/{DateTime.Now.ToString("yyyy-MM-dd_HH")}.log";
                    if (!File.Exists(path))
                        AnalizeContent($"{Paths.Configs}/{Server.Port}/{day}/{DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd_HH")}.log");
                    File.AppendAllLines(path, Backlog);
                    Backlog.Clear();
                    File.AppendAllLines($"{Paths.Configs}/{Server.Port}/{day}/error.log", ErrorBacklog);
                    ErrorBacklog.Clear();
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }
                yield return MEC.Timing.WaitForSeconds(1);
            }
        }
        private static void Compress(string day)
        {
            try
            {
                ZipFile.CreateFromDirectory(day, $"{day}.zip");
                Directory.Delete(day, true);
            }
            catch (System.Exception ex)
            {
                Log.Error("Failed to compress");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
        }
        internal static void AnalizeContent(string file)
        {
            if (!File.Exists(file))
                return;
            var result = AnalizeContent(File.ReadAllLines(file), DateTime.Now.AddHours(-1));
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + ".analized.log"), Newtonsoft.Json.JsonConvert.SerializeObject(result));
            File.Delete(file);
        }
        private static Dictionary<string, Data> AnalizeContent(string[] lines, DateTime dateTime)
        {
            Dictionary<string, List<(float Took, DateTime Time)>> times = new Dictionary<string, List<(float Took, DateTime Time)>>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                string[] data = line.Replace("[", "").Split(']');
                string[] date = data[0].Split(':');
                var time = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2].Split('.')[0]), int.Parse(date[2].Split('.')[1]));
                string executor = string.Join(".", data[1].Trim().Replace(" ", "").Split(new string[] { ":" }, StringSplitOptions.None));
                data[2] = data[2].Replace(".", ",");
                float TimeTook = float.Parse(data[2]);
                if (!times.ContainsKey(executor))
                    times.Add(executor, new List<(float Took, DateTime Time)>());
                times[executor].Add((TimeTook, time));
            }
            Dictionary<string, Data> ProccesedData = new Dictionary<string, Data>();
            foreach (var time in times)
            {
                float min = float.MaxValue;
                float max = 0;
                float avg = 0;
                Dictionary<string, int> calls = new Dictionary<string, int>();
                foreach (var item in time.Value)
                {
                    avg += item.Took;
                    if (max < item.Took)
                        max = item.Took;
                    if (min > item.Took)
                        min = item.Took;
                    string stringTime = item.Time.ToString("yyyy-MM-dd HH-mm");
                    if (!calls.ContainsKey(stringTime))
                        calls.Add(stringTime, 0);
                    calls[stringTime]++;
                }
                float avgCalls = 0;
                foreach (var item in calls)
                    avgCalls += item.Value;
                avgCalls /= calls.Values.Count;
                avg /= time.Value.Count;
                var info = (avg, time.Value.Count, min, max, avgCalls);
                ProccesedData.Add(time.Key, new Data(info));
            }

            return ProccesedData;
        }

        internal class Data
        {
            public float Avg;
            public int Calls;
            public float Min;
            public float Max;
            public float AvgCallsPerMinute;

            public Data() { }
            public Data((float Avg, int Calls, float Min, float Max, float AvgCallsPerMinute) info)
            {
                Avg = info.Avg;
                Calls = info.Calls;
                Min = info.Min;
                Max = info.Max;
                AvgCallsPerMinute = info.AvgCallsPerMinute;
            }
        }
    }

    /// <summary>
    /// Diagnostics module
    /// </summary>
    public abstract class Module
    {
        internal static readonly Dictionary<IPlugin<IConfig>, List<Module>> Modules = new Dictionary<IPlugin<IConfig>, List<Module>>();
        /// <summary>
        /// Module Name
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// If module should be enabled
        /// </summary>
        public virtual bool Enabled { get; protected set; } = true;
        /// <summary>
        /// Plugin that this module belong to
        /// </summary>
        public readonly IPlugin<IConfig> plugin;
        /// <summary>
        /// If is requied for basic functions
        /// </summary>
        public virtual bool IsBasic { get; } = false;
        /// <summary>
        /// Used to use special logging method
        /// </summary>
        protected __Log Log => new __Log(Name);
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="plugin">Plugin creating module</param>
        public Module(IPlugin<IConfig> plugin)
        {
            this.plugin = plugin;
            if (!Modules.ContainsKey(plugin))
                Modules.Add(plugin, new List<Module>());
            Modules[plugin].RemoveAll(i => i.Name == Name);
            Modules[plugin].Add(this);
        }
        /// <summary>
        /// Enables all modules that has <see cref="Module.Enabled"/> set to <see langword="true"/> from specific plugin
        /// </summary>
        /// <param name="plugin">Plugin</param>
        public static void OnEnable(IPlugin<IConfig> plugin)
        {
            foreach (var item in Modules[plugin].Where(i => i.Enabled))
            {
                MasterHandler.Ini();
                Exiled.API.Features.Log.Debug($"Enabling {item.Name} from {plugin.Author}.{plugin.Name}");
                item.OnEnable();
                Exiled.API.Features.Log.Debug($"Enabled {item.Name} from {plugin.Author}.{plugin.Name}");
            }
        }
        /// <summary>
        /// Disables all modules that has <see cref="Module.Enabled"/> set to <see langword="true"/> from specific plugin
        /// </summary>
        /// <param name="plugin">Plugin</param>
        public static void OnDisable(IPlugin<IConfig> plugin)
        {
            foreach (var item in Modules[plugin].Where(i => i.Enabled))
            {
                MasterHandler.Ini();
                Exiled.API.Features.Log.Debug($"Disabling {item.Name} from {plugin.Author}.{plugin.Name}");
                item.OnDisable();
                Exiled.API.Features.Log.Debug($"Disabled {item.Name} from {plugin.Author}.{plugin.Name}");
            }
        }
        /// <summary>
        /// Enables all modules that has <see cref="Module.Enabled"/> set to <see langword="true"/> and <see cref="Module.IsBasic"/> set to <see langword="false"/> except from <paramref name="plugin"/>
        /// </summary>
        /// <param name="plugin">Plugin</param>
        public static void EnableAllExcept(IPlugin<IConfig> plugin)
        {
            foreach (var module in Modules.Where(p => p.Key != plugin))
            {
                foreach (var item in module.Value.Where(i => i.Enabled && !i.IsBasic))
                {
                    MasterHandler.Ini();
                    Exiled.API.Features.Log.Debug($"Enabling {item.Name} from {plugin.Author}.{plugin.Name}");
                    item.OnEnable();
                    Exiled.API.Features.Log.Debug($"Enabled {item.Name} from {plugin.Author}.{plugin.Name}");
                }
            }
        }
        /// <summary>
        /// Disables all modules that has <see cref="Module.Enabled"/> set to <see langword="true"/> and <see cref="Module.IsBasic"/> set to <see langword="false"/> except from <paramref name="plugin"/>
        /// </summary>
        /// <param name="plugin">Plugin</param>
        public static void DisableAllExcept(IPlugin<IConfig> plugin)
        {
            foreach (var module in Modules.Where(p => p.Key != plugin))
            {
                foreach (var item in module.Value.Where(i => i.Enabled && !i.IsBasic))
                {
                    MasterHandler.Ini();
                    Exiled.API.Features.Log.Debug($"Disabling {item.Name} from {plugin.Author}.{plugin.Name}");
                    item.OnDisable();
                    Exiled.API.Features.Log.Debug($"Disabled {item.Name} from {plugin.Author}.{plugin.Name}");
                }
            }
        }
        /// <summary>
        /// Called when enabling
        /// </summary>
        public abstract void OnEnable();
        /// <summary>
        /// Called when disabling
        /// </summary>
        public abstract void OnDisable();
    }
    /// <summary>
    /// Used to Log with prefix
    /// </summary>
    public class __Log
    {
        private readonly string module;
        /// <summary>
        /// Constructor
        /// </summary>
        public __Log(string module) => this.module = module;
        /// <inheritdoc cref="Log.Debug(object, bool)"/>
        public void Debug(object message, bool canBeSant = true) => Log.Debug($"[{module}] {message}", canBeSant);
        /// <inheritdoc cref="Log.Info(object)"/>
        public void Info(object message) => Log.Info($"[{module}] {message}");
        /// <inheritdoc cref="Log.Warn(object)"/>
        public void Warn(object message) => Log.Warn($"[{module}] {message}");
        /// <inheritdoc cref="Log.Error(object)"/>
        public void Error(object message) => Log.Error($"[{module}] {message}");
    }
}
