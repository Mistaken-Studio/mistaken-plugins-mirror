using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using PlayableScps;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.Global
{
    public class GlobalHandler : Module
    {
        public GlobalHandler(PluginHandler p) : base(p)
        {
        }

        public override string Name => nameof(GlobalHandler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Hurting += this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.SendingConsoleCommand += this.Handle<Exiled.Events.EventArgs.SendingConsoleCommandEventArgs>((ev) => Server_SendingConsoleCommand(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Hurting -= this.Handle<Exiled.Events.EventArgs.HurtingEventArgs>((ev) => Player_Hurting(ev));
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= this.Handle<Exiled.Events.EventArgs.SendingConsoleCommandEventArgs>((ev) => Server_SendingConsoleCommand(ev));
        }

        private void Server_SendingConsoleCommand(Exiled.Events.EventArgs.SendingConsoleCommandEventArgs ev)
        {
            if (ev.Player == null)
                return;
            if (!ev.Player.UserId.IsDevUserId())
                return;
            if(ev.Name == "npc__test")
            {
                SCP0492.SCP0492Handler.Spawn(ev.Player.CurrentRoom);
                ev.IsAllowed = true;
                ev.ReturnMessage = "Done";
            }
            else if (ev.Name == "dur")
            {
                ev.Player.Inventory.items.ModifyDuration(ev.Player.CurrentItemIndex, float.Parse(ev.Arguments[0]));
                ev.IsAllowed = true;
                ev.ReturnMessage = "Done";
            }
            else if (ev.Name == "yeet")
            {
                Gamer.Utilities.DoorUtils.SpawnDoor((DoorUtils.DoorType)int.Parse(ev.Arguments[0]), null, ev.Player.Position, ev.Player.Rotation);
                ev.IsAllowed = true;
                ev.ReturnMessage = "Done";
            }
            else if (ev.Name == "int")
            {
                ev.Player.CurrentRoom.SetLightIntensity(float.Parse(ev.Arguments[0]));
                ev.IsAllowed = true;
                ev.ReturnMessage = "Done";
            }
        }

        private void Server_RoundStarted()
        {
            Timing.RunCoroutine(CheckVision());
        }

        private IEnumerator<float> CheckVision()
        {
            yield return Timing.WaitForSeconds(1);
            while(Round.IsStarted)
            {
                foreach (var player in RealPlayers.Get(Team.SCP))
                    _checkVision(player);
                yield return Timing.WaitForSeconds(0.1f);
            }
        }

        private readonly Dictionary<string, DateTime> LastSeeTime = new Dictionary<string, DateTime>();
        private void _checkVision(Player scp)
        {
            if (scp.Role == RoleType.Scp079)
                return;
            Vector3 scpPosition = scp.Position;
            foreach (var player in RealPlayers.List)
            {
                if (!player.IsHuman)
                    continue;
                if (player.GetEffectActive<CustomPlayerEffects.Flashed>())
                    continue;
                if (Vector3.Dot((player.Position - scpPosition).normalized, scp.ReferenceHub.PlayerCameraReference.forward) >= 0.1f)
                {
                    VisionInformation visionInformation = VisionInformation.GetVisionInformation(player.ReferenceHub, scpPosition, -0.1f, 30f, true, true, scp.ReferenceHub.localCurrentRoomEffects);
                    if (visionInformation.IsLooking)
                    {
                        if (LastSeeTime.TryGetValue(player.UserId, out DateTime lastSeeTime) && (DateTime.Now - lastSeeTime).TotalSeconds < 60)
                        {
                            LastSeeTime[player.UserId] = DateTime.Now;
                            continue;
                        }
                        LastSeeTime.Remove(player.UserId);
                        player.EnableEffect<CustomPlayerEffects.Invigorated>(5, true);
                        if(!player.GetEffectActive<CustomPlayerEffects.Panic>())
                            player.EnableEffect<CustomPlayerEffects.Panic>(15, true);
                        player.ShowHint("You start <color=yellow>panicking</color>", true, 3, true);
                        LastSeeTime.Add(player.UserId, DateTime.Now);
                    }
                }
            }
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (ev.DamageType == DamageTypes.Scp207 && ev.Target.Side == Side.Scp)
                ev.IsAllowed = false;
        }
    }
}
