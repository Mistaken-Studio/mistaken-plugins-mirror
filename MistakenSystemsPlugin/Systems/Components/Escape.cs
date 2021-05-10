using Exiled.API.Features;
using NPCS;
using System;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Components
{
    public class Escape : MonoBehaviour
    {
        private Action<Player> OnTrigger;
        private static GameObject prefab;
        private static GameObject Prefab
        {
            get
            {
                if (prefab == null)
                {
                    prefab = new GameObject("Escape", typeof(Escape), typeof(BoxCollider))
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
        public static Escape Spawn(Vector3 pos, Vector3 size, Action<Player> onTrigger)
        {
            try
            {
                Log.Debug($"Spawning Escape on ({pos.x}, {pos.y}, {pos.z}) with size ({size.x}, {size.y}, {size.z})");
                var obj = GameObject.Instantiate(Prefab, pos, Quaternion.identity);
                var Escape = obj.GetComponent<Escape>();
                Escape.OnTrigger = onTrigger;
                obj.GetComponent<BoxCollider>().size = size;

                return Escape;
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return null;
            }
        }

#pragma warning disable IDE0051 // Usuń nieużywane prywatne składowe
        private void OnTriggerEnter(Collider other)
#pragma warning restore IDE0051 // Usuń nieużywane prywatne składowe
        {
            if (!other.GetComponent<CharacterClassManager>())
                return;
            var player = Player.Get(other.gameObject);
            if (player?.IsNPC() ?? true || player.IsDead)
                return;
            OnTrigger(player);
        }
    }
}
