using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using Gamer.Utilities.TranslationManagerSystem;
using MEC;
using System.Reflection;
using Gamer.EventManager.EventCreator;
using EventManager.Events;
using Gamer.Utilities;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Exiled.API.Extensions;
using Exiled.API.Enums;

namespace Gamer.EventManager
{
    public class EventManager : Plugin<EMConfig>
    {
        public override PluginPriority Priority => PluginPriority.Highest;
        public override string Name => "EventManager";

        public override string Author => "Gamer & Xname";

        internal static EventManager singleton;

        public const bool DNPN = true;

        #region Vars
        public static EventCreator.IEMEventClass ActiveEvent { get; internal set; }
        public static bool EventActive() => ActiveEvent != null;

        internal static int rounds_without_event = 0;
        internal static bool ForceEnd = false;

        public static readonly string EMLB = $"[<color=#6B9ADF><b>Event Manager</b></color> {(DNPN ? "<color=#6B9ADF>Test Build</color>" : "")}] ";
        #endregion

        public override void OnEnabled()
        {
            singleton = this;
            new SystemsHandler(this);

            LoadEvents();

            base.OnEnabled();
        }

        public static void RefreshTranslation()
        {
            foreach (var pluginTranslations in Translations.ToArray())
            {
                foreach (var item in pluginTranslations.Value.ToArray())
                {
                    Translations[pluginTranslations.Key][item.Key] = TranslationManager.ReadTranslation(item.Key, "Event_" + pluginTranslations.Key.Id);
                }

                pluginTranslations.Key.Name = TranslationManager.ReadTranslation("Name", "Event_" + pluginTranslations.Key.Id);
                pluginTranslations.Key.Description = TranslationManager.ReadTranslation("Description", "Event_" + pluginTranslations.Key.Id);
            }
        }

        public static EventCreator.Version EMVersion { get; } = new EventCreator.Version(4, 0, 0);
        public static readonly Dictionary<EventCreator.IEMEventClass, Dictionary<string, string>> Translations = new Dictionary<IEMEventClass, Dictionary<string, string>>();
        public static readonly Dictionary<string, EventCreator.IEMEventClass> Events = new Dictionary<string, IEMEventClass>();

        public void LoadEvents()
        {
            Gamer.Utilities.Logger.Info("EVENT_LOADER", "Loading Events Started");
            Gamer.Utilities.Logger.Info("EVENT_LOADER", "Loading Internal Events Started");
            foreach (Type t in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(IEMEventClass))))
            {
                PrepareEvent(Activator.CreateInstance(t) as IEMEventClass);
            }
            Gamer.Utilities.Logger.Info("EVENT_LOADER", "Loading Internal Events Comlpeted");
            LoadExternalEvents();
            RefreshTranslation();
            Gamer.Utilities.Logger.Info("EVENT_LOADER", "Loading Events Comlpeted");
        }

        private void PrepareEvent(IEMEventClass ev)
        {
            Gamer.Utilities.Logger.Info("EVENT_LOADER", "Event loaded: " + ev.Id);
            if (!Gamer.EventManager.EventManager.EMVersion.Compatible(ev.Version))
            {
                Gamer.Utilities.Logger.Warn("EVENT_LOADER", "Trying to load an outdated event " + ev.Id + " " + ev.Version);
            }
            else
            {
                Events[ev.Id] = ev;
                Translations[ev] = new Dictionary<string, string>();
                TranslationManager.RegisterTranslation("Name", "Event_" + ev.Id, ev.Name);
                TranslationManager.RegisterTranslation("Description", "Event_" + ev.Id, ev.Description);
                foreach (var item in ev.Translations)
                {
                    Translations[ev][item.Key] = item.Value;
                    TranslationManager.RegisterTranslation(item.Key, "Event_" + ev.Id, item.Value);
                }
                //ev.SetPlugin(this);
                ev.Register();
                Gamer.Utilities.Logger.Info("EVENT_LOADER", "Event loaded: " + ev.Id);
            }     
        }

        public void LoadExternalEvents()
        {
            try
            {
                Gamer.Utilities.Logger.Info("EVENT_LOADER", "Loading External Plugins Started");
                string @string =  Paths.Plugins + "/Events";
                if (!Directory.Exists(@string))
                    Directory.CreateDirectory(@string);
                foreach (string path in Directory.GetFiles(@string))
                {
                    if (path.EndsWith(".dll"))
                    {
                        Gamer.Utilities.Logger.Debug("EVENT_LOADER", path);
                        Assembly assembly = Assembly.LoadFrom(path);
                        try
                        {
                            foreach (Type type in assembly.GetTypes())
                            {
                                if (type.IsSubclassOf(typeof(IEMEventClass)) && type != typeof(IEMEventClass))
                                {
                                    try
                                    {
                                        IEMEventClass plugin = (IEMEventClass)Activator.CreateInstance(type);
                                        if (plugin.Id != null)
                                        {
                                            PrepareEvent(plugin);
                                        }
                                        else
                                        {
                                            Gamer.Utilities.Logger.Warn("EVENT_LOADER", $"Plugin loaded but missing an id: {type?.ToString()} [{path}]");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Gamer.Utilities.Logger.Error("EVENT_LOADER", $"Failed to create instance of plugin {type?.ToString()} [{path}]");
                                        Gamer.Utilities.Logger.Error("EVENT_LOADER", ex.GetType().Name + ": " + ex.Message);
                                        Gamer.Utilities.Logger.Error("EVENT_LOADER", ex.StackTrace);
                                    }
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            Gamer.Utilities.Logger.Error("EVENT_LOADER", $"Failed to load DLL [{path}], is it up to date?");
                            Gamer.Utilities.Logger.Debug("EVENT_LOADER", ex2.Message);
                            Gamer.Utilities.Logger.Debug("EVENT_LOADER", ex2.StackTrace);
                        }

                    }
                }
                Gamer.Utilities.Logger.Info("EVENT_LOADER", "Loading External Plugins Comlpeted");
            }
            catch (Exception ex)
            {
                Gamer.Utilities.Logger.Error("EVENT_LOADER", ex.Message);
                Gamer.Utilities.Logger.Error("EVENT_LOADER", ex.StackTrace);
            }
        }

        #region Lang
        public static string T_Event_Start = "Uruchomiono event: ";
        public static string T_Event_WIN = "<color=#6B9ADF>$player</color> wygrał!";
        public static string T_Event_NO_WIN = "Nikt nie wygrał.";
        public static string T_Event_NUM_ALIVE = "Zostało $players <color=#6B9ADF>żywych</color>";
        #endregion
    }

    public class EMConfig : API.Config
    {
        public bool AutoEvent { get; set; } = false;
        public int AutoEvent_Rounds { get; set; } = 5;
    }

    public static class Functions
    {
        public static ItemType GetRandomItem()
        {
            int rand = new System.Random().Next(0, Enum.GetValues(typeof(ItemType)).ToArray<int>().Max<int>());
            return (ItemType)rand;
        }
    }

    public static class Extensions
    {
        public static void SlowChangeRole(this Player player, RoleType role, Vector3 pos = default) => Timing.RunCoroutine(SlowFC(player, role, pos));
        private static IEnumerator<float> SlowFC(Player player, RoleType role, Vector3 pos = default)
        {
            yield return Timing.WaitForSeconds(1);
            player.Role = role;
            yield return Timing.WaitForSeconds(1);
            if (pos != default) 
                player.Position = pos;
        }
    }

    namespace EventCreator
    {
        public struct Version
        {
            public int Major;
            public int Minor;
            public int Patch;

            public Version(int major, int minor, int patch)
            {
                this.Major = major;
                this.Minor = minor;
                this.Patch = patch;
            }

            public Version(string txt)
            {
                var data = txt.Split('.');
                try
                {
                    this.Major = int.Parse(data[0]);
                    this.Minor = int.Parse(data[1]);
                    this.Patch = int.Parse(data[2]);
                }
                catch (System.Exception)
                {
                    throw new Exception("Wrong version string");
                }
            }

            public bool Compatible(Version v) => this.Major == v.Major && this.Minor == v.Minor;

            public override string ToString()
            {
                return $"{Major}.{Minor}.{Patch}";
            }
        }
        public interface IEMEvent
        {
            string Name { get; }
            string Id { get; }
            string Description { get; }
        }
        internal interface InternalEvent
        {
        }
        public abstract class IEMEventClass : IEMEvent
        {
            protected bool Running = false;
            public bool Active => EventManager.ActiveEvent?.Id == this.Id;

            //protected Plugin plugin;

            //public void OnEnd(Player player = null)
            //{
            //    if (player == null)
            //        Map.Broadcast(10, $"{EventManager.EMLB} {EventManager.T_Event_NO_WIN}");
            //    else
            //        Map.Broadcast(10, $"{EventManager.EMLB} {EventManager.T_Event_WIN.Replace("$player", player.Nickname)}");
            //    DeInitiate();
            //    Round.IsLocked = false;
            //    EventManager.ForceEnd = true;
            //    //WaitAndExecute(10, () => { Round.Restart(true); });
            //}

            //public void SetPlugin(Plugin p) => this.plugin = p;

            public void OnEnd(string winner = null, bool customWinText = false)
            {
                if (winner == null) Map.Broadcast(10, $"{EventManager.EMLB} {EventManager.T_Event_NO_WIN}");
                else if (!customWinText) Map.Broadcast(10, $"{EventManager.EMLB} {EventManager.T_Event_WIN.Replace("$player", winner)}");
                else Map.Broadcast(10, $"{EventManager.EMLB} {winner}");
                DeInitiate();
                Round.IsLocked = false;
                EventManager.ForceEnd = true;
                //WaitAndExecute(10, () => { Round.Restart(true); });
            }

            public void EndOnOneAliveOf(RoleType role = RoleType.ClassD)
            {
                var players = RealPlayers.List.Where(x => x.Role == role && x.IsAlive).ToArray();
                if (players.Length == 1) OnEnd(players[0].Nickname);
            }

            public void Initiate()
            {
                Log.Debug("Deinitiating modules");
                Gamer.Diagnostics.Module.DisableAllExcept(EventManager.singleton);
                Log.Debug("Deinitiated modules");
                /*EventData eventData = new EventData(this);
                foreach (var item in EventManager.EventHandlers.ToArray())
                {
                    eventData.ExecuteHandler(item.Value);
                }
                if (!eventData.Allow) return;*/
                Running = true;
                //plugin = p;
                EventManager.ActiveEvent = this;
                Map.Broadcast(10, $"{EventManager.EMLB} {EventManager.T_Event_Start} <color=#6B9ADF>{this.Name}</color>");
                OnIni();
                if(this is ISpawnRandomItems)
                {
                    foreach (var item in Map.Rooms)
                    {
                        int rand = UnityEngine.Random.Range(0, 10);
                        switch (rand)
                        {
                            case 0:
                                ItemType.GunCOM15.Spawn(UnityEngine.Random.Range(0, 12), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(4, 15), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(4, 15), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                            case 1:
                                ItemType.GunUSP.Spawn(UnityEngine.Random.Range(0, 18), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(4, 12), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(4, 12), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                            case 2:
                                ItemType.GunMP7.Spawn(UnityEngine.Random.Range(0, 35), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo762.Spawn(UnityEngine.Random.Range(5, 35), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo762.Spawn(UnityEngine.Random.Range(5, 35), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                            case 3:
                                ItemType.GunProject90.Spawn(UnityEngine.Random.Range(0, 50), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(5, 40), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo9mm.Spawn(UnityEngine.Random.Range(5, 40), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                            case 4:
                                ItemType.GunE11SR.Spawn(UnityEngine.Random.Range(0, 40), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo556.Spawn(UnityEngine.Random.Range(5, 30), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo556.Spawn(UnityEngine.Random.Range(5, 30), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                            case 5:
                                ItemType.GunLogicer.Spawn(UnityEngine.Random.Range(0, 75), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo762.Spawn(UnityEngine.Random.Range(10, 50), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                ItemType.Ammo762.Spawn(UnityEngine.Random.Range(10, 50), item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                            case 6:
                                ItemType.WeaponManagerTablet.Spawn(float.MaxValue, item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                            case 7:
                                ItemType.Medkit.Spawn(2, item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                            case 8:
                                ItemType.Adrenaline.Spawn(1, item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                            case 9:
                                ItemType.Painkillers.Spawn(5, item.Position + new Vector3(0, 1, 0), Quaternion.identity);
                                break;
                        }
                    }
                }
            }

            public virtual void DeInitiate()
            {
                OnDeIni();
                Log.Debug("Event Deactivated");
                Log.Debug("Reinitiating modules");
                Gamer.Diagnostics.Module.EnableAllExcept(EventManager.singleton);
                Log.Debug("Reinitiated modules");
                //plugin = null;
                Running = false;
                EventManager.ActiveEvent = null;
            }

            protected void WaitAndExecute(float time, Action action)
            {
                Timing.RunCoroutine(IEWaitAndExecute(time, action));
            }

            protected void WaitAndExecuteLoop(float time, Action action)
            {
                Timing.RunCoroutine(IEWaitAndExecute(time, () => 
                {
                    action?.Invoke();
                    WaitAndExecuteLoop(time, action);
                }));
            }

            private IEnumerator<float> IEWaitAndExecute(float time, Action action)
            {
                yield return Timing.WaitForSeconds(time);
                action?.Invoke();
            }


            public abstract void Register();
            public abstract void OnIni();
            public abstract void OnDeIni();

            public abstract string Name { get; set; }
            public abstract string Id { get; }
            public abstract string Description { get; set; }
            public abstract Version Version { get; }
            public abstract Dictionary<string, string> Translations { get; }
        }

        public interface IWinOnEscape
        {
        }
        public interface IWinOnLastAlive
        {
        }
        public interface IEndOnNoAlive
        {
        }

        public interface IAnnouncPlayersAlive
        {
            bool ClearPrevious { get; }
        }
        public interface ISpawnRandomItems
        {
        }
    }
}
