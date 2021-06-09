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

namespace Gamer.Mistaken.BetterSCP.SCP012
{
    internal class SCP012Handler : Module
    {
        internal readonly List<(ItemType, Vector3, Vector3, Vector3)> scpItems = new List<(ItemType, Vector3, Vector3, Vector3)>()
        {
            (ItemType.KeycardO5, new Vector3(3.68f, 6.34f, 4.66f), new Vector3(7.4f, 210f, 0.43f), Vector3.right * 90f),
            (ItemType.WeaponManagerTablet, new Vector3(), new Vector3(), new Vector3())
        };
        public SCP012Handler(IPlugin<IConfig> plugin) : base(plugin)
        {
        }
        public override string Name => nameof(SCP012Handler);
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => Server_RoundStarted(), "RoundStart");
        }
        private void Server_RoundStarted()
        {
            /*var room = Gamer.Utilities.MapPlus.Rooms.First(r => r.Type == RoomType.Lcz012);
            var instances = Pickup.Instances.Where(i => (Vector3.Distance(i.Networkposition, room.Position) <= 10));
            foreach (var instance in instances)
            {
                foreach (var d in RealPlayers.List.Where(x => x.IsActiveDev()))
                {
                    d.SendConsoleMessage($"{instance?.name}" ,"green");
                }
            }
            foreach (var item in scpItems)
            {
                var basePos = room.Position;
                var offset = room.transform.forward * -item.Item2.x + room.transform.right * -item.Item2.z + UnityEngine.Vector3.up * item.Item2.y;
                basePos += offset;
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Server.Host.Inventory.pickupPrefab);
                gameObject.transform.position = basePos;
                gameObject.transform.localScale = item.Item3;
                gameObject.transform.rotation = Quaternion.Euler(room.transform.eulerAngles + item.Item4);
                gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                gameObject.layer = 0;
                var keycard = gameObject.GetComponent<Pickup>();
                keycard.Locked = true;
                keycard.SetupPickup(item.Item1, 9991026f, Server.Host.Inventory.gameObject, new Pickup.WeaponModifiers(true, 0, 0, 0), gameObject.transform.position, gameObject.transform.rotation);
            }*/
        }
    }
}
