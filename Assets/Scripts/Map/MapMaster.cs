using System;
using Core;
using Map.Maps;
using Unity.Netcode;
using UnityEngine;

namespace Map {
    public class MapMaster : NetworkSingleton<MapMaster> {
        [SerializeField]
        private bool serverInit = false;

        public bool IsServerInit() {
            return serverInit;
        }
        
        private readonly NetworkVariable<int> _networkEnergyLoadRatio = new NetworkVariable<int>();

        public IBaseMapController instance { get; set; }

        public static IBaseMapController MapInstance() {
            return Instance.instance;
        }

        public int NetworkEnergyLoadRatio() {
            return _networkEnergyLoadRatio.Value;
        }

        private void ServerInit() {
            _networkEnergyLoadRatio.Value = 7;
        }

        public event Action OnServerInitCallback = null;
        internal void InvokeOnServerInitCallback() => OnServerInitCallback?.Invoke();

        protected void Start() {
            NetworkManager.Singleton.OnServerStarted += () => {
                ServerInit();
                InvokeOnServerInitCallback();
                serverInit = true;
            };
        }
    }
}