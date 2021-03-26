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
    internal class Escape : MonoBehaviour
    {
        public Action<Player> OnTrigger;
        private static readonly GameObject Prefab = new GameObject("Escape", typeof(Escape), typeof(BoxCollider));
        public static Escape Spawn(Vector3 pos, Vector3 size, Action<Player> onTrigger)
        {
            var obj = GameObject.Instantiate(Prefab, pos, Quaternion.identity);
            obj.layer = LayerMask.GetMask("TransparentFX", "Ignore Raycast");
            var Escape = obj.GetComponent<Escape>();
            Escape.OnTrigger = onTrigger;
            var collider = obj.GetComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;

            return Escape;
        }

        private void OnTriggerEnter(Collider other)
        {
            Log.Debug("0.1");
            if (!other.GetComponent<CharacterClassManager>())
                return;
            Log.Debug("0.2");
            var player = Player.Get(other.gameObject);
            Log.Debug("0.3");
            if (player.IsNPC())
                return;
            Log.Debug("0.4");
            if (player.IsDead)
                return;
            Log.Debug("0.5");
            this.OnTrigger(player);
        }
    }
}
