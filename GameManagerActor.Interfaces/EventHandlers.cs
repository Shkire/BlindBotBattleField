using GameManagerActor.Interfaces.BasicClasses;
using Microsoft.ServiceFabric.Actors;
using System.Collections.Generic;

namespace GameManagerActor.Interfaces.EventHandlers
{
    /// <summary>
    /// Defines an interface for game lobby events. Allows actors send these events to the client
    /// </summary>
    public interface IGameLobbyEvents : IActorEvents
    {
        /// <summary>
        /// Event that sends a list with the players in the lobby to the client
        /// </summary>
        /// <param name="o_playersList">Players in the lobby</param>
        void GameLobbyInfoUpdate(List<string> o_playersList);

        /// <summary>
        /// Event that notifies the client that game is starting
        /// </summary>
        void GameStart(Dictionary<string,int[]> o_playerPositions);
    }

    /// <summary>
    /// Defines an interface for game events. Allows actors send these events to the client
    /// </summary>
    public interface IGameEvents : IActorEvents
    {
        void TurretAiming(int[] o_aimPos);

        /// <summary>
        /// Notifies the client that a player was killed by other
        /// </summary>
        /// <param name="o_player">Name of the killed player</param>
        /// <param name="o_killer">Name of the player that killed it</param>
        /// <param name="o_playerPos">Position vector of the killed player</param>
        /// <param name="o_reason">Reason for the player death</param>
        void PlayerKilled(string o_player, string o_killer, int[] o_playerPos, DeathReason o_reason);

        /// <summary>
        /// Notifies the client that a player died
        /// </summary>
        /// <param name="o_player">Name of the dead player</param>
        /// <param name="o_playerPos">Position vector of the killed player</param>
        /// <param name="o_reason">Reason for the player death</param>
        void PlayerDead(string o_player, int[] o_playerPos, DeathReason o_reason);

        /// <summary>
        /// Notifies the client of a list of positions where a bomb hit
        /// </summary>
        /// <param name="o_hitList">List of bomb hit position vectors</param>
        void BombHits(List<int[]> o_hitList);

        /// <summary>
        /// Notifies the client that a player used radar
        /// </summary>
        /// <param name="o_playerPos">Player position vector</param>
        void RadarUsed(int[] o_playerPos);

        /// <summary>
        /// Notifies the client that game session was finished
        /// </summary>
        /// <param name="o_winner">Winner player name</param>
        void GameFinished(string o_winner);
    }
}
