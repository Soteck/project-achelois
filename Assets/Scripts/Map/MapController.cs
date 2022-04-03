using Core;
using Unity.Netcode;

public class MapController : NetworkSingleton<MapController> {
    
    NetworkVariable<int> playersInGame = new NetworkVariable<int>();

    public int PlayersInGame {
        get { return playersInGame.Value; }
    }

    void Start() {
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += (id) => { playersInGame.Value++; };

            NetworkManager.Singleton.OnClientDisconnectCallback += (id) => { playersInGame.Value--; };
        }
    }
}