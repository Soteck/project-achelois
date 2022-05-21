using Enums;
using UnityEngine;
using NetworkPlayer = Network.NetworkPlayer;


namespace Map.Maps {
    public interface IBaseMapController {
        public int PlayersInGame();
        public float TimeElapsed();
        
        public float MapDuration();
        
        public float WarmupDuration();

        public void ServerRequestJoinTeam(Team team, ulong playerId);

        public NetworkPlayer GetPlayer(ulong playerId);

        public Camera MapCamera();

        public int TeamARespawn();
        public int TeamBRespawn();

        //
        /*
         * Positive value = Team A win
         * Negative value = Team B win
         * Value 0 = Tie
         */
        public int GetWinningTeam();

        public MapState GetMapState();
    }
}