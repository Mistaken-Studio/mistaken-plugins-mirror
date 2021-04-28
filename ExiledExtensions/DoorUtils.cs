using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace Gamer.Utilities
{
    /// <summary>
    /// Door Utils
    /// </summary>
    public static class DoorUtils
    {
        /// <summary>
        /// Calls Ini
        /// </summary>
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

        private static readonly Dictionary<DoorType, DoorVariant> Prefabs = new Dictionary<DoorType, DoorVariant>();

        /// <summary>
        /// Door Type
        /// </summary>
        public enum DoorType
        {
#pragma warning disable CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
            EZ_BREAKABLE,
            HCZ_BREAKABLE,
            LCZ_BREAKABLE,
#pragma warning restore CS1591 // Brak komentarza XML dla widocznego publicznie typu lub składowej
        }
        /// <summary>
        /// Returns door prefab
        /// </summary>
        /// <param name="type">Door Type</param>
        /// <returns>Prefab</returns>
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
        /// <summary>
        /// Spawns Door
        /// </summary>
        /// <param name="type">Door Type</param>
        /// <param name="name">Door name or <see langword="null"/> if there should be no name</param>
        /// <param name="position">Door Position, if <see cref="Vector3.y"/> is smaller than 900 then door are automaticly locked to prevent crash</param>
        /// <param name="rotation">Door Rotation</param>
        /// <returns></returns>
        public static DoorVariant SpawnDoor(DoorType type, string name, Vector3 position, Vector3 rotation)
        {
            DoorVariant doorVariant = UnityEngine.Object.Instantiate(GetPrefab(type), position, Quaternion.Euler(rotation));
            if (doorVariant is BasicDoor door)
                door._portalCode = 1;
            if (!string.IsNullOrEmpty(name))
                doorVariant.gameObject.AddComponent<DoorNametagExtension>().UpdateName(name);
            if (position.y < 900)
                doorVariant.NetworkActiveLocks |= (ushort)DoorLockReason.AdminCommand;
            NetworkServer.Spawn(doorVariant.gameObject);
            return doorVariant;
        }
    }
}
