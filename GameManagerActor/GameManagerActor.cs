using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using GameManagerActor.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using LoginService.Interfaces;
using GameManagerActor.BasicClasses;
using GameManagerActor.Interfaces.EventHandlers;
using GameManagerActor.Interfaces.BasicClasses;
using ServerResponse;

namespace GameManagerActor
{
    /// <remarks>
    /// Esta clase representa a un actor.
    /// Cada elemento ActorID se asigna a una instancia de esta clase.
    /// El atributo StatePersistence determina la persistencia y la replicación del estado del actor:
    ///  - Persisted: el estado se escribe en el disco y se replica.
    ///  - Volatile: el estado se conserva solo en la memoria y se replica.
    ///  - None: el estado se conserva solo en la memoria y no se replica.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class GameManagerActor : Actor, IGameManagerActor, IRemindable
    {
        private const int PLAYER_ATTACK_RATE = 2;

        public GameManagerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// Initializes the GameSession object
        /// </summary>
        public async Task InitializeGameAsync()
        {
            //Creates GameSession
            GameSession gameSession = new GameSession();
            //Saves GameSession as "gamesession" state
            await this.StateManager.SetStateAsync("gamesession", gameSession);
        }

        /// <summary>
        /// Tries to connect player to GameSession
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <returns>True if player could be connected, false if game is full or started and false with exception if game was removed (or other reasons for exception throw)</returns>
        public async Task<ServerResponseInfo<bool, Exception>> ConnectPlayerAsync(string i_player)
        {
            ServerResponseInfo<bool, Exception> res = new ServerResponseInfo<bool, Exception>();
            try
            {
                //Get GameSession from StateManager
                GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");    
                //Adds player to game if posible
                if (gameSession.AddPlayer(i_player))
                {
                    //If first player in lobby
                    if (gameSession.playerCount == 1)
                        //Registers LobyCheck reminder
                        await this.RegisterReminderAsync("LobbyCheck", null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                    //Saves "gamesession" state
                    await this.StateManager.SetStateAsync("gamesession", gameSession);
                    ILoginService login = ServiceProxy.Create<ILoginService>(new Uri(ServiceUri.AbsoluteUri.Replace("GameManagerActorService", "LoginService")));
                    //Adds player to server (SQL Register)
                    await login.AddPlayerAsync(Id.ToString());
                    //Returns true
                    res.info = true;
                    return res;
                }
                //Saves "gamesession" state
                await this.StateManager.SetStateAsync("gamesession", gameSession);
                //Returns false (player couldn't be added: game full or started)
                res.info = false;
                return res;
            }
            //Catch exception if "gamesesion" state doesn't exist
            catch (Exception e)
            {
                //Returns false and exception (player couldn't be added: game removed)
                res.info = false;
                res.exception = e;
                return res;
            }
        }

        /// <summary>
        /// Send ActorEvent with lobby info to clients
        /// </summary>
        public async Task UpdateLobbyInfoAsync()
        {
            //Gets "gamesession" from StateManager
            GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
            //If game is in Lobby state
            if (gameSession.state.Equals(GameState.Lobby))
            {
                //Gets IGameLobbyEvents and sends GameLobbyInfoUpdate event with list of players
                var ev = GetEvent<IGameLobbyEvents>();
                ev.GameLobbyInfoUpdate(gameSession.playerList);

            }
        }

        /// <summary>
        /// Recieves notification from player's client that player stills connected
        /// </summary>
        /// <param name="i_player">Player name</param>
        public async Task PlayerStillConnectedAsync(string i_player)
        {
            //Gets "gamesession" from StateManager
            GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
            //Notifies that player stills connected
            gameSession.StillPlaying(i_player);
            //Saves "gamesession" state
            await this.StateManager.SetStateAsync("gamesession", gameSession);
        }

        //REMOVE GAME WHEN NO PLAYERS (REMINDER)???
        /// <summary>
        /// Disconnects player from GameSession
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <returns></returns>
        public async Task PlayerDisconnectAsync(string i_player)
        {
            //Gets "gamesession" from StateManagers
            GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
            //Removes player from game
            gameSession.RemovePlayer(i_player);
            //If game is in Lobby state and there's no players
            if (gameSession.state.Equals(GameState.Lobby) && gameSession.connectedPlayerCount == 0)
            {
                //Gets LobbyCheck reminder and unregisters it
                IActorReminder reminder = GetReminder("LobbyCheck");
                await UnregisterReminderAsync(reminder);
            }
            //Saves "gamesession" state
            await this.StateManager.SetStateAsync("gamesession", gameSession);
            ILoginService login = ServiceProxy.Create<ILoginService>(new Uri(ServiceUri.AbsoluteUri.Replace("GameManagerActorService","LoginService")));
            await login.RemovePlayerAsync(Id.ToString());
        }

        /// <summary>
        /// Gets player position
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <returns>Player position vector</returns>
        public async Task<ServerResponseInfo<int[]>> GetPlayerPosAsync(string i_player)
        {
            ServerResponseInfo<int[]> res = new ServerResponseInfo<int[]>();
            //Gets "gamesession" from StateManager
            GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
            //Returns player position
            res.info = gameSession.GetPlayerPos(i_player);
            return res;
        }

        /// <summary>
        /// Moves player
        /// </summary>
        /// <param name="i_dir">Movement vector</param>
        /// <param name="i_player">Player name</param>
        public async Task PlayerMovesAsync(int[] i_dir, string i_player)
        {
            //Out params
            int[] playerPos;
            string killedPlayer;
            //Gets "gamesession" from StateManager
            GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
            //Moves player
            MovementResult result = gameSession.MovePlayer(i_dir, i_player, out playerPos, out killedPlayer);
            //If something happened
            if (!result.Equals(MovementResult.Nothing))
            {
                //Gets IGameEvents
                var ev = GetEvent<IGameEvents>();
                //If player died
                if (result.Equals(MovementResult.PlayerDead))
                    //Send PlayerDead event and notifies client that player died, where and reason
                    ev.PlayerDead(i_player,playerPos,DeathReason.Hole);
                //If player killed other player
                else
                    //Send PlayerKilled event and notifies client which player died, which killed it, death position vector and death reason 
                    ev.PlayerKilled(killedPlayer, i_player, playerPos, DeathReason.PlayerSmash);
                //If there's only one player alive
                if (gameSession.AlivePlayers().Count == 1)
                    //Send GameFinished event and notifies client that game has finished and which player won
                    ev.GameFinished(gameSession.AlivePlayers()[0]);
            }
            //Saves "gamesession" state
            await this.StateManager.SetStateAsync("gamesession", gameSession);
        }

        /// <summary>
        /// Manages player attack
        /// </summary>
        /// <param name="i_player">Player name</param>
        public async Task PlayerAttacksAsync(string i_player)
        {
            //Out params
            List<int[]> i_hitList;
            Dictionary<string, int[]> i_killedPlayersDict;
            //Gets "gamesession" from StateManager
            GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
            //Manages layer attack
            gameSession.PlayerAttacks(i_player,PLAYER_ATTACK_RATE,out i_hitList,out i_killedPlayersDict);
            //Gets IGameEvents
            var ev = GetEvent<IGameEvents>();
            //Sends BombHits event to clients and notifies of hit area
            ev.BombHits(i_hitList);
            //If there are players killed by the attack
            if (i_killedPlayersDict.Count > 0)
                //For each player
                foreach (string killedPlayerId in i_killedPlayersDict.Keys)
                {
                    //Sends PlayerKilled event to clients and notifies which player was killed, player position vector and death reason
                    ev.PlayerKilled(killedPlayerId, i_player, i_killedPlayersDict[killedPlayerId],DeathReason.PlayerHit);
                }
            //Saves "gamesession" state
            await this.StateManager.SetStateAsync("gamesession", gameSession);
            //If there's only one player alive
            if (gameSession.AlivePlayers().Count == 1)
                //Sends GameFinished event to clients
                ev.GameFinished(gameSession.AlivePlayers()[0]);
        }

        /// <summary>
        /// Manages player's radar, returns info to player and notifies other players about it
        /// </summary>
        /// <param name="i_player">Player that used radar</param>
        /// <returns>Map info for this player</returns>
        public async Task<ServerResponseInfo<CellContent[][]>> RadarActivatedAsync(string i_player)
        {
            ServerResponseInfo<CellContent[][]> res = new ServerResponseInfo<CellContent[][]>();
            //Gets "gamesession" from StateManager
            GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
            //Gets IGameEvents and send RadarUsed to clients with player position vector
            var ev = GetEvent<IGameEvents>();
            ev.RadarUsed(gameSession.GetPlayerPos(i_player));
            //Returns map info to player
            res.info = gameSession.RadarActivated(i_player);
            return res;
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            //Gets "gamesession" from Statemanager
            GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
            //If LobbyCheck reminder
            if (reminderName.Equals("LobbyCheck"))
            {
                //Get players that are not connected
                List<string> removedPlayers = gameSession.CheckPlayers();
                //If there are players
                if (removedPlayers.Count > 0)
                    //Disconnect each player
                    foreach (string removingPlayer in removedPlayers)
                        await PlayerDisconnectAsync(removingPlayer);
            }
            //If game is in Lobby state and lobby is full
            if (gameSession.state.Equals(GameState.Lobby) && gameSession.isFull)
            {
                //Unregister LobbyCheck reminder
                await this.UnregisterReminderAsync(GetReminder("LobbyCheck"));
                //Prepare game
                gameSession.PrepareGame();
                //Saves "gamesession" state
                await this.StateManager.SetStateAsync("gameSession", gameSession);
                //Gets IGameLobbyEvents and send GameStart event to clients
                var ev = GetEvent<IGameLobbyEvents>();
                ev.GameStart();
            }
            //otherwise
            else
                //Update lobby info
                await UpdateLobbyInfoAsync();
        }
    }
}
