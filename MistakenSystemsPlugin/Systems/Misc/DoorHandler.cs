using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Gamer.Diagnostics;
using Gamer.Utilities;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamer.Mistaken.Systems.Misc
{
    public class DoorHandler : Module
    {
        public DoorHandler(PluginHandler p) : base(p)
        {
        }


        public override string Name => "Door";
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.Shooting += this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor += this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
        }
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Player.Shooting -= this.Handle<Exiled.Events.EventArgs.ShootingEventArgs>((ev) => Player_Shooting(ev));
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Handle<Exiled.Events.EventArgs.InteractingDoorEventArgs>((ev) => Player_InteractingDoor(ev));
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;
            var type = ev.Door.Type();
            if (type == DoorType.EscapePrimary)
            {
                Map.Doors.First(d => d.Type() == DoorType.EscapeSecondary).NetworkTargetState = ev.Door.NetworkTargetState;
            }
            else if (type == DoorType.EscapeSecondary)
            {
                Map.Doors.First(d => d.Type() == DoorType.EscapePrimary).NetworkTargetState = ev.Door.NetworkTargetState;
            }
        }

        public static readonly Dictionary<GameObject, BreakableDoor> Doors = new Dictionary<GameObject, BreakableDoor>();
        private void Server_WaitingForPlayers()
        {
            Log.Debug("[DOOR] Starting");
            DoorUtils.Ini();
            Log.Debug("[DOOR] Ini Done");
            if (Map.Doors == null)
            {
                Log.Warn("[DOOR] Doors not found");
                return;
            }
            if (!Map.Doors.Any(d => d.Type() == DoorType.HID))
                Log.Warn("[DOOR] HID door not found");
            else
                (Map.Doors.First(d => d.Type() == DoorType.HID) as BreakableDoor)._ignoredDamageSources |= DoorDamageType.Grenade;
            Log.Debug("[DOOR] HID Done");
            HashSet<DoorVariant> toIgnore = new HashSet<DoorVariant>();
            if (!Map.Doors.Any(d => d.Type() == DoorType.CheckpointEntrance))
                Log.Warn("[DOOR] CheckpointEZ door not found");
            else
            {
                var checkpointEZ = Map.Doors.First(d => d.Type() == DoorType.CheckpointEntrance) as CheckpointDoor;
                foreach (var door in checkpointEZ._subDoors)
                {
                    toIgnore.Add(door);
                    if (!(door is BreakableDoor damageableDoor))
                        continue;
                    damageableDoor._maxHealth = 2000;
                    damageableDoor._remainingHealth = damageableDoor._maxHealth;
                    damageableDoor._ignoredDamageSources = DoorDamageType.Weapon | DoorDamageType.Grenade;
                }
            }
            Log.Debug("[DOOR] CheckpointEZ Done");
            if (!Map.Doors.Any(d => d.Type() == DoorType.CheckpointLczA))
                Log.Warn("[DOOR] CheckpointA door not found");
            else
            {
                var checkpointLCZ_A = Map.Doors.First(d => d.Type() == DoorType.CheckpointLczA) as CheckpointDoor;
                foreach (var door in checkpointLCZ_A._subDoors)
                {
                    toIgnore.Add(door);
                    if (!(door is BreakableDoor damageableDoor))
                        continue;
                    damageableDoor._maxHealth = 1000;
                    damageableDoor._remainingHealth = damageableDoor._maxHealth;
                    damageableDoor._ignoredDamageSources = DoorDamageType.Weapon | DoorDamageType.Grenade;
                }
            }
            Log.Debug("[DOOR] CheckpointA Done");
            if (!Map.Doors.Any(d => d.Type() == DoorType.CheckpointLczB))
                Log.Warn("[DOOR] CheckpointB door not found");
            else
            {
                var checkpointLCZ_B = Map.Doors.First(d => d.Type() == DoorType.CheckpointLczB) as CheckpointDoor;

                foreach (var door in checkpointLCZ_B._subDoors)
                {
                    toIgnore.Add(door);
                    if (!(door is BreakableDoor damageableDoor))
                        continue;
                    damageableDoor._maxHealth = 1000;
                    damageableDoor._remainingHealth = damageableDoor._maxHealth;
                    damageableDoor._ignoredDamageSources = DoorDamageType.Weapon | DoorDamageType.Grenade;
                }
            }
            Log.Debug("[DOOR] CheckpointB Done");


            foreach (var door in Map.Doors.Where(d => d.Type() == DoorType.Scp106Primary || d.Type() == DoorType.Scp106Secondary || d.Type() == DoorType.Scp106Bottom).Select(d => (d as CheckpointDoor)._subDoors[0]))
            {
                toIgnore.Add(door);
                if (door is BreakableDoor d)
                    d._ignoredDamageSources |= DoorDamageType.Grenade;
                else
                    Log.Debug(door.Type() + " is not Breakable");
            }
            Log.Debug("[DOOR] 106 Done");
            try
            {
                Log.Debug("[DOOR] Starting doors");
                foreach (var door in Map.Doors)
                {
                    if (!(door is BreakableDoor damageableDoor))
                        continue;
                    if (toIgnore.Contains(door))
                        continue;

                    //Log.Debug("Checking " + door.name);
                    switch (door.name.Split('(')[0].Trim())
                    {
                        case "HCZ BreakableDoor":
                            damageableDoor._maxHealth = 150;
                            break;
                        case "EZ BreakableDoor":
                        case "LCZ PortallessBreakableDoor":
                        case "LCZ BreakableDoor":
                            damageableDoor._maxHealth = 75;
                            break;
                        case "Prison BreakableDoor":
                            damageableDoor._maxHealth = 50;
                            break;
                        default:
                            Log.Debug("[DOOR] Skipped " + door.name);
                            continue;
                    }
                    //Log.Debug("Updating " + door.name);
                    damageableDoor._remainingHealth = damageableDoor._maxHealth;
                    damageableDoor._ignoredDamageSources = DoorDamageType.None;
                }
                Log.Debug("[DOOR] Doors done");

                Log.Debug("[DOOR] Registering Doors");
                foreach (var item in Map.Doors.Where(d => d is BreakableDoor))
                {
                    var door = item as BreakableDoor;
                    if (door == null)
                        continue;
                    if ((door._ignoredDamageSources & DoorDamageType.Weapon) == (DoorDamageType)0)
                        RegisterDoors(door, item.transform);
                }
                Log.Debug("[DOOR] Registered Doors");
            }
            catch (System.Exception ex)
            {
                Log.Error("[DOOR] Failed to set door health");
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
        }

        private void RegisterDoors(BreakableDoor door, Transform transform)
        {
            Doors[transform.gameObject] = door;
            for (int i = 0; i < transform.childCount; i++)
                RegisterDoors(door, transform.GetChild(i));
        }

        private void Player_Shooting(Exiled.Events.EventArgs.ShootingEventArgs ev)
        {
            if (ev.Target == null)
            {
                var citem = Base.CustomItems.CustomItemsHandler.GetCustomItem(ev.Shooter.CurrentItem);
                if (citem != null)
                    return;
                var colliders = UnityEngine.Physics.OverlapSphere(ev.Position, 0.1f);
                if (colliders == null)
                    return;
                foreach (var item in colliders)
                {
                    if (!Doors.TryGetValue(item.gameObject, out var door) || door == null)
                        continue;
                    var wm = ev.Shooter.ReferenceHub.weaponManager;
                    float time = Vector3.Distance(ev.Shooter.CameraTransform.position, ev.Position);
                    float damage = wm.weapons[(int)wm.curWeapon].damageOverDistance.Evaluate(time) * 0.1f;
                    door.ServerDamage(damage, DoorDamageType.Weapon);
                    wm.RpcConfirmShot(true, wm.curWeapon);
                }
            }
        }
    }
}
