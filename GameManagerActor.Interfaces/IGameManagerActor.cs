using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using GameManagerActor.Interfaces.EventHandlers;
using GameManagerActor.Interfaces.BasicClasses;
using ServerResponse;
using System;

namespace GameManagerActor.Interfaces
{
    /// <summary>
    /// GameManagerActor service method declaration. Allows comunication with actor service from client or other services
    /// </summary>
    public interface IGameManagerActor : IActor, IActorEventPublisher<IGameLobbyEvents>, IActorEventPublisher<IGameEvents>
    {
        /// <summary>
        /// Initializes the GameSession object
        /// </summary>
        Task InitializeGameAsync();

        /// <summary>
        /// Tries to connect player to GameSession
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <returns>True if player could be connected, false if game is full or started and false with exception if game was removed (or other reasons for exception throw)</returns>
        Task<ServerResponseInfo<bool,Exception>> ConnectPlayerAsync(string i_player);

        /// <summary>
        /// Send ActorEvent with lobby info to clients
        /// </summary>
        Task UpdateLobbyInfoAsync();

        /// <summary>
        /// Recieves notification from player's client that player stills connected
        /// </summary>
        /// <param name="i_player">Player name</param>
        Task PlayerStillConnectedAsync(string i_player);

        /// <summary>
        /// Disconnects player from GameSession
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <returns></returns>
        Task PlayerDisconnectAsync(string i_player);

        /// <summary>
        /// Gets player position
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <returns>Player position vector</returns>
        Task<ServerResponseInfo<int[]>> GetPlayerPosAsync(string i_player);

        /// <summary>
        /// Moves player
        /// </summary>
        /// <param name="i_dir">Movement vector</param>
        /// <param name="i_player">Player name</param>
        Task PlayerMovesAsync(int[] i_dir, string i_player);

        /// <summary>
        /// Manages player attack
        /// </summary>
        /// <param name="i_player">Player name</param>
        Task PlayerAttacksAsync(string i_player);

        /// <summary>
        /// Manages player's radar, returns info to player and notifies other players about it
        /// </summary>
        /// <param name="i_player">Player that used radar</param>
        /// <returns>Map info for this player</returns>
        Task<ServerResponseInfo<CellContent[][]>> RadarActivatedAsync(string i_player);
    }
}