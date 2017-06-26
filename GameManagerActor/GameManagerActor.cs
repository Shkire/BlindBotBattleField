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
        /// <param name="i_mapIndex">Maximum number of players</param>
        /// <param name="i_maxPlayers">Chosen map index</param>
        public async Task InitializeGameAsync(int i_maxPlayers)
        {
            //Creates GameSession
            GameSession gameSession = new GameSession(i_maxPlayers);
            //Saves GameSession as "gamesession" state
            await this.StateManager.SetStateAsync("gamesession", gameSession);
            await this.RegisterReminderAsync("RemoveIfEmpty", null, TimeSpan.FromSeconds(60), TimeSpan.FromMilliseconds(-1));
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
                    {
                        await this.UnregisterReminderAsync(GetReminder("RemoveIfEmpty"));
                        //Registers LobyCheck reminder
                        await this.RegisterReminderAsync("LobbyCheck", null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                    }
                    //Saves "gamesession" state
                    await this.StateManager.SetStateAsync("gamesession", gameSession);
                    ActorEventSource.Current.Message(ServiceUri.AbsoluteUri);
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
                ActorEventSource.Current.Message("Lanza evento");
                foreach (string player in gameSession.playerList)
                {
                    ActorEventSource.Current.Message("Jugador: "+player);
                }
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
            /*
            if (gameSession.playerCount == 0)
                await this.RegisterReminderAsync("RemoveIfEmpty", null, TimeSpan.FromSeconds(60), TimeSpan.FromMilliseconds(-1));
                */
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
        public async Task<ServerResponseInfo<bool,Exception>> PlayerMovesAsync(int[] i_dir, string i_player)
        {
            ServerResponseInfo<bool, Exception> response = new ServerResponseInfo<bool, Exception>();
            response.info = true;
            try
            {
                //Gets "gamesession" from StateManager
                GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
                //Moves player
                MovementResult result = gameSession.MovePlayer(i_dir, i_player);
                //If player wasn't connected
                if (result.type.Equals(MovementResultType.PlayerNotConnected))
                {
                    response.info = false;
                }
                //If something happened
                else if (!result.type.Equals(MovementResultType.Nothing))
                {
                    //Gets IGameEvents
                    var ev = GetEvent<IGameEvents>();
                    //If player died
                    if (result.type.Equals(MovementResultType.PlayerDied))
                        //Send PlayerDead event and notifies client that player died, where and reason
                        ev.PlayerDead(i_player, result.playerPos, DeathReason.Hole);
                    //If player killed other player
                    else
                        //Send PlayerKilled event and notifies client which player died, which killed it, death position vector and death reason 
                        ev.PlayerKilled(result.killedPlayer, i_player, result.playerPos, DeathReason.PlayerSmash);
                    //If there's only one player alive
                    if (gameSession.AlivePlayers().Count == 1)
                        //Send GameFinished event and notifies client that game has finished and which player won
                        ev.GameFinished(gameSession.AlivePlayers()[0]);
                }
                //Saves "gamesession" state
                await this.StateManager.SetStateAsync("gamesession", gameSession);
            }
            catch(Exception e)
            {
                response.info = false;
                response.exception = e;
            }
            return response;
        }

        /// <summary>
        /// Manages player attack
        /// </summary>
        /// <param name="i_player">Player name</param>
        public async Task<ServerResponseInfo<bool,Exception>> PlayerAttacksAsync(string i_player)
        {
            ServerResponseInfo<bool, Exception> response = new ServerResponseInfo<bool, Exception>();
            response.info = true;
            try
            {
                //Gets "gamesession" from StateManager
                GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
                //Manages player attack
                AttackResult result = gameSession.PlayerAttacks(i_player, PLAYER_ATTACK_RATE);
                //If player wasn't connected
                if (!result.success)
                {
                    response.info = false;
                }
                //Otherwise
                else
                {
                    //Gets IGameEvents
                    var ev = GetEvent<IGameEvents>();
                    //Sends BombHits event to clients and notifies of hit area
                    ev.BombHits(result.hitPoints);
                    //If there are players killed by the attack
                    if (result.killedPlayersDict.Count > 0)
                        //For each player
                        foreach (string killedPlayerId in result.killedPlayersDict.Keys)
                        {
                            //Sends PlayerKilled event to clients and notifies which player was killed, player position vector and death reason
                            ev.PlayerKilled(killedPlayerId, i_player, result.killedPlayersDict[killedPlayerId], DeathReason.PlayerHit);
                        }
                    //Saves "gamesession" state
                    await this.StateManager.SetStateAsync("gamesession", gameSession);
                    //If there's only one player alive
                    if (gameSession.AlivePlayers().Count == 1)
                        //Sends GameFinished event to clients
                        ev.GameFinished(gameSession.AlivePlayers()[0]);
                }
            }
            catch (Exception e)
            {
                response.info = false;
                response.exception = e;
            }
            return response;
        }

        /// <summary>
        /// Manages player's radar, returns info to player and notifies other players about it
        /// </summary>
        /// <param name="i_player">Player that used radar</param>
        /// <returns>Map info for this player</returns>
        public async Task<ServerResponseInfo<bool,Exception,CellContent[][]>> RadarActivatedAsync(string i_player)
        {
            ServerResponseInfo<bool, Exception, CellContent[][]> response = new ServerResponseInfo<bool, Exception, CellContent[][]>();
            response.info = true;
            try
            {
                //Gets "gamesession" from StateManager
                GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
                //Gets radar result
                RadarResult result = gameSession.RadarActivated(i_player);
                //If player wasn't connected
                if (!result.success)
                {
                    response.info = false;
                }
                //Otherwise
                else
                {
                    //If player wasn't dead
                    if (result.mapInfo != null)
                    {
                        //Gets IGameEvents and send RadarUsed to clients with player position vector
                        var ev = GetEvent<IGameEvents>();
                        ev.RadarUsed(gameSession.GetPlayerPos(i_player));
                        //Adds map info to response
                        response.additionalInfo = result.mapInfo;
                    }
                }
            }
            catch (Exception e)
            {
                response.info = false;
                response.exception = e;
            }
            return response;
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
                //If game is in Lobby state and lobby is full
                if (gameSession.state.Equals(GameState.Lobby) && gameSession.isFull)
                {
                    //Unregister LobbyCheck reminder
                    await this.UnregisterReminderAsync(GetReminder("LobbyCheck"));
                    //Prepare game
                    gameSession.PrepareGame();
                    //Gets IGameLobbyEvents and send GameStart event to clients
                    var ev = GetEvent<IGameLobbyEvents>();
                    ev.GameStart(gameSession.getPlayerPositions);
                    await this.RegisterReminderAsync("TurretAim", null , TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(-1));
                }
                //otherwise
                else
                    //Update lobby info
                    await UpdateLobbyInfoAsync();
            }
            if (reminderName.Equals("RemoveIfEmpty"))
            {
                ILoginService login = ServiceProxy.Create<ILoginService>(new Uri(ServiceUri.AbsoluteUri.Replace("GameManagerActorService", "LoginService")));
                await login.DeleteGameAsync(Id.ToString(),ServiceUri.AbsoluteUri);
            }
            if (reminderName.Equals("TurretAim"))
            {
                if (context == null)
                {
                    int[] aimPos = gameSession.GetTurretAimPos(PLAYER_ATTACK_RATE);
                    var ev = GetEvent<IGameEvents>();
                    ev.TurretAiming(aimPos);
                    await this.RegisterReminderAsync("TurretAim", new byte[] { 1, (byte)aimPos[0], (byte)aimPos[1] }, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
                }
                else
                {
                    int counter = context[0];
                    int[] aimPos = new int[] { context[1], context[2] };
                    counter++;
                    var ev = GetEvent<IGameEvents>();
                    ev.TurretAiming(aimPos);
                    if (counter < 5)
                        await this.RegisterReminderAsync("TurretAim", new byte[] { (byte)counter, (byte)aimPos[0], (byte)aimPos[1] }, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
                    else
                        await this.RegisterReminderAsync("TurretShot", new byte[] {(byte)aimPos[0], (byte)aimPos[1] }, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
                }
            }
            if (reminderName.Equals("TurretShot"))
            {
                int[] aimpos = new int[] { context[0], context[1] };
                AttackResult result = gameSession.TurretAttacks(aimpos, PLAYER_ATTACK_RATE);
                //Gets IGameEvents
                var ev = GetEvent<IGameEvents>();
                //Sends BombHits event to clients and notifies of hit area
                ev.BombHits(result.hitPoints);
                //If there are players killed by the attack
                if (result.killedPlayersDict.Count > 0)
                    //For each player
                    foreach (string killedPlayerId in result.killedPlayersDict.Keys)
                    {
                        //Sends PlayerKilled event to clients and notifies which player was killed, player position vector and death reason
                        ev.PlayerDead(killedPlayerId, result.killedPlayersDict[killedPlayerId], DeathReason.Turret);
                    }
                //If there's only one player alive
                if (gameSession.AlivePlayers().Count == 1)
                    //Sends GameFinished event to clients
                    ev.GameFinished(gameSession.AlivePlayers()[0]);
                await this.RegisterReminderAsync("TurretAim", null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(-1));
            }
            //Saves "gamesession" state
            await this.StateManager.SetStateAsync("gameSession", gameSession);
        }
    }
}
