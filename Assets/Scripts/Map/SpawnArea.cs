using System;
using System.Collections.Generic;
using Enums;
using Player;
using Unity.Netcode;
using UnityEngine;
using Logger = Core.Logger;

namespace Map {
    [RequireComponent(typeof(NetworkObject))]
    public class SpawnArea : NetworkBehaviour {
        //Public vars
        public GameTeam gameTeam;
        public string spawnName;

        //Network-shared vars
        private readonly NetworkVariable<Guid> _spawnId = new NetworkVariable<Guid>();

        //Private vars
        private static List<SpawnArea> all_spawn_areas = new List<SpawnArea>();
        private bool registered = false;

        //Server only vars
        private List<Vector3> _spawnPoints;
        private const float SpawnSize = 1f;
        private const float SpawnFromFloor = 0.9f;

        void Start() {
            MapMaster.Instance.OnServerInitCallback += OnServerInit;
        }

        private void OnServerInit() {
            _spawnId.Value = Guid.NewGuid();
            _spawnPoints = new List<Vector3>();
            Vector3 componentSize = GetComponent<Renderer>().bounds.size;
            Vector3 position = transform.position;
            //Now we will split the X and Y of the size of the spawn as a grid and assign every slot as a spawn point
            long xN = (long) Math.Floor(componentSize.x / SpawnSize);
            long zN = (long) Math.Floor(componentSize.z / SpawnSize);
            for (int xIndex = 0; xIndex < xN; xIndex++) {
                for (int zIndex = 0; zIndex < zN; zIndex++) {
                    _spawnPoints.Add(new Vector3(
                        (position.x + (xIndex - 1) + (SpawnSize / 2)),
                        position.y + SpawnFromFloor,
                        (position.z + (zIndex - 1) + (SpawnSize / 2))
                    ));
                }
            }

            if (_spawnPoints.Count < 16) {
                Logger.Warning("Not enough spawn area for spawn " + spawnName + " Nº: " + _spawnPoints.Count);
            }


            all_spawn_areas.Add(this);
        }


        public static Guid GetDefaultTeamASpawnArea() {
            foreach (SpawnArea spawnArea in all_spawn_areas) {
                if (spawnArea.gameTeam == GameTeam.TeamA) {
                    return spawnArea._spawnId.Value;
                }
            }

            return Guid.Empty;
        }

        public static Guid GetDefaultTeamBSpawnArea() {
            foreach (SpawnArea spawnArea in all_spawn_areas) {
                if (spawnArea.gameTeam == GameTeam.TeamB) {
                    return spawnArea._spawnId.Value;
                }
            }

            return Guid.Empty;
        }

        public static Vector3 GetSpawnPosition(Guid guid, int position) {
            foreach (SpawnArea spawnArea in all_spawn_areas) {
                if (spawnArea._spawnId.Value.Equals(guid)) {
                    return spawnArea._spawnPoints[position];
                }
            }

            Logger.Warning("Spawn with id " + guid.ToString() + " not found!");
            return new Vector3();
        }
    }
}