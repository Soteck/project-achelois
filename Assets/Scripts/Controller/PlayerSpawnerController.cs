﻿using System;
using Core;
using Enums;
using Map;
using Player;
using Unity.Netcode;
using UnityEngine;
using Util;
using NetworkPlayer = Network.NetworkPlayer;

namespace Controller {
    public class PlayerSpawnerController : NetworkSingleton<PlayerSpawnerController> {
        private GameObject _controllablePlayerPrefab;
        private GameObject _spectatorPrefab;
        
        private GameObject controllablePlayerPrefab {
            get {
                if (_controllablePlayerPrefab == null) {
                    _controllablePlayerPrefab = MapMaster.MapInstance().GetControllablePlayerPrefab();
                }

                return _controllablePlayerPrefab;
            }
            set => _controllablePlayerPrefab = value;
        }
        
        private GameObject spectatorPrefab {
            get {
                if (_spectatorPrefab == null) {
                    _spectatorPrefab = MapMaster.MapInstance().GetSpectatorPrefab();
                }

                return _spectatorPrefab;
            }
            set => _spectatorPrefab = value;
        }


        private void DoServerSpawnControllablePlayer(ulong playerId, NetworkPlayer player, int spawnPosition) {
            //GameObject go = NetworkObjectPool.Instance.GetNetworkObject(controlablePlayerPrefab).gameObject;
            Vector3 position = SpawnArea.GetSpawnPosition(player.selectedSpawnPoint.Value, spawnPosition);
            GameObject go = Instantiate(controllablePlayerPrefab, position, Quaternion.identity);
            //go.transform.position = new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10));
            //go.transform.position = go.transform.TransformDirection(position);

            NetworkObject no = go.GetComponent<NetworkObject>();
            no.transform.position = position;
            no.SpawnWithOwnership(playerId);
            //no.ChangeOwnership(playerId);
            player.ServerNotifyStateChange(PlayerState.PlayingAlive);
            PlayableSoldier po = go.GetComponent<PlayableSoldier>();
            po.InitClassRole();
            // DisableAllCameras(player.activeCamera);
        }
        
        // Public methods
        public static void ServerSpawnControllablePlayer(ulong playerId, NetworkPlayer player, int number) {
            Instance.DoServerSpawnControllablePlayer(playerId, player, number);
        }
    }
}