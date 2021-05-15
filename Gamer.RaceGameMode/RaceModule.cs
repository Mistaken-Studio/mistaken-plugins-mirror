using Exiled.API.Features;
using Exiled.API.Interfaces;
using Gamer.Diagnostics;
using Interactables.Interobjects.DoorUtils;
using MapGeneration;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Gamer.RaceGameMode
{
    class RaceModule : Module
    {
        public RaceModule(IPlugin<IConfig> plugin) : base(plugin)
        {
        }

        public override string Name => "Race";

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
        }

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Handle(() => Server_WaitingForPlayers(), "WaitingForPlayers");
        }

        private DoorVariant doorPrefab;
        private void Server_WaitingForPlayers()
        {
            foreach (var spawnpoint in UnityEngine.Object.FindObjectsOfType<DoorSpawnpoint>())
            {
                switch (spawnpoint.TargetPrefab.name.ToUpper())
                {
                    case "HCZ BREAKABLEDOOR":
                        doorPrefab = spawnpoint.TargetPrefab;
                        break;
                }
            }
            /*foreach (var item in Map.Rooms)
            {
                item.Transform.gameObject.SetActive(false);
            }*/

            GenerateMap();
        }

        private void GenerateMap()
        {
            int max_x = 25;
            int max_z = 25;
            for (int x = 0; x < max_x; x++)
            {
                for (int z = 0; z < max_z; z++)
                {
                    SpawnDoor(new Vector3(1000 + (-max_x + x) * 2, 1000, (-max_z + z) * 3.2f));
                }
            }
        }

        private void SpawnDoor(Vector3 pos)
        {
            DoorVariant doorVariant = UnityEngine.Object.Instantiate(doorPrefab, pos, Quaternion.Euler(90, 0, 0));
            var go = doorVariant.gameObject;
            GameObject.Destroy(doorVariant.GetComponent<DoorEventOpenerExtension>());
            if (doorVariant.TryGetComponent<Scp079Interactable>(out var scp079Interactable))
                GameObject.Destroy(scp079Interactable);
            GameObject.Destroy(doorVariant);
            NetworkServer.Spawn(go);
        }
    }
}
