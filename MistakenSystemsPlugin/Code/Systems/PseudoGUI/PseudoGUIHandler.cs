using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Mistaken.Systems.End;
using Gamer.Utilities;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace Gamer.Mistaken.Systems.GUI
{
    public class PseudoGUIHandler : Module
    {
        public override bool IsBasic => true;
        //public override bool Enabled => false;
        public PseudoGUIHandler(PluginHandler p) : base(p)
        {
            Timing.RunCoroutine(DoRoundLoop());
        }

        public override string Name => "PseudoGUI";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Run = true;
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
            Run = false;
        }

        public enum Position
        {
            TOP,
            MIDDLE,
            BOTTOM
        }
        public static void Ignore(Player p) => ToIgnore.Add(p);
        public static void StopIgnore(Player p)
        {
            ToIgnore.Remove(p);
            ToUpdate.Add(p);
        }
        public static void Set(Player player, string key, Position type, string content, float duration)
        {
            Set(player, key, type, content);
            Timing.CallDelayed(duration, () => Set(player, key, type, null));
        }
        public static void Set(Player player, string key, Position type, string content)
        {
            bool remove = string.IsNullOrWhiteSpace(content);
            if(remove)
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
        }

        private static readonly HashSet<Player> ToIgnore = new HashSet<Player>();

        private bool Run = false;
        private DateTime start;
        private IEnumerator<float> DoRoundLoop()
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(.25f);
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

            player.ShowHint($"<size=75%>{toWrite}<br><br><br><br><br><br><br><br><br><br></size>", 7200);
            //Log.Debug($"Updating {player.Id} with {toWrite}");
        }
    }
}
