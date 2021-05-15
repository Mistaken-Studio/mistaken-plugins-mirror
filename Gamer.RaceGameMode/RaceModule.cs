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
            foreach (var item in Map.Rooms)
            {
                item.Transform.gameObject.SetActive(false);
            }
        }

        private void GenerateMap()
        {
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 100; y++)
                {
                    SpawnDoor(new Vector3(-100 + x, 1000, -100 + y));
                }
            }
        }

        private void SpawnDoor(Vector3 pos)
        {
            DoorVariant doorVariant = UnityEngine.Object.Instantiate(doorPrefab, pos, Quaternion.identity);
            var go = doorVariant.gameObject;
            GameObject.Destroy(doorVariant.GetComponent<DoorEventOpenerExtension>());
            if (doorVariant.TryGetComponent<Scp079Interactable>(out var scp079Interactable))
                GameObject.Destroy(scp079Interactable);
            GameObject.Destroy(doorVariant);
            NetworkServer.Spawn(go);
        }
    }
}
