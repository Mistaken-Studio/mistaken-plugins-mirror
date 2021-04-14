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
                    prefab = new GameObject("Killer", typeof(Killer), typeof(BoxCollider));
                    prefab.layer = Layer;
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
            catch(System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return null;
            }
        }

        public void Start()
        {
            Exiled.Events.Handlers.Player.Died += Player_Died;

            Timing.RunCoroutine(DoDamage(), this.gameObject);
        }
        private void OnDestroy()
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
                    Systems.GUI.PseudoGUIHandler.Set(item, "killer", Systems.GUI.PseudoGUIHandler.Position.MIDDLE, null);
                }
                foreach (var player in InArea.ToArray())
                {
                    if (this.Selector(player) || player.Role == RoleType.Scp079)
                        continue;
                    if (player.IsGodModeEnabled || player.IsDead)
                        continue;
                    player.Hurt(this.Dmg, new DamageTypes.DamageType("*Anty Camper"), this.Message);
                    Systems.GUI.PseudoGUIHandler.Set(player, "killer", Systems.GUI.PseudoGUIHandler.Position.MIDDLE, this.Message);
                    hinted.Add(player);
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
            if (player?.IsNPC() ?? true)
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
