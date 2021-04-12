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
    internal class PseudoGUIHandler : Module
    {
        public override bool IsBasic => true;
        //public override bool Enabled => false;
        public PseudoGUIHandler(PluginHandler p) : base(p)
        {            
        }

        public override string Name => "PseudoGUI";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound += this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.RestartingRound -= this.Handle(() => Server_RestartingRound(), "RoundRestart");
        }

        public enum Position
        {
            TOP,
            MIDDLE,
            BOTTOM
        }
        private static readonly Dictionary<Player, Dictionary<string, (string Content, Position Type)>> CustomInfo = new Dictionary<Player, Dictionary<string, (string Content, Position Type)>>();
        private static readonly List<Player> ToUpdate = new List<Player>();
        private void Server_RestartingRound()
        {
            CustomInfo.Clear();
            ToUpdate.Clear();
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(DoRoundLoop());
        }

        private IEnumerator<float> DoRoundLoop()
        {
            yield return Timing.WaitForSeconds(1);
            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(1);
                if (ToUpdate.Count == 0)
                    continue;
                foreach (var item in ToUpdate.ToArray())
                {
                    try
                    {
                        if (item.IsConnected)
                            Update(item);
                        ToUpdate.Remove(item);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }
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
                            bottomContent += "<br>";
                            bottomLines++;
                        }

                        bottomContent += item.Content;
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

            player.ShowHint(toWrite, 3600);
        }
    }
}
