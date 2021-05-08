using Exiled.API.Features;
using NPCS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Components
{
    public class InRage : MonoBehaviour
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
                    prefab = new GameObject(nameof(InRage), typeof(InRage), typeof(BoxCollider))
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
            catch (System.Exception ex)
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

        public void Start()
        {
            Exiled.Events.Handlers.Player.Died += Player_Died;
            Exiled.Events.Handlers.Player.ChangingRole += Player_ChangingRole;
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (ev.ShouldPreservePosition)
                return;
            if (!ColliderInArea.Contains(ev.Player.GameObject))
                return;
            OnExit?.Invoke(ev.Player);
            ColliderInArea.Remove(ev.Player.GameObject);
        }

        private void OnDestroy()
        {
            Exiled.Events.Handlers.Player.Died -= Player_Died;
            Exiled.Events.Handlers.Player.ChangingRole -= Player_ChangingRole;
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (ColliderInArea.Contains(ev.Target.GameObject))
            {
                OnExit?.Invoke(ev.Target);
                ColliderInArea.Remove(ev.Target.GameObject);
            }
        }

        public readonly HashSet<GameObject> ColliderInArea = new HashSet<GameObject>();
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
            OnEnter?.Invoke(player);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!ColliderInArea.Contains(other.gameObject))
                return;
            ColliderInArea.Remove(other.gameObject);
            var player = Player.Get(other.gameObject);
            OnExit?.Invoke(player);
        }
    }
}
