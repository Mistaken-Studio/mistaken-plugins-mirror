#pragma warning disable IDE0079
#pragma warning disable IDE0060

using Exiled.API.Features;
using Gamer.Diagnostics;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.BetterSCP.SCP0492
{
    internal class SCP0492Handler : Diagnostics.Module
    {
        public SCP0492Handler(PluginHandler p) : base(p)
        {
        }

        public override bool Enabled => false;
        public override string Name => nameof(SCP0492Handler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Died += this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.Died -= this.Handle<Exiled.Events.EventArgs.DiedEventArgs>((ev) => Player_Died(ev));
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {

        }

        private void Server_RoundStarted()
        {
            if (Server.Port >= 7790)
                Spawn(Map.Rooms.FirstOrDefault(r => r.Type == Exiled.API.Enums.RoomType.Hcz049));
            else
                Log.Debug("Not Test server");
        }

        public static void Spawn(Room room) => Timing.RunCoroutine(SpawnNPCZombie(room));

        private static IEnumerator<float> SpawnNPCZombie(Room room)
        {
            yield return Timing.WaitForSeconds(1);
            while (Round.IsStarted)
            {
                for (int i = 0; i < 20; i++)
                {
                    NPCS.Methods.CreateNPC(room.Position + Vector3.up, Vector2.zero, "zombie.yml");
                }

                yield return Timing.WaitForSeconds(60 * 5);
            }
        }
    }
}
