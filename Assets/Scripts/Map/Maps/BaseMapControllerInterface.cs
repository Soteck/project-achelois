using Enums;
using UnityEngine;
using NetworkPlayer = Network.NetworkPlayer;


namespace Map.Maps {
    public interface BaseMapControllerInterface {
        public int PlayersInGame();
        public float TimeElapsed();
        
        public float MapDuration();

        public void ServerRequestJoinTeam(Team team, ulong playerId);

        public NetworkPlayer GetPlayer(ulong playerId);

        public Camera MapCamera();

        public int TeamARespawn();
        public int TeamBRespawn();
    }
}