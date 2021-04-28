using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using UnityEngine;

namespace Gamer.Mistaken.Systems.InfoMessage
{
    public class InfoMessageManager : Module
    {
        public readonly static Dictionary<RoleType, string> WelcomeMessages = new Dictionary<RoleType, string>();

        public InfoMessageManager(PluginHandler p) : base(p)
        {
            p.RegisterTranslation("Info_SCP_Swap", "You can type \".swapscp\" to <b>change</b> <color=red>SCP</color> for next <color=yellow>{0}</color> seconds");
            p.RegisterTranslation("Info_SCP_List", "You are <color=red>SCP</color> with:");
            p.RegisterTranslation("Info_SCP_List_Element", "<color=yellow>{0}</color> as <color=red>{1}</color>");
        }

        public override string Name => "InfoMessageManager";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.ChangingRole -= this.Handle<Exiled.Events.EventArgs.ChangingRoleEventArgs>((ev) => Player_ChangingRole(ev));
        }

        private static void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if(ev.NewRole.GetSide() == Exiled.API.Enums.Side.Scp)
                Timing.RunCoroutine(UpdateSCPs(ev.Player));
            SpawnTimes[ev.Player] = DateTime.Now;
        }

        public readonly static Dictionary<Player, DateTime> SpawnTimes = new Dictionary<Player, DateTime>();

        public static TimeSpan TimeSinceChangedRole(Player player) => 
            SpawnTimes.ContainsKey(player) ? DateTime.Now - SpawnTimes[player] : default;

        private static IEnumerator<float> UpdateSCPs(Player p)
        {
            yield return Timing.WaitForSeconds(1);
            Mistaken.Base.GUI.PseudoGUIHandler.Ignore(p);
            for (int i = 0; i < 30; i++)
            {
                GetSCPS(p);
                yield return Timing.WaitForSeconds(1);
            }
            Mistaken.Base.GUI.PseudoGUIHandler.StopIgnore(p);
        }

        private static void GetSCPS(Player p)
        {
            DateTime start = DateTime.Now;
            List<string> message = NorthwoodLib.Pools.ListPool<string>.Shared.Rent();
            message.Add("<br><br><br>");
            if (p.Role != RoleType.Scp0492 && 30 - Round.ElapsedTime.TotalSeconds > 0)
                message.Add(PluginHandler.Instance.ReadTranslation("Info_SCP_Swap", Mathf.RoundToInt(30 - (float)Round.ElapsedTime.TotalSeconds)));
            if (RealPlayers.Get(Team.SCP).Count() > 1)
                message.Add(PluginHandler.Instance.ReadTranslation("Info_SCP_List"));

            foreach (var player in RealPlayers.List.Where(player => player.Team == Team.SCP && player.Role != RoleType.Scp0492 && p.Id != player.Id))
                message.Add(PluginHandler.Instance.ReadTranslation("Info_SCP_List_Element", player?.Nickname, player?.Role.ToString().ToUpper()));
            string fullmsg = string.Join("<br>", message);
            if (TimeSinceChangedRole(p).TotalSeconds < 30 && WelcomeMessages.TryGetValue(p.Role, out string roleMessage))
                fullmsg = $"<size=40><voffset=20em>{roleMessage}<br><br><br><size=90%>{fullmsg}</size><br><br><br><br><br><br><br><br><br><br></voffset></size>";
#pragma warning disable CS0618
            p.ShowHint(fullmsg, true, 2, true);
#pragma warning restore CS0618
            NorthwoodLib.Pools.ListPool<string>.Shared.Return(message);
            Diagnostics.MasterHandler.LogTime("InfoMessageManager", "GetSCPS", start, DateTime.Now);
        }
    }
}
