using Exiled.API.Features;
using Gamer.RoundLoggerSystem;
using Gamer.Utilities;
using MEC;
using NPCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Components
{
    internal class Killer : MonoBehaviour
    {
        public bool InstaKill;
        public float Dmg;
        public Func<Player, bool> Selector;
        public string Message;
        private static readonly GameObject Prefab = new GameObject("Killer", typeof(Killer), /*typeof(Rigidbody),*/ typeof(BoxCollider));
        public static Killer Spawn(Vector3 pos, Vector3 size, Quaternion rotation = default, float dmg = 5, string message = "You can't stay here", bool instaKill = false, Func<Player, bool> selector = null)
        {
            Log.Debug($"Spawning killer na ({pos.x}, {pos.y}, {pos.z}) with size ({size.x}, {size.y}, {size.z})");
            var obj = GameObject.Instantiate(Prefab, pos, Quaternion.identity);
            obj.layer = LayerMask.GetMask("TransparentFX", "Ignore Raycast");
            var killer = obj.GetComponent<Killer>();
            killer.Dmg = dmg;
            killer.Message = message;
            killer.Selector = selector ?? (p => true);
            killer.InstaKill = instaKill;
            var collider = obj.GetComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;

            return killer;
        }
        private static readonly List<Killer> Killers = new List<Killer>();

        public void Start()
        {
            Exiled.Events.Handlers.Player.Died +=  Player_Died;

            Killers.Add(this);
            Log.Debug("Start");
            Timing.RunCoroutine(DoDamage(), this.gameObject);
        }
        private void OnDestroy()
        {
            Exiled.Events.Handlers.Player.Died -= Player_Died;
            Killers.Remove(this);
            Log.Debug("Destroyed");
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
            Log.Debug("Begin killers loop");
            while (!destroyed && !InstaKill)
            {
                //Log.Debug("In killers loop");
                yield return Timing.WaitForSeconds(1);
                foreach (var player in InArea.ToArray())
                {
                    if (this.Selector(player) || player.Role == RoleType.Scp079)
                        continue;
                    if (player.IsGodModeEnabled || player.IsDead)
                        continue;
                    player.Hurt(this.Dmg, new DamageTypes.DamageType("*Anty Camper"), this.Message);
                    player.ShowHint(this.Message, 1.1f);
                    RoundLogger.Log("KILLER", "DAMAGE", $"{player.PlayerToString()} was damaged({this.Dmg}) with message \"{this.Message}\"");
                }
            }
        }

        private readonly List<Player> InArea = new List<Player>();
        private readonly HashSet<GameObject> ColliderInArea = new HashSet<GameObject>();
        private void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<CharacterClassManager>())
                return;
            var player = Player.Get(other.gameObject);
            if (player.IsNPC())
                return;
            ColliderInArea.Add(other.gameObject);
            if (!InArea.Contains(player))
                InArea.Add(player);
            if(InstaKill)
            {
                if (this.Selector(player) || player.Role == RoleType.Scp079)
                    return;
                if (player.IsGodModeEnabled || player.IsDead)
                    return;
                player.Kill(this.Message);
                RoundLogger.Log("KILLER", "KILL", $"{player.PlayerToString()} was killed with message \"{this.Message}\"");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!ColliderInArea.Contains(other.gameObject))
                return;
            ColliderInArea.Remove(other.gameObject);
            var player = Player.Get(other.gameObject);
            InArea.Remove(player);
        }
    }
}
