using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using GameManagerActor.Interfaces.EventHandlers;
using GameManagerActor.Interfaces.BasicClasses;

namespace GameManagerActor.Interfaces
{
    /// <summary>
    /// GameManagerActor service method declaration. Allows comunication with actor service from client or other services
    /// </summary>
    public interface IGameManagerActor : IActor, IActorEventPublisher<IGameLobbyEvents>, IActorEventPublisher<IGameEvents>
    {
        /// <summary>
        /// Initializes the GameMap object
        /// </summary>
        /// <returns></returns>
        Task InitializeGameAsync();

        /// <summary>
        /// Tries to connect player to game session
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <returns>0 if player could be connected, 1 if game is full or started and 2 if game was removed</returns>
        Task<int> ConnectPlayerAsync(string i_player);

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
        /// <returns></returns>
        Task PlayerAttacksAsync(string i_player);

        /// <summary>
        /// Disconnects player from game session
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <returns></returns>
        Task PlayerDisconnectAsync(string i_player);

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
        /// Gets player position
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <returns>Player position vector</returns>
        Task<int[]> GetPlayerPosAsync(string i_player);

        /// <summary>
        /// Manages player's radar, returns info to player and notifies other players about it
        /// </summary>
        /// <param name="i_player">Player that used radar</param>
        /// <returns>Map info for this player</returns>
        Task<CellContent[][]> RadarActivatedAsync(string i_player);
    }
}