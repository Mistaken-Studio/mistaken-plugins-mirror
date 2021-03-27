using Exiled.API.Features;
using NPCS;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Components
{
    public class InRage : MonoBehaviour
    {
        public Action<Player> OnEnter;
        public Action<Player> OnExit;
        private static GameObject prefab;
        private static GameObject Prefab
        {
            get
            {
                if (prefab == null)
                {
                    prefab = new GameObject(nameof(InRage), typeof(InRage), typeof(BoxCollider));
                    prefab.layer = Layer;
                    var collider = prefab.GetComponent<BoxCollider>();
                    collider.isTrigger = true;
                }
                return prefab;
            }
        }
        private static readonly int Layer = LayerMask.GetMask("TransparentFX", "Ignore Raycast");
        public static InRage Spawn(Vector3 pos, Vector3 size, Action<Player> onEnter = null, Action<Player> onExit = null)
        {
            try
            {
                var obj = GameObject.Instantiate(Prefab, pos, Quaternion.identity);
                var component = obj.GetComponent<InRage>();
                component.OnEnter = onEnter;
                component.OnExit = onExit;
                obj.GetComponent<BoxCollider>().size = size;

                return component;
            }
            catch(System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return null;
            }
        }
        public static InRage Spawn(Transform parrent, Vector3 offset, Vector3 size, Action<Player> onEnter = null, Action<Player> onExit = null)
        {
            try
            {
                var obj = GameObject.Instantiate(Prefab, parrent);
                obj.transform.localPosition = offset;
                obj.transform.rotation = Quaternion.identity;
                var component = obj.GetComponent<InRage>();
                component.OnEnter = onEnter;
                component.OnExit = onExit;
                obj.GetComponent<BoxCollider>().size = size;

                return component;
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return null;
            }
        }

        private readonly HashSet<GameObject> ColliderInArea = new HashSet<GameObject>();
        private void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<CharacterClassManager>())
                return;
            var player = Player.Get(other.gameObject);
            if (player.IsNPC())
                return;
            ColliderInArea.Add(other.gameObject);
            this.OnEnter?.Invoke(player);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!ColliderInArea.Contains(other.gameObject))
                return;
            ColliderInArea.Remove(other.gameObject);
            var player = Player.Get(other.gameObject);
            this.OnExit?.Invoke(player);
        }
    }
}
