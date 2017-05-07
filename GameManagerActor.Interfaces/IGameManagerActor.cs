using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace GameManagerActor.Interfaces
{
    public class MapInfo
    {
        public enum CellContent
        {
            None,
            Floor,
            Hole,
            Player
        }

        private CellContent p_content;

        private string p_playerId;

        public CellContent content
        {
            get
            {
                return p_content;
            }
        }

        public string playerId
        {
            get
            {
                return p_playerId;
            }
        }

        public MapInfo(string i_playerId)
        {
            p_content = CellContent.Player;
            p_playerId = i_playerId;
        }

        public MapInfo()
        {
            p_content = CellContent.Hole;
        }
    }

    public interface IGameLobbyEvents : IActorEvents
    {
        //!!!!!Define Event params
        void GameLobbyInfoUpdate(List<string> o_playerIdMap);

        void GameStart();
    }

    public interface IGameEvents : IActorEvents
    { 
        void PlayerKilled(string o_playerKilledId, string o_playerKillerId, int[] o_playerPos);

        void PlayerDead(string o_playerId, int o_reason, int[] o_playerPos);

        void BombHits(List<int[]> o_hitList);
    }

    /// <summary>
    /// GameManager Actor Interface.
    /// Contains all actor methods and other actors or client can call it using this Interface.
    /// </summary>
    public interface IGameManagerActor : IActor, IActorEventPublisher<IGameLobbyEvents>, IActorEventPublisher<IGameEvents>
    {
        Task<bool> PlayerRegisterAsync(string i_playerId);

        Task PlayerMovesAsync(int[] i_dir, string i_playerId);

        Task PlayerAttacksAsync(string i_playerId);

        Task PlayerDisconnectAsync(string i_playerId);

        Task UpdateLobbyInfoAsync();

        Task StartGameAsync();

        Task PlayerStillConnectedAsync(string i_playerId);
    }
}