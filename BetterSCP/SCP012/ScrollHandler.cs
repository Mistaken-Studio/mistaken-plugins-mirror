using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using Exiled.API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.PerformanceData;
using UnityEngine;
using Gamer.Utilities;
using Gamer.Mistaken.Base.Staff;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.API.Extensions;

namespace Gamer.Mistaken.BetterSCP.SCP012
{
    internal class SCP012Handler : Module
    {
        internal readonly List<(ItemType, Vector3, Vector3, Vector3)> scpItems = new List<(ItemType, Vector3, Vector3, Vector3)>()
        {
            (ItemType.KeycardO5, new Vector3(-3.68f, -6.34f, -4.66f), new Vector3(7.4f, 210f, 0.43f), Vector3.right * 90f),
            (ItemType.WeaponManagerTablet, new Vector3(-3.68f, -6.31f, -4.3f), new Vector3(1f, 2f, 1f), new Vector3(0f, 180f, 70f))
        };
        internal readonly List<Vector3> itemOffset = new List<Vector3>()
        {
            new Vector3(-8.5f, -7.8f, -7.5f),
            new Vector3(1.6f, -7.8f, -7.5f)
        };
        public SCP012Handler(IPlugin<IConfig> plugin) : base(plugin)
        {
        }
        public override string Name => nameof(SCP012Handler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Interacting_Door(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Interacting_Door(ev));
        }
        private void Interacting_Door(InteractingDoorEventArgs ev)
        {
            if (ev.Door.Type() == DoorType.Scp012 && ev.IsAllowed)
            {
                this.CallDelayed(0.5f, () => 
                {
                    foreach (var item in Pickup.Instances.Where(i => (Vector3.Distance(i.Networkposition, room.Position) <= 10) && (i.NetworkitemId == ItemType.KeycardZoneManager || i.NetworkitemId == ItemType.GunCOM15)))
                    {
                        int index = UnityEngine.Random.Range(0, itemOffset.Count);
                        var basePos = room.Position;
                        var offset = room.transform.forward * -itemOffset[index].x + room.transform.right * -itemOffset[index].z + Vector3.up * itemOffset[index].y;
                        basePos += offset;
                        item.Networkposition = basePos;
                    }
                }, "Scp012ChangingItemPosition");
            }
        }
        internal Room room;
        private void Server_RoundStarted()
        {
            room = Gamer.Utilities.MapPlus.Rooms.First(r => r.Type == RoomType.Lcz012);
            foreach (var item in scpItems)
            {
                var basePos = room.Position;
                var offset = room.transform.forward * -item.Item2.x + room.transform.right * -item.Item2.z + Vector3.up * item.Item2.y;
                basePos += offset;
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
                gameObject.transform.position = basePos;
                gameObject.transform.localScale = item.Item3;
                gameObject.transform.rotation = Quaternion.Euler(room.transform.eulerAngles + item.Item4);
                Mirror.NetworkServer.Spawn(gameObject);
                gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                var keycard = gameObject.GetComponent<Pickup>();
                keycard.Locked = true;
                keycard.SetupPickup(item.Item1, 9991026f, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            }
        }
    }
}
