using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.End
{
    internal class FireworkManager : Module
    {
        public FireworkManager(PluginHandler p) : base(p)
        {
        }

        public override string Name => "Firework";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Map.ExplodingGrenade += this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Map.ExplodingGrenade -= this.Handle<Exiled.Events.EventArgs.ExplodingGrenadeEventArgs>((ev) => Map_ExplodingGrenade(ev));
        }


        private void Map_ExplodingGrenade(Exiled.Events.EventArgs.ExplodingGrenadeEventArgs ev)
        {
            if (Grenades.Contains(ev.Grenade))
            {
                ev.IsAllowed = false;
                return;
            }
        }

        private void Server_RoundStarted()
        {
            if ((DateTime.Now.Day == 31 && DateTime.Now.Month == 12 && DateTime.Now.Hour >= 20) || (DateTime.Now.Day == 1 && DateTime.Now.Month == 1 && DateTime.Now.Hour < 2))
                Timing.RunCoroutine(Loop());
        }
        private static readonly HashSet<GameObject> Grenades = new HashSet<GameObject>();
        private IEnumerator<float> Loop()
        {
            yield return Timing.WaitForSeconds(1);
            var grenadeManager = Server.Host.GrenadeManager;
            Grenades.GrenadeSettings settings = grenadeManager.availableGrenades[0];
            int rid = RoundPlus.RoundId; while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                if (RealPlayers.List.Any(p => p.IsAlive && p.Position.y > 800))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        Grenades.Grenade component = UnityEngine.Object.Instantiate(settings.grenadeInstance).GetComponent<Grenades.Grenade>();
                        component.InitData(grenadeManager, new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(10f, 100f), UnityEngine.Random.Range(-5f, 5f)), Vector3.up);
                        component.transform.position = Spawns[UnityEngine.Random.Range(0, Spawns.Length)];
                        Grenades.Add(component.gameObject);
                        if (Grenades.Count > 100)
                            Grenades.Remove(Grenades.First());
                        NetworkServer.Spawn(component.gameObject);
                        yield return Timing.WaitForSeconds(UnityEngine.Random.Range(0.01f, 0.1f));
                    }
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        public static Vector3[] Spawns = new Vector3[]
        {
            new Vector3(0, 1020, 0),
            new Vector3(55, 1020, -55),
            new Vector3(5, 1020, -93),
            new Vector3(27, 1020, -30),
            new Vector3(171, 1010, -21),
            new Vector3(213, 1010, -17),
            new Vector3(190, 1010, -100),
            new Vector3(151, 1010, -75),
            new Vector3(178, 1010, 30),
            new Vector3(-97, 1010, -59)
        };
    }
}
