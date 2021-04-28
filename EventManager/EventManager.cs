#pragma warning disable CS0618 // Typ lub składowa jest przestarzała

using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.EventManager.EventCreator;
using Gamer.Utilities;
using Gamer.Utilities.TranslationManagerSystem;
using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Gamer.EventManager
{
    /// <inheritdoc/>
    public class EventManager : Plugin<EMConfig>
    {
        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Highest;
        /// <inheritdoc/>
        public override string Name => "EventManager";
        /// <inheritdoc/>
        public override string Author => "Gamer & Xname";

        internal static EventManager singleton;

        internal const bool DNPN = true;

        #region Vars
        /// <summary>
        /// Currenlty acitve event or null
        /// </summary>
        public static EventCreator.IEMEventClass ActiveEvent { get; internal set; }
        /// <summary>
        /// If Any Event is Acitve
        /// </summary>
        /// <returns></returns>
        public static bool EventActive() => ActiveEvent != null;

        internal static int rounds_without_event = 0;
        internal static bool ForceEnd = false;

        internal static readonly string EMLB = $"[<color=#6B9ADF><b>Event Manager</b></color> {(DNPN ? "<color=#6B9ADF>Test Build</color>" : "")}] ";
        internal Queue<EventCreator.IEMEventClass> EventQueue = new Queue<EventCreator.IEMEventClass>();
        #endregion
        /// <inheritdoc/>
        public override void OnEnabled()
        {
            singleton = this;
            new SystemsHandler(this);

            LoadEvents();

            base.OnEnabled();
        }
        /// <summary>
        /// Refreshes translations
        /// </summary>
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
        private static readonly Dictionary<EventCreator.IEMEventClass, Dictionary<string, string>> Translations = new Dictionary<IEMEventClass, Dictionary<string, string>>();
        internal static readonly Dictionary<string, EventCreator.IEMEventClass> Events = new Dictionary<string, IEMEventClass>();
        /// <summary>
        /// Loads Events
        /// </summary>
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
        /// <summary>
        /// Loads external events
        /// </summary>
        public void LoadExternalEvents()
        {
            try
            {
                Gamer.Utilities.Logger.Info("EVENT_LOADER", "Loading External Plugins Started");
                string @string = Paths.Plugins + "/Events";
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
        /// <summary>
        /// Translation
        /// </summary>
        public static string T_Event_Start = "Uruchomiono event: ";
        /// <summary>
        /// Translation
        /// </summary>
        public static string T_Event_WIN = "<color=#6B9ADF>$player</color> wygrał!";
        /// <summary>
        /// Translation
        /// </summary>
        public static string T_Event_NO_WIN = "Nikt nie wygrał.";
        /// <summary>
        /// Translation
        /// </summary>
        public static string T_Event_NUM_ALIVE = "Zostało $players <color=#6B9ADF>żywych</color>";
        #endregion
    }
    /// <inheritdoc/>
    public class EMConfig : API.Config
    {
        /// <summary>
        /// If event should automaticly enalbe
        /// </summary>
        public bool AutoEvent { get; set; } = false;
        /// <summary>
        /// Number of round
        /// </summary>
        public int AutoEvent_Rounds { get; set; } = 5;
    }
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Waits 1s, changes role, waits 1s, sets position if is not default
        /// </summary>
        /// <param name="player">player</param>
        /// <param name="role">role</param>
        /// <param name="pos">position</param>
        public static void SlowChangeRole(this Player player, RoleType role, Vector3 pos = default) => Timing.RunCoroutine(SlowFC(player, role, pos));
        private static IEnumerator<float> SlowFC(Player player, RoleType role, Vector3 pos = default)
        {
            yield return Timing.WaitForSeconds(1);
            player.Role = role;
            if (pos != default)
            {
                yield return Timing.WaitForSeconds(1);
                player.Position = pos;
            }
        }
    }

    namespace EventCreator
    {
        /// <summary>
        /// Every event base
        /// </summary>
        public interface IEMEvent
        {
            /// <summary>
            /// Event name
            /// </summary>
            string Name { get; }
            /// <summary>
            /// Event Id
            /// </summary>
            string Id { get; }
            /// <summary>
            /// Event Description
            /// </summary>
            string Description { get; }
        }
        internal interface InternalEvent
        {
        }
        /// <inheritdoc/>
        public abstract class IEMEventClass : IEMEvent
        {
            /// <summary>
            /// Is Event Active
            /// </summary>
            protected bool Running = false;
            /// <summary>
            /// If Event is active
            /// </summary>
            public bool Active => EventManager.ActiveEvent?.Id == Id;

            /// <summary>
            /// Ends event
            /// </summary>
            /// <param name="winner">winner</param>
            /// <param name="customWinText">if winner is text</param>
            public void OnEnd(string winner = null, bool customWinText = false)
            {
                if (winner == null) 
                    Map.Broadcast(10, $"{EventManager.EMLB} {EventManager.T_Event_NO_WIN}");
                else if (!customWinText) 
                    Map.Broadcast(10, $"{EventManager.EMLB} {EventManager.T_Event_WIN.Replace("$player", winner)}");
                else 
                    Map.Broadcast(10, $"{EventManager.EMLB} {winner}");
                DeInitiate();
                Round.IsLocked = false;
                EventManager.ForceEnd = true;
            }
            /// <summary>
            /// Ends event on one alive of class
            /// </summary>
            /// <param name="role"></param>
            public void EndOnOneAliveOf(RoleType role = RoleType.ClassD)
            {
                var players = RealPlayers.List.Where(x => x.Role == role).ToArray();
                if (players.Length == 1) 
                    OnEnd(players[0].Nickname);
            }
            /// <summary>
            /// Initiates event
            /// </summary>
            public void Initiate()
            {
                Log.Debug("Deinitiating modules");
                Gamer.Diagnostics.Module.DisableAllExcept(EventManager.singleton);
                Log.Debug("Deinitiated modules");
                Running = true;
                EventManager.ActiveEvent = this;
                Map.Broadcast(10, $"{EventManager.EMLB} {EventManager.T_Event_Start} <color=#6B9ADF>{Name}</color>");
                OnIni();
                if (this is ISpawnRandomItems)
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
            /// <summary>
            /// DeInitiates event
            /// </summary>
            public void DeInitiate()
            {
                OnDeIni();
                Log.Debug("Event Deactivated");
                Log.Debug("Reinitiating modules");
                Gamer.Diagnostics.Module.EnableAllExcept(EventManager.singleton);
                Log.Debug("Reinitiated modules");
                Running = false;
                EventManager.ActiveEvent = null;
            }
            /// <summary>
            /// Waits <paramref name="time"/> and calls <paramref name="action"/>
            /// </summary>
            /// <param name="time">time in s</param>
            /// <param name="action">action</param>
            protected void WaitAndExecute(float time, Action action)
            {
                Timing.CallDelayed(time, action);
            }
            /// <summary>
            /// Calls <paramref name="action"/> every <paramref name="time"/>
            /// </summary>
            /// <param name="time">time in s</param>
            /// <param name="action">action</param>
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

            /// <summary>
            /// Called on Register
            /// </summary>
            public abstract void Register();
            /// <summary>
            /// Called on Ini
            /// </summary>
            public abstract void OnIni();
            /// <summary>
            /// Called on DeIni
            /// </summary>
            public abstract void OnDeIni();

            /// <summary>
            /// Event Name
            /// </summary>
            public abstract string Name { get; set; }
            /// <summary>
            /// Event Id
            /// </summary>
            public abstract string Id { get; }
            /// <summary>
            /// Event Description
            /// </summary>
            public abstract string Description { get; set; }
            /// <summary>
            /// Translations
            /// </summary>
            public virtual Dictionary<string, string> Translations { get; }
        }
        /// <summary>
        /// Event ends on Escape
        /// </summary>
        public interface IWinOnEscape
        {
        }
        /// <summary>
        /// Event ends on Last Alive
        /// </summary>
        public interface IWinOnLastAlive
        {
        }
        /// <summary>
        /// Event ends on No alive
        /// </summary>
        public interface IEndOnNoAlive
        {
        }
        /// <summary>
        /// Announcec when someone dies
        /// </summary>
        public interface IAnnouncPlayersAlive
        {
            /// <summary>
            /// If broadcasts should be cleared before sending
            /// </summary>
            bool ClearPrevious { get; }
        }
        /// <summary>
        /// Spawns random items on start
        /// </summary>
        public interface ISpawnRandomItems
        {
        }
    }
}
