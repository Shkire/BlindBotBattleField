using Client.BasicClasses;
using GameManagerActor.Interfaces.BasicClasses;
using GameManagerActor.Interfaces.EventHandlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.EventHandlers
{
    /// <summary>
    /// Implementation of EventHandler for IGameLobbyEvents
    /// </summary>
    class LobbyEventsHandler : IGameLobbyEvents
    {
        /// <summary>
        /// Allows EventHandler to interact with ClientGameManager
        /// </summary>
        private ClientGameManager p_gameManager;

        /// <summary>
        /// EventHandler constructor
        /// </summary>
        /// <param name="i_gameManager">ClientGameManager used by EventHandler</param>
        public LobbyEventsHandler(ClientGameManager i_gameManager)
        {
            p_gameManager = i_gameManager;
        }

        /// <summary>
        /// Refreshes lobby info
        /// </summary>
        /// <param name="i_playersList">Players in the lobby</param>
        public void GameLobbyInfoUpdate(List<string> i_playersList)
        {
            //Clears console and sets color to white
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            //Shows all players in lobby
            Console.WriteLine("Game Lobby\n");
            for (int i = 0; i < i_playersList.Count; i++)
            {
                if (i_playersList[i].Equals(p_gameManager.playerName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("->");
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\t" + (i + 1) + ". " + i_playersList[i]);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nEsc: Exit Lobby");
            //Notifies server that player stills connected
            p_gameManager.PlayerStillConnected();
        }

        /// <summary>
        /// Game starts
        /// </summary>
        public void GameStart(Dictionary<string,int[]> i_playerPositions)
        {
            p_gameManager.StartGame(i_playerPositions);
        }
    }

    /// <summary>
    /// Implementation of EventHandler for IGameEvents
    /// </summary>
    class GameEventsHandler : IGameEvents
    {
        /// <summary>
        /// Allows EventHandler to interact with ClientGameManager
        /// </summary>
        private ClientGameManager p_gameManager;

        /// <summary>
        /// EventHandler constructor
        /// </summary>
        /// <param name="i_gameManager">ClientGameManager used by EventHandler</param>
        public GameEventsHandler(ClientGameManager i_gameManager)
        {
            p_gameManager = i_gameManager;
        }

        /// <summary>
        /// Notifies that a player was killed by other
        /// </summary>
        /// <param name="i_player">Name of the killed player</param>
        /// <param name="i_killer">Name of the player that killed it</param>
        /// <param name="i_playerPos">Position vector of the killed player</param>
        /// <param name="i_reason">Reason for the player death</param>
        public void PlayerKilled(string i_player, string i_killer, int[] i_playerPos, DeathReason i_reason)
        {
            p_gameManager.PlayerDeadRecieved(i_playerPos, i_player, i_killer, i_reason);
        }

        /// <summary>
        /// Notifies that a player died
        /// </summary>
        /// <param name="i_player">Name of the dead player</param>
        /// <param name="i_playerPos">Position vector of the killed player</param>
        /// <param name="i_reason">Reason for the player death</param>
        public void PlayerDead(string i_player, int[] i_playerPos, DeathReason i_reason)
        {
            p_gameManager.PlayerDeadRecieved(i_playerPos, i_player, i_reason);
        }

        /// <summary>
        /// Notifies of a list of positions where a bomb hit
        /// </summary>
        /// <param name="i_hitList">List of bomb hit position vectors</param>
        public void BombHits(List<int[]> i_hitList)
        {
            p_gameManager.BombHitsRecieved(i_hitList);
        }

        /// <summary>
        /// Notifies that a player used radar
        /// </summary>
        /// <param name="o_playerPos">Player position vector</param>
        public void RadarUsed(int[] i_playerPos)
        {
            p_gameManager.PlayerDetected(i_playerPos);
        }

        /// <summary>
        /// Notifies that game session was finished
        /// </summary>
        /// <param name="o_winner">Winner player name</param>
        public void GameFinished(string i_winner)
        {
            p_gameManager.FinishGame(i_winner);
        }

        public void TurretAiming(int[] i_aimPos)
        {
            p_gameManager.TurretAiming(i_aimPos);
        }
    }
}
