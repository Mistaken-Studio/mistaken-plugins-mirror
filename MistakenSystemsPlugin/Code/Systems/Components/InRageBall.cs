using Exiled.API.Features;
using NPCS;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Components
{
    public class InRageBall : MonoBehaviour
    {
        public bool AllowNPCs = false;
        public Action<Player> OnEnter;
        public Action<Player> OnExit;
        private static GameObject prefab;
        private static GameObject Prefab
        {
            get
            {
                if (prefab == null)
                {
                    prefab = new GameObject(nameof(InRageBall), typeof(InRageBall), typeof(CapsuleCollider));
                    prefab.layer = Layer;
                    var collider = prefab.GetComponent<CapsuleCollider>();
                    collider.isTrigger = true;
                }
                return prefab;
            }
        }
        private static readonly int Layer = LayerMask.GetMask("TransparentFX"/*, "Ignore Raycast"*/);
        public static InRageBall Spawn(Vector3 pos, float radius, float height, Action<Player> onEnter = null, Action<Player> onExit = null)
        {
            try
            {
                var obj = GameObject.Instantiate(Prefab, pos, Quaternion.identity);
                var component = obj.GetComponent<InRageBall>();
                component.OnEnter = onEnter;
                component.OnExit = onExit;
                var collider = obj.GetComponent<CapsuleCollider>();
                collider.radius = radius;
                collider.height = height;

                return component;
            }
            catch(System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
                return null;
            }
        }
        public static InRageBall Spawn(Transform parrent, Vector3 offset, float radius, float height, Action<Player> onEnter = null, Action<Player> onExit = null)
        {
            try
            {
                var obj = GameObject.Instantiate(Prefab, parrent);
                obj.transform.localPosition = offset;
                obj.transform.rotation = Quaternion.identity;
                var component = obj.GetComponent<InRageBall>();
                component.OnEnter = onEnter;
                component.OnExit = onExit;
                var collider = obj.GetComponent<CapsuleCollider>();
                collider.radius = radius;
                collider.height = height;

                return component;
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
        }
        private void OnDestroy()
        {
            Exiled.Events.Handlers.Player.Died -= Player_Died;
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            this.OnExit?.Invoke(ev.Target);
            ColliderInArea.Remove(ev.Target.GameObject);
        }

        private readonly HashSet<GameObject> ColliderInArea = new HashSet<GameObject>();
        private void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<CharacterClassManager>())
                return;
            var player = Player.Get(other.gameObject);
            if (player?.IsDead ?? true)
                return;
            if (player.IsNPC() && !AllowNPCs)
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
