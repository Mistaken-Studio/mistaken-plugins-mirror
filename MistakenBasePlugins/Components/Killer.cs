using Exiled.API.Features;
using Gamer.Mistaken.Base.GUI;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using MEC;
using NPCS;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Base.Components
{
    public class Killer : MonoBehaviour
    {
        public bool InstaKill;
        public float Dmg;
        public Func<Player, bool> Selector;
        public string Message;
        private static GameObject prefab;
        private static GameObject Prefab
        {
            get
            {
                if (prefab == null)
                {
                    prefab = new GameObject("Killer", typeof(Killer), typeof(BoxCollider))
                    {
                        layer = Layer
                    };
                    var collider = prefab.GetComponent<BoxCollider>();
                    collider.isTrigger = true;
                }
                return prefab;
            }
        }
        private static readonly int Layer = LayerMask.GetMask("TransparentFX", "Ignore Raycast");
        public static Killer Spawn(Vector3 pos, Vector3 size, float dmg = 5, string message = "You can't stay here", bool instaKill = false, Func<Player, bool> selector = null)
        {
            try
            {
                Log.Debug($"Spawning killer on ({pos.x}, {pos.y}, {pos.z}) with size ({size.x}, {size.y}, {size.z})");
                var obj = GameObject.Instantiate(Prefab, pos, Quaternion.identity);
                var killer = obj.GetComponent<Killer>();
                killer.Dmg = dmg;
                killer.Message = message;
                killer.Selector = selector ?? (p => true);
                killer.InstaKill = instaKill;
                obj.GetComponent<BoxCollider>().size = size;

                return killer;
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return null;
            }
        }

        public void Start()
        {
            Exiled.Events.Handlers.Player.Died += Player_Died;

            Timing.RunCoroutine(DoDamage(), gameObject);
        }
#pragma warning disable IDE0051 // Usuń nieużywane prywatne składowe
        private void OnDestroy()
#pragma warning restore IDE0051 // Usuń nieużywane prywatne składowe
        {
            Exiled.Events.Handlers.Player.Died -= Player_Died;
            destroyed = true;
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            InArea.Remove(ev.Target);
            ColliderInArea.Remove(ev.Target.GameObject);
        }

        private bool destroyed = false;
        private IEnumerator<float> DoDamage()
        {
            HashSet<Player> hinted = new HashSet<Player>();
            while (!destroyed && !InstaKill)
            {
                //Log.Debug("In killers loop");
                yield return Timing.WaitForSeconds(1);
                foreach (var item in hinted.Where(p => !InArea.Contains(p)))
                {
                    item.SetGUI("killer", Base.GUI.PseudoGUIHandler.Position.MIDDLE, null);
                }
                foreach (var player in InArea.ToArray())
                {
                    if (Selector(player) || player.Role == RoleType.Scp079)
                        continue;
                    if (player.IsGodModeEnabled || player.IsDead)
                        continue;
                    player.Hurt(Dmg, new DamageTypes.DamageType("*Anty Camper"), Message);
                    player.SetGUI("killer", Base.GUI.PseudoGUIHandler.Position.MIDDLE, Message);
                    hinted.Add(player);
                    RoundLogger.Log("KILLER", "DAMAGE", $"{player.PlayerToString()} was damaged({Dmg}) with message \"{Message}\"");
                }
            }
        }

        private readonly List<Player> InArea = new List<Player>();
        private readonly HashSet<GameObject> ColliderInArea = new HashSet<GameObject>();
#pragma warning disable IDE0051 // Usuń nieużywane prywatne składowe
        private void OnTriggerEnter(Collider other)
#pragma warning restore IDE0051 // Usuń nieużywane prywatne składowe
        {
            if (!other.GetComponent<CharacterClassManager>())
                return;
            var player = Player.Get(other.gameObject);
            if (player?.IsNPC() ?? true)
                return;
            ColliderInArea.Add(other.gameObject);
            if (!InArea.Contains(player))
                InArea.Add(player);
            if (InstaKill)
            {
                if (Selector(player) || player.Role == RoleType.Scp079)
                    return;
                if (player.IsGodModeEnabled || player.IsDead)
                    return;
                player.Kill(Message);
                RoundLogger.Log("KILLER", "KILL", $"{player.PlayerToString()} was killed with message \"{Message}\"");
            }
        }

#pragma warning disable IDE0051 // Usuń nieużywane prywatne składowe
        private void OnTriggerExit(Collider other)
#pragma warning restore IDE0051 // Usuń nieużywane prywatne składowe
        {
            if (!ColliderInArea.Contains(other.gameObject))
                return;
            ColliderInArea.Remove(other.gameObject);
            var player = Player.Get(other.gameObject);
            InArea.Remove(player);
        }
    }
}
