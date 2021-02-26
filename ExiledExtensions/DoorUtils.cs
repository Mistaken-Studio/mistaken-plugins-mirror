using Exiled.API.Enums;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.Utilities
{
    public static class DoorUtils
    {
        public static void Ini()
        {
            Prefabs.Clear();
            foreach (var spawnpoint in UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>())
            {
                switch (spawnpoint.TargetPrefab.name.ToUpper())
                {
                    case "EZ BREAKABLEDOOR":
                        Prefabs[DoorType.EZ_BREAKABLE] = spawnpoint.TargetPrefab;
                        break;
                    case "HCZ BREAKABLEDOOR":
                        Prefabs[DoorType.HCZ_BREAKABLE] = spawnpoint.TargetPrefab;
                        break;
                    case "LCZ BREAKABLEDOOR":
                        Prefabs[DoorType.LCZ_BREAKABLE] = spawnpoint.TargetPrefab;
                        break;
                }
            }
        }

        public static readonly Dictionary<DoorType, DoorVariant> Prefabs = new Dictionary<DoorType, DoorVariant>();

        public enum DoorType
        {
            EZ_BREAKABLE,
            HCZ_BREAKABLE,
            LCZ_BREAKABLE,
            AIRLOCK,
            CHECKPOINT,
        }
        public static DoorVariant GetPrefab(DoorType type)
        {
            switch (type)
            {
                case DoorType.EZ_BREAKABLE:
                case DoorType.HCZ_BREAKABLE:
                case DoorType.LCZ_BREAKABLE:
                    return Prefabs[type];
                default:
                    return null;
            }
        }

        public static DoorVariant SpawnDoor(DoorType type, string name, Vector3 position, Vector3 rotation) => 
            SpawnDoor(type, name, position, Quaternion.Euler(rotation));
        public static DoorVariant SpawnDoor(DoorType type, string name, Vector3 position, Quaternion rotation)
        {
            if(type == DoorType.AIRLOCK || type == DoorType.CHECKPOINT)
            {
                return null;
            }
            else
            {
                DoorVariant doorVariant = UnityEngine.Object.Instantiate(GetPrefab(type), position, rotation);
                if (doorVariant is BasicDoor door)
                    door._portalCode = 1;
                if (!string.IsNullOrEmpty(name))
                    doorVariant.gameObject.AddComponent<DoorNametagExtension>().UpdateName(name);
                NetworkServer.Spawn(doorVariant.gameObject);
                return doorVariant;
            }
        }
    }
}
