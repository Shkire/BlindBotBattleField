using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using GameManagerActor.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using LoginService.Interfaces;
using GameManagerActor.BasicClasses;
using GameManagerActor.Interfaces.BasicClasses;
using ExtensionMethods;
using GameManagerActor.Sockets;
using BasicClasses.Common;

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
        public async Task<ServerResponseInfo<bool, Exception>> ConnectPlayerAsync(string i_player, byte[] i_address)
        {
            ServerResponseInfo<bool, Exception> res = new ServerResponseInfo<bool, Exception>();
            try
            {
                //Get GameSession from StateManager
                GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");    
                //Adds player to game if posible
                if (gameSession.AddPlayer(i_player,i_address))
                {
                    //If first player in lobby
                    if (gameSession.playerCount == 1)
                    {
                        await this.UnregisterReminderAsync(GetReminder("RemoveIfEmpty"));
                        //Registers LobyCheck reminder
                        await this.RegisterReminderAsync("LobbyCheck", null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
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
                List<string> message = new List<string>();
                message.Add("GameLobbyInfoUpdate");
                message.Add(gameSession.playerList.SerializeObject());
                foreach (string player in gameSession.playerList)
                {
                    SocketClient socket = new SocketClient();
                    socket.StartLobbyClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                }
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
                    //If player died
                    if (result.type.Equals(MovementResultType.PlayerDied))
                    {  
                        List<string> message = new List<string>();
                        message.Add("PlayerDead");
                        message.Add(i_player.SerializeObject());
                        message.Add(gameSession.playerList.SerializeObject());
                        message.Add(DeathReason.Hole.SerializeObject());
                        foreach (string player in gameSession.playerList)
                        {
                            SocketClient socket = new SocketClient();
                            socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                        }
                    }
                    //If player killed other player
                    else
                    {
                        List<string> message = new List<string>();
                        message.Add("PlayerKilled");
                        message.Add(result.killedPlayer.SerializeObject());
                        message.Add(i_player.SerializeObject());
                        message.Add(result.playerPos.SerializeObject());
                        message.Add(DeathReason.PlayerSmash.SerializeObject());
                        foreach (string player in gameSession.playerList)
                        {
                            SocketClient socket = new SocketClient();
                            socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                        }
                    }
                    //If there's only one player alive
                    if (gameSession.AlivePlayers().Count == 1)
                    {
                        List<string> message = new List<string>();
                        message.Add("GameFinished");
                        message.Add(gameSession.AlivePlayers()[0].SerializeObject());
                        foreach (string player in gameSession.playerList)
                        {
                            SocketClient socket = new SocketClient();
                            socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                        }
                    }
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
                    List<string> message = new List<string>();
                    message.Add("BombHits");
                    message.Add(result.hitPoints.SerializeObject());
                    foreach (string player in gameSession.playerList)
                    {
                        SocketClient socket = new SocketClient();
                        socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                    }
                    //If there are players killed by the attack
                    if (result.killedPlayersDict.Count > 0)
                        //For each player
                        foreach (string killedPlayerId in result.killedPlayersDict.Keys)
                        {
                            message = new List<string>();
                            message.Add("PlayerKilled");
                            message.Add(killedPlayerId.SerializeObject());
                            message.Add(i_player.SerializeObject());
                            message.Add(result.killedPlayersDict[killedPlayerId].SerializeObject());
                            message.Add(DeathReason.PlayerHit.SerializeObject());
                            foreach (string player in gameSession.playerList)
                            {
                                SocketClient socket = new SocketClient();
                                socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                            }
                        }
                    //Saves "gamesession" state
                    await this.StateManager.SetStateAsync("gamesession", gameSession);
                    //If there's only one player alive
                    if (gameSession.AlivePlayers().Count == 1)
                    {
                        message = new List<string>();
                        message.Add("GameFinished");
                        message.Add(gameSession.AlivePlayers()[0].SerializeObject());
                        foreach (string player in gameSession.playerList)
                        {
                            SocketClient socket = new SocketClient();
                            socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                        }
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
                        List<string> message = new List<string>();
                        message.Add("RadarUsed");
                        message.Add(gameSession.GetPlayerPos(i_player).SerializeObject());
                        foreach (string player in gameSession.playerList)
                        {
                            SocketClient socket = new SocketClient();
                            socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                        }
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
            ActorEventSource.Current.Message("REMINDER RECIEVED");
            ActorEventSource.Current.Message("REMINDER NAME: "+reminderName);
            //Gets "gamesession" from Statemanager
            GameSession gameSession = await this.StateManager.GetStateAsync<GameSession>("gamesession");
            ActorEventSource.Current.Message("REMINDER: State gotten");
            //If LobbyCheck reminder
            if (reminderName.Equals("LobbyCheck"))
            {
                try
                {
                    ActorEventSource.Current.Message("REMINDER: LOBBY CHECK");
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
                        ActorEventSource.Current.Message("REMINDER: ALL PLAYERS");
                        //Unregister LobbyCheck reminder
                        await this.UnregisterReminderAsync(GetReminder("LobbyCheck"));
                        ActorEventSource.Current.Message("REMINDER: UNREGISTERED");
                        //Prepare game
                        gameSession.PrepareGame();
                        ActorEventSource.Current.Message("REMINDER: GAME PREPARED");
                        List<string> message = new List<string>();
                        ActorEventSource.Current.Message("REMINDER: STRINGLIST CREATED");
                        message.Add("GameStart");
                        ActorEventSource.Current.Message("REMINDER: GAMESTART ADDED");
                        message.Add(gameSession.getPlayerPositions.ToSerializable().SerializeObject());
                        ActorEventSource.Current.Message("REMINDER: PLAYER POSITIONS ADDED");
                        ActorEventSource.Current.Message("REMINDER: MESSAGE CREATED");
                        ActorEventSource.Current.Message("REMINDER: MESSAGE : " + message.SerializeObject());
                        ActorEventSource.Current.Message("REMINDER: MESSAGE LENGTH: " + message.SerializeObject().Length);
                        foreach (string player in gameSession.playerList)
                        {
                            SocketClient socket = new SocketClient();
                            socket.StartLobbyClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                        }
                        await this.RegisterReminderAsync("TurretAim", null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(-1));

                    }
                    //otherwise
                    else
                        //Update lobby info
                        await UpdateLobbyInfoAsync();
                }
                catch (Exception e)
                {
                    ActorEventSource.Current.Message(e.ToString());
                }
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
                    List<string> message = new List<string>();
                    message.Add("TurretAiming");
                    message.Add(aimPos.SerializeObject());
                    foreach (string player in gameSession.playerList)
                    {
                        SocketClient socket = new SocketClient();
                        socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                    }
                    await this.RegisterReminderAsync("TurretAim", new byte[] { 1, (byte)aimPos[0], (byte)aimPos[1] }, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
                }
                else
                {
                    int counter = context[0];
                    int[] aimPos = new int[] { context[1], context[2] };
                    counter++;
                    List<string> message = new List<string>();
                    message.Add("TurretAiming");
                    message.Add(aimPos.SerializeObject());
                    foreach (string player in gameSession.playerList)
                    {
                        SocketClient socket = new SocketClient();
                        socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                    }
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
                List<string> message = new List<string>();
                message.Add("BombHits");
                message.Add(result.hitPoints.SerializeObject());
                foreach (string player in gameSession.playerList)
                {
                    SocketClient socket = new SocketClient();
                    socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                }
                //If there are players killed by the attack
                if (result.killedPlayersDict.Count > 0)
                    //For each player
                    foreach (string killedPlayerId in result.killedPlayersDict.Keys)
                    {
                        message = new List<string>();
                        message.Add("PlayerDead");
                        message.Add(killedPlayerId.SerializeObject());
                        message.Add(result.killedPlayersDict[killedPlayerId].SerializeObject());
                        message.Add(DeathReason.Turret.SerializeObject());
                        foreach (string player in gameSession.playerList)
                        {
                            SocketClient socket = new SocketClient();
                            socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                        }
                    }
                //If there's only one player alive
                if (gameSession.AlivePlayers().Count == 1)
                {
                    message = new List<string>();
                    message.Add("GameFinished");
                    message.Add(gameSession.AlivePlayers()[0].SerializeObject());
                    foreach (string player in gameSession.playerList)
                    {
                        SocketClient socket = new SocketClient();
                        socket.StartGameSessionClient(gameSession.GetPlayerAddress(player), message.SerializeObject() + "<EOF>");
                    }
                }
                await this.RegisterReminderAsync("TurretAim", null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(-1));
            }
            //Saves "gamesession" state
            await this.StateManager.SetStateAsync("gameSession", gameSession);
        }
    }
}
