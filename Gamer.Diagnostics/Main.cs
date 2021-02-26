using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace Gamer.Diagnostics
{
    public static class MasterHandler
    {
        public static Exiled.Events.Events.CustomEventHandler Handle(this Module module, Action action, string Name)
        {
            DateTime start;
            DateTime end;
            TimeSpan diff;
            return () =>
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
                    ErrorBacklog.Add($"[{module.Name}: {Name}] Caused Exception");
                    ErrorBacklog.Add(ex.Message);
                    ErrorBacklog.Add(ex.StackTrace);
                }
                end = DateTime.Now;
                diff = end - start;
                Backlog.Add($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] [{module.Name}: {Name}] {diff.TotalMilliseconds}");
            };
        }

        public static Exiled.Events.Events.CustomEventHandler<T> Handle<T>(this Module module, Action<T> action) where T : EventArgs
        {
            DateTime start;
            DateTime end;
            TimeSpan diff;
            return (ev) =>
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
                catch(System.Exception ex)
                {
                    Log.Error($"[{module.Name}: {ev.GetType().Name}] Caused Exception");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                    ErrorBacklog.Add($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] [{module.Name}: {ev.GetType().Name}] Caused Exception");
                    ErrorBacklog.Add(ex.Message);
                    ErrorBacklog.Add(ex.StackTrace);
                }
                end = DateTime.Now;
                diff = end - start;
                Backlog.Add($"[{DateTime.Now.ToString("HH:mm:ss.fff")}] [{module.Name}: {ev.GetType().Name}] {diff.TotalMilliseconds}");
            };
        }

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
                    File.AppendAllLines($"{Paths.Configs}/{Server.Port}/{day}/{DateTime.Now.ToString("yyyy-MM-dd_HH")}.log", Backlog);
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
            catch(System.Exception ex)
            {
                Log.Error("Failed to compress");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
        }
    }

    public abstract class Module
    {
        internal static readonly Dictionary<IPlugin<IConfig>, List<Module>> Modules = new Dictionary<IPlugin<IConfig>, List<Module>>();
        public abstract string Name { get; }
        public virtual bool Enabled { get; protected set; } = true;
        public readonly IPlugin<IConfig> plugin;

        public Module(IPlugin<IConfig> plugin)
        {
            this.plugin = plugin;
            if (!Modules.ContainsKey(plugin))
                Modules.Add(plugin, new List<Module>());
            Modules[plugin].RemoveAll(i => i.Name == this.Name);
            Modules[plugin].Add(this);
        }

        public static void OnEnable(IPlugin<IConfig> plugin)
        {
            foreach (var item in Modules[plugin].Where(i => i.Enabled))
            {
                MasterHandler.Ini();
                Log.Debug($"Enabling {item.Name} from {plugin.Author}.{plugin.Name}");
                item.OnEnable();
                Log.Debug($"Enabled {item.Name} from {plugin.Author}.{plugin.Name}");
            }
        }
        public static void OnDisable(IPlugin<IConfig> plugin)
        {
            foreach (var item in Modules[plugin].Where(i => i.Enabled))
            {
                MasterHandler.Ini();
                Log.Debug($"Disabling {item.Name} from {plugin.Author}.{plugin.Name}");
                item.OnDisable();
                Log.Debug($"Disabled {item.Name} from {plugin.Author}.{plugin.Name}");
            }
        }

        public abstract void OnEnable();
        public abstract void OnDisable();
    }
}
