using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using System;
using System.Collections.Generic;

namespace Gamer.Mistaken.Base.GUI
{
    /// <inheritdoc/>
    public class PseudoGUIHandler : Module
    {
        /// <inheritdoc/>
        public override bool IsBasic => true;
        //public override bool Enabled => false;
        /// <inheritdoc/>
        public PseudoGUIHandler(PluginHandler p) : base(p)
        {
            Timing.RunCoroutine(DoLoop());
        }
        /// <inheritdoc/>
        public override string Name => "PseudoGUI";
        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Run = true;
        }
        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Run = false;
        }
        /// <summary>
        /// Content position
        /// </summary>
        public enum Position
        {
#pragma warning disable CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
            TOP,
            MIDDLE,
            BOTTOM
#pragma warning restore CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
        }
        /// <summary>
        /// Stops updating GUI
        /// </summary>
        /// <param name="p">player to ignore</param>
        public static void Ignore(Player p) => ToIgnore.Add(p);
        /// <summary>
        /// Starts updating GUI
        /// </summary>
        /// <param name="p">player to stop ignoring</param>
        public static void StopIgnore(Player p)
        {
            ToIgnore.Remove(p);
            ToUpdate.Add(p);
        }
        private static readonly Dictionary<string, uint> SetIds = new Dictionary<string, uint>();
        private static uint SetId = 0;
        internal static void Set(Player player, string key, Position type, string content, float duration)
        {
            uint localId = SetId++;
            SetIds[key] = localId;
            Set(player, key, type, content);
            Timing.CallDelayed(duration, () =>
            {
                //if (localId == SetIds[key])
                Set(player, key, type, null);
            });
        }
        internal static void Set(Player player, string key, Position type, string content)
        {
            bool remove = string.IsNullOrWhiteSpace(content);
            if (remove)
            {
                if (!CustomInfo.TryGetValue(player, out Dictionary<string, (string, Position)> value) || !value.ContainsKey(key))
                    return;
                value.Remove(key);
            }
            else
            {
                if (!CustomInfo.ContainsKey(player))
                    CustomInfo[player] = new Dictionary<string, (string Content, Position Type)>();
                else if (CustomInfo[player].TryGetValue(key, out (string Conetent, Position Type) value) && value.Conetent == content)
                    return;
                CustomInfo[player][key] = (content, type);
            }
            ToUpdate.Add(player);
        }
        private static readonly Dictionary<Player, Dictionary<string, (string Content, Position Type)>> CustomInfo = new Dictionary<Player, Dictionary<string, (string Content, Position Type)>>();
        private static readonly List<Player> ToUpdate = new List<Player>();
        private void Server_RestartingRound()
        {
            CustomInfo.Clear();
            ToUpdate.Clear();
            ToIgnore.Clear();
            SetId = 0;
            SetIds.Clear();
        }

        private static readonly HashSet<Player> ToIgnore = new HashSet<Player>();

        private bool Run = false;
        private DateTime start;
        private IEnumerator<float> DoLoop()
        {
            int i = 0;
            while (true)
            {
                yield return Timing.WaitForSeconds(.25f);
                i += 1;
                if (i >= 40)
                {
                    start = DateTime.Now;
                    foreach (var item in RealPlayers.List)
                    {
                        try
                        {
                            if (item.IsConnected && !ToIgnore.Contains(item))
                                Update(item);
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error(ex.Message);
                            Log.Error(ex.StackTrace);
                        }
                    }
                    ToUpdate.Clear();
                    i = 0;
                    Diagnostics.MasterHandler.LogTime("PseudoGUI", "RoundLoop", start, DateTime.Now);
                    continue;
                }
                if (!Run)
                    continue;
                if (ToUpdate.Count == 0)
                    continue;
                start = DateTime.Now;
                foreach (var item in ToUpdate.ToArray())
                {
                    try
                    {
                        if (item.IsConnected && !ToIgnore.Contains(item))
                            Update(item);
                        ToUpdate.Remove(item);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }
                Diagnostics.MasterHandler.LogTime("PseudoGUI", "RoundLoop", start, DateTime.Now);
            }
        }

        private void Update(Player player)
        {
            if (!CustomInfo.ContainsKey(player))
                CustomInfo[player] = new Dictionary<string, (string Content, Position Type)>();

            string topContent = "";
            int topLines = 0;

            string middleContent = "";
            int middleLines = 0;

            string bottomContent = "";
            int bottomLines = 0;

            foreach (var item in CustomInfo[player].Values)
            {
                int lines = item.Content.Split(new string[] { "<br>" }, StringSplitOptions.None).Length;
                switch (item.Type)
                {
                    case Position.TOP: //18
                        if (topContent.Length > 0)
                        {
                            topContent += "<br>";
                            topLines++;
                        }

                        topContent += item.Content;
                        topLines += lines;
                        break;
                    case Position.MIDDLE:
                        if (middleContent.Length > 0)
                        {
                            middleContent += "<br>";
                            middleLines++;
                        }

                        middleContent += item.Content;
                        middleLines += lines;
                        break;
                    case Position.BOTTOM: //15
                        if (bottomContent.Length > 0)
                        {
                            bottomContent = "<br>" + bottomContent;
                            bottomLines++;
                        }

                        bottomContent = item.Content + bottomContent;
                        bottomLines += lines;
                        break;
                }
            }

            string toWrite = "";
            toWrite += topContent;
            int linesToAddTop = 18 - topLines - (middleLines - (middleLines % 2)) / 2;
            for (int i = 0; i < linesToAddTop; i++)
                toWrite += "<br>";
            toWrite += middleContent;
            int linesToAddBottom = 15 - bottomLines - (middleLines - (middleLines % 2)) / 2;
            for (int i = 0; i < linesToAddBottom; i++)
                toWrite += "<br>";
            toWrite += bottomContent;

            player.ShowHint($"<size=75%><color=#FFFFFFFF>{toWrite}</color><br><br><br><br><br><br><br><br><br><br></size>", 7200);
            //Log.Debug($"Updating {player.Id} with {toWrite}");
        }
    }
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Sets GUI Element
        /// </summary>
        /// <param name="player">target</param>
        /// <param name="key">key</param>
        /// <param name="type">position</param>
        /// <param name="content">content</param>
        /// <param name="duration">duration</param>
        public static void SetGUI(this Player player, string key, PseudoGUIHandler.Position type, string content, float duration) =>
            PseudoGUIHandler.Set(player, key, type, content, duration);
        /// <summary>
        /// Sets GUI Element
        /// </summary>
        /// <param name="player">target</param>
        /// <param name="key">key</param>
        /// <param name="type">position</param>
        /// <param name="content">content</param>
        public static void SetGUI(this Player player, string key, PseudoGUIHandler.Position type, string content) =>
            PseudoGUIHandler.Set(player, key, type, content);
    }
}
