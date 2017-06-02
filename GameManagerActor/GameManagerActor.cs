using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using GameManagerActor.Interfaces;
using static GameManagerActor.Interfaces.MapInfo;
using static GameManagerActor.GameMap;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using LoginService.Interfaces;

namespace GameManagerActor
{
    [Serializable]
    class GameMap
    {
        //List of players on this game
        private List<string> p_playerList;
        //Players that has confirmed that are connected
        private List<string> p_connectedPlayers;
        private int p_maxPlayers = 2;
        private Dictionary<string, int[]> p_playerPositions;
        private MapInfo[][] p_gameMapInfo;
        private List<string> p_deadPlayers;
        public GameState state;

        public enum GameState
        {
            Lobby,
            Game,
            Results
        }

        #region GETTERS_AND_SETTERS

        public int playerCount
        {
            get
            {
                return p_playerList.Count;
            }
        }

        public int connectedPlayerCount
        {
            get
            {
                return p_connectedPlayers.Count;
            }
        }

        public List<string> playerList
        {
            get
            {
                return p_playerList;
            }
        }

        public bool isFull
        {
            get
            {
                return playerCount == p_maxPlayers;
            }
        }

        #endregion

        public GameMap()
        {
            p_playerList = new List<string>();
            p_connectedPlayers = new List<string>();
            p_playerPositions = new Dictionary<string, int[]>();
            p_deadPlayers = new List<string>();
        }

        /// <summary>
        /// Tries to add player to game
        /// </summary>
        /// <param name="i_playerId">Player to add</param>
        /// <returns>True if player could be added, false otherwise</returns>
        public bool AddPlayer(string i_playerId)
        {
            //If game is in LobbyState and number of players is lower than max
            if (state.Equals(GameState.Lobby) && p_playerList.Count < p_maxPlayers)
            {
                //Adds player to player list
                p_playerList.Add(i_playerId);
                //Adds player to list of players that still playing
                p_connectedPlayers.Add(i_playerId);
                //Player added correctly
                return true;
            }
            //otherwise
            return false;
        }

        /// <summary>
        /// Adds player to list of players who still playing the game (or waiting in the lobby)
        /// </summary>
        /// <param name="i_playerId">Player that stills playing</param>
        public void StillPlaying(string i_playerId)
        {
            //If player isn't on the list of players that still playing
            if (!p_connectedPlayers.Contains(i_playerId))
                //Adds it to the list
                p_connectedPlayers.Add(i_playerId);
        }

        //ADD WHILE PLAYING??
        /// <summary>
        /// Removes player from game
        /// </summary>
        /// <param name="i_playerId">Player to remove</param>
        public void RemovePlayer(string i_playerId)
        {
            //Removes player from players list and from list of players that still playing
            p_playerList.Remove(i_playerId);
            p_connectedPlayers.Remove(i_playerId);
        }

        /// <summary>
        /// Checks which players still playing
        /// </summary>
        /// <returns>List of players that are not playing</returns>
        public List<string> CheckPlayers()
        {
            //Creates list of players to remove
            List<string> removedPlayers = new List<string>();
            //For each player in game
            foreach (string player in p_playerList)
            {
                //If player stills playing
                if (p_connectedPlayers.Contains(player))
                {
                    //Removes player from list of players that still playing
                    p_connectedPlayers.Remove(player);
                }
                //otherwise
                else
                {
                    //Adds player to list of players to remove
                    removedPlayers.Add(player);
                }
            }
            //Returns list of players to remove
            return removedPlayers;
        }

        /// <summary>
        /// Prepares game for start playing
        /// </summary>
        public void PrepareGame()
        {
            p_gameMapInfo = new MapInfo[3][];
            for (int i = 0; i < 3; i++)
                p_gameMapInfo[i] = new MapInfo[3];
            p_gameMapInfo[0][0] = new MapInfo(p_playerList[0]);
            p_playerPositions.Add(p_playerList[0], new int[] { 0, 0 });
            p_gameMapInfo[2][2] = new MapInfo(p_playerList[1]);
            p_playerPositions.Add(p_playerList[1], new int[] { 2, 2 });
            p_gameMapInfo[1][1] = new MapInfo();
        }

        /// <summary>
        /// Kills the player
        /// </summary>
        /// <param name="i_playerId">Player to kill</param>
        public void KillPlayer(string i_playerId)
        {
            p_playerList.Remove(i_playerId);
            p_deadPlayers.Add(i_playerId);
        }

        //NEW TYPE TO RETURN?
        /// <summary>
        /// Moves the player and notifies if other player were killed with it
        /// </summary>
        /// <param name="i_dir">Movement vector</param>
        /// <param name="i_playerId">Player to move</param>
        /// <param name="o_playerPos">Player killed position</param>
        /// <param name="o_killedPlayer">Player killed</param>
        /// <returns>-1 if player isn't playing or is dead, 0 if player moved correctly, 1 if player died moving and 2 if player killed other player</returns>
        public int MovePlayer(int[] i_dir, string i_playerId, out int[] o_playerPos, out string o_killedPlayer)
        {
            o_playerPos = new int[] { };
            o_killedPlayer = string.Empty;
            int result = -1;
            ActorEventSource.Current.Message("Checking if dead");
            if (!p_deadPlayers.Contains(i_playerId))
            {
                ActorEventSource.Current.Message("Alive");
                result = 0;
                int[] playerPos = p_playerPositions[i_playerId];
                ActorEventSource.Current.Message("Player pos {0},{1}",playerPos[0],playerPos[1]);
                int[] destPos = new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] };
                ActorEventSource.Current.Message("Player pos def {0},{1}", destPos[0], destPos[1]);
                MapInfo destPosInfo = null;
                bool outOfBounds = false;
                if (destPos[0] < 0 || destPos[0] >= p_gameMapInfo.Length || destPos[1] < 0 || destPos[1] >= p_gameMapInfo[0].Length)
                    outOfBounds = true;
                else
                {
                    destPosInfo = p_gameMapInfo[destPos[0]][destPos[1]];
                }
                if (outOfBounds || (destPosInfo != null && destPosInfo.content.Equals(CellContent.Hole)))
                {
                    p_deadPlayers.Add(i_playerId);
                    p_gameMapInfo[playerPos[0]][playerPos[1]] = null;
                    o_playerPos = new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] };
                    result = 1;
                }
                else
                {
                    if (destPosInfo!= null && destPosInfo.content.Equals(CellContent.Player))
                    {
                        o_killedPlayer = destPosInfo.playerId;
                        p_deadPlayers.Add(o_killedPlayer);
                        o_playerPos = new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] };
                        result = 2;
                    }
                    p_gameMapInfo[playerPos[0]][playerPos[1]] = null;
                    p_gameMapInfo[playerPos[0] + i_dir[0]][playerPos[1] + i_dir[1]] = destPosInfo;
                    p_playerPositions.Remove(i_playerId);
                    p_playerPositions.Add(i_playerId, new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] });
                }
            }
            return result;
        }

        /// <summary>
        /// Manages player attack and notifies where attacked the player and which players killed
        /// </summary>
        /// <param name="i_playerId">Player that attacks</param>
        /// <param name="i_attackRate"></param>
        /// <param name="o_hitPoints">List of position vectors of attack hits</param>
        /// <param name="o_killedPlayersDict">Dictionary with player killed an their position</param>
        public void PlayerAttacks(string i_playerId, int i_attackRate, out List<int[]> o_hitPoints, out Dictionary<string,int[]> o_killedPlayersDict)
        {
            o_hitPoints = new List<int[]>();
            o_killedPlayersDict = new Dictionary<string, int[]>();

            if (!p_deadPlayers.Contains(i_playerId))
            {
                for (int i = -i_attackRate; i <= i_attackRate; i++)
                {
                    for (int j = -(i_attackRate - Math.Abs(i)); j <= i_attackRate - Math.Abs(i); j++)
                    {
                        int[] hitPos = new int[] { p_playerPositions[i_playerId][0] + i, p_playerPositions[i_playerId][1] + j };
                        bool outOfBounds = false;
                        if (hitPos[0] < 0 || hitPos[0] >= p_gameMapInfo.Length || hitPos[1] < 0 || hitPos[1] >= p_gameMapInfo[0].Length)
                            outOfBounds = true;
                        if (!outOfBounds && p_gameMapInfo[hitPos[0]][hitPos[1]] != null && p_gameMapInfo[hitPos[0]][hitPos[1]].content.Equals(CellContent.Player) && !p_gameMapInfo[hitPos[0]][hitPos[1]].playerId.Equals(i_playerId))
                        {
                            string deadPlayer = p_gameMapInfo[p_playerPositions[i_playerId][0] + i][p_playerPositions[i_playerId][1] + j].playerId;
                            p_gameMapInfo[p_playerPositions[i_playerId][0] + i][p_playerPositions[i_playerId][1] + j] = null;
                            p_deadPlayers.Add(deadPlayer);

                            o_killedPlayersDict.Add(deadPlayer, hitPos);
                        }
                        else
                        {
                            o_hitPoints.Add(hitPos);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets player position
        /// </summary>
        /// <param name="i_playerId">Player whose position will be returned</param>
        /// <returns>Player position vector</returns>
        public int[] GetPlayerPos(string i_playerId)
        {
            return p_playerPositions[i_playerId];
        }

        /// <summary>
        /// Manages player's radar
        /// </summary>
        /// <param name="i_playerId">Player that used radar</param>
        /// <returns>Map info for this player</returns>
        public CellContent[][] RadarActivated(string i_playerId)
        {
            CellContent[][] res = new CellContent[5][];
            for (int i = 0; i < 5; i++)
            {
                res[i] = new CellContent[5];
            }
            int[] playerPosReal = p_playerPositions[i_playerId];
            int[] playerPosVirt = new int[] { (int)Math.Floor(res.Length / 2f), (int)Math.Floor(res[0].Length/2f) };
            for (int i = 0; i < res.Length; i++)
            {
                for (int j = 0; j < res[0].Length; j++)
                {
                    if (i!=playerPosVirt[0] || j!=playerPosVirt[1])
                    {
                        int[] realPos = new int[] { playerPosReal[0] - playerPosVirt[0] + i, playerPosReal[1] - playerPosVirt[1] + j };
                        if ((realPos[0] < 0) || (realPos[0] >= p_gameMapInfo.Length) || (realPos[1] < 0) || (realPos[1] >= p_gameMapInfo[0].Length))
                        {
                            res[i][j] = CellContent.Hole;
                        }
                        else
                        {
                            res[i][j] = p_gameMapInfo[realPos[0]][realPos[1]] == null ? CellContent.Floor : p_gameMapInfo[realPos[0]][realPos[1]].content;
                        }
                    }
                }
            }
            return res;
        }
    }

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
        /// GameMap state initialization
        /// </summary>
        public async Task InitializeGameAsync()
        {
            //Creates GameMap
            GameMap gameMap = new GameMap();
            //Saves GameMap as "gamemap" state
            await this.StateManager.SetStateAsync("gamemap", gameMap);
        }

        //RETURN STATE AS ENUM???
        /// <summary>
        /// Connect player to game
        /// </summary>
        /// <param name="i_playerId">Player to connect</param>
        /// <returns>0 if player could be connected, 1 if game is full or started and 2 if game was removed</returns>
        public async Task<int> ConnectPlayerAsync(string i_playerId)
        {
            try
            {
                //Get GameMap from StateManager
                GameMap gameMap = await this.StateManager.GetStateAsync<GameMap>("gamemap");    
                //Adds player to game if posible
                if (gameMap.AddPlayer(i_playerId))
                {
                    //If first player in lobby
                    if (gameMap.playerCount == 1)
                        //Registers LobyCheck reminder
                        await this.RegisterReminderAsync("LobbyCheck", null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                    //Saves "gamemap" state
                    await this.StateManager.SetStateAsync("gamemap", gameMap);
                    ILoginService login = ServiceProxy.Create<ILoginService>(new Uri(ServiceUri.AbsoluteUri.Replace("GameManagerActorService", "LoginService")));
                    await login.AddPlayerAsync(Id.ToString());
                    //Returns 0 (player correctly connected)
                    return 0;
                }
                //Saves "gamemap" state
                await this.StateManager.SetStateAsync("gamemap", gameMap);
                //Returns 1 (player couldn't be added: game full or started)
                return 1;
            }
            //Catch exception if "gamemap" state doesn't exist
            catch (Exception e)
            {
                //Returns 2 (player couldn't be added: game removed)
                return 2;
            }
        }

        /// <summary>
        /// Send ActorEvent with lobby info to clients
        /// </summary>
        public async Task UpdateLobbyInfoAsync()
        {
            //Gets "gamemap" from StateManager
            GameMap gameMap = await this.StateManager.GetStateAsync<GameMap>("gamemap");
            //If game is in Lobby state
            if (gameMap.state.Equals(GameState.Lobby))
            {
                //Gets IGameLobbyEvents and sends GameLobbyInfoUpdate event with list of players
                var ev = GetEvent<IGameLobbyEvents>();
                ev.GameLobbyInfoUpdate(gameMap.playerList);

            }
        }

        /// <summary>
        /// Recieves notification from player's client that player stills connected
        /// </summary>
        /// <param name="i_playerId">Player that stills connected</param>
        public async Task PlayerStillConnectedAsync(string i_playerId)
        {
            //Gets "gamemap" from StateManager
            GameMap gameMap = await this.StateManager.GetStateAsync<GameMap>("gamemap");
            //Notifies that player stills connected
            gameMap.StillPlaying(i_playerId);
            //Saves "gamemap" state
            await this.StateManager.SetStateAsync("gamemap", gameMap);
        }

        //REMOVE GAME WHEN NO PLAYERS (REMINDER)???
        /// <summary>
        /// Disconnect player from game
        /// </summary>
        /// <param name="i_playerId">Player to disconnect</param>
        public async Task PlayerDisconnectAsync(string i_playerId)
        {
            //Gets "gamemap" from StateManagers
            GameMap gameMap = await this.StateManager.GetStateAsync<GameMap>("gamemap");
            //Removes player from game
            gameMap.RemovePlayer(i_playerId);
            //If game is in Lobby state and there's no players
            if (gameMap.state.Equals(GameState.Lobby) && gameMap.connectedPlayerCount == 0)
            {
                //Gets LobbyCheck reminder and unregisters it
                IActorReminder reminder = GetReminder("LobbyCheck");
                await UnregisterReminderAsync(reminder);
            }
            //Saves "gamemap" state
            await this.StateManager.SetStateAsync("gamemap", gameMap);
            ILoginService login = ServiceProxy.Create<ILoginService>(new Uri(ServiceUri.AbsoluteUri.Replace("GameManagerActorService","LoginService")));
            await login.RemovePlayerAsync(Id.ToString());
        }

        /*
        /// <summary>
        /// Notifies clients that game has been started
        /// </summary>
        public async Task StartGameAsync()
        {
            var ev = GetEvent<IGameLobbyEvents>();
            ev.GameStart();
        }
        */

        // UNCOMMENTED
        /// <summary>
        /// Manages player attack and notifies client about it
        /// </summary>
        /// <param name="i_playerId">Player that attacked</param>
        public async Task PlayerAttacksAsync(string i_playerId)
        {
            List<int[]> i_hitList;
            Dictionary<string, int[]> i_killedPlayersDict;
            GameMap gameMap = await this.StateManager.GetStateAsync<GameMap>("gamemap");
            gameMap.PlayerAttacks(i_playerId,PLAYER_ATTACK_RATE,out i_hitList,out i_killedPlayersDict);
            var ev = GetEvent<IGameEvents>();
            ev.BombHits(i_hitList);
            if (i_killedPlayersDict.Count > 0)
                foreach (string killedPlayerId in i_killedPlayersDict.Keys)
                {
                    ev.PlayerKilled(killedPlayerId, i_playerId, i_killedPlayersDict[killedPlayerId]);
                }
            await this.StateManager.SetStateAsync("gamemap", gameMap);
        }

        //UNCOMMENTED
        /// <summary>
        /// Manages player movement
        /// </summary>
        /// <param name="i_dir">Movement vector</param>
        /// <param name="i_playerId">Player that is moved</param>
        public async Task PlayerMovesAsync(int[] i_dir, string i_playerId)
        {
            int[] playerPos;
            string killedPlayer;
            ActorEventSource.Current.Message("Getting gamemap");
            GameMap gameMap = await this.StateManager.GetStateAsync<GameMap>("gamemap");
            ActorEventSource.Current.Message("Gamemap gotten: {0}",gameMap);
            int result = gameMap.MovePlayer(i_dir, i_playerId, out playerPos, out killedPlayer);
            if (result > 0)
            {
                var ev = GetEvent<IGameEvents>();
                if (result == 1)
                    ev.PlayerDead(i_playerId, 0, playerPos);
                if (result == 2)
                    ev.PlayerKilled(killedPlayer, i_playerId, playerPos);
            }
            await this.StateManager.SetStateAsync("gamemap", gameMap);

        }

        /// <summary>
        /// Gets player position
        /// </summary>
        /// <param name="i_playerId">Player whose position is returned</param>
        /// <returns>Position vector</returns>
        public async Task<int[]> GetPlayerPosAsync(string i_playerId)
        {
            //Gets "gamemap" from StateManager
            GameMap gameMap = await this.StateManager.GetStateAsync<GameMap>("gamemap");


            /*
            ActorEventSource.Current.Message("Getting position");
            ActorEventSource.Current.Message("Player {0}",i_playerId);
            ActorEventSource.Current.Message("Position {0},{1}", gameMap.GetPlayerPos(i_playerId)[0], gameMap.GetPlayerPos(i_playerId)[1]);
            */

            //Returns player position
            int[] res = gameMap.GetPlayerPos(i_playerId);
            return res;
        }

        /// <summary>
        /// Manages player's radar, return info to player and notifies other players about it
        /// </summary>
        /// <param name="i_playerId">Player that used radar</param>
        /// <returns>Map info for this player</returns>
        public async Task<CellContent[][]> RadarActivatedAsync(string i_playerId)
        {
            //Gets "gamemap" from StateManager
            GameMap gameMap = await this.StateManager.GetStateAsync<GameMap>("gamemap");
            //Gets IGameEvents and send RadarUsed to clients with player position vector
            var ev = GetEvent<IGameEvents>();
            ev.RadarUsed(gameMap.GetPlayerPos(i_playerId));
            //Returns map info to player
            CellContent[][] res = gameMap.RadarActivated(i_playerId);
            return res;
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            //Gets "gamemap" from Statemanager
            GameMap gameMap = await this.StateManager.GetStateAsync<GameMap>("gamemap");
            //If LobbyCheck reminder
            if (reminderName.Equals("LobbyCheck"))
            {
                //Get players that are not connected
                List<string> removedPlayers = gameMap.CheckPlayers();
                //If there are players
                if (removedPlayers.Count > 0)
                    //Disconnect each player
                    foreach (string removingPlayer in removedPlayers)
                        await PlayerDisconnectAsync(removingPlayer);
            }
            //If game is in Lobby state and lobby is full
            if (gameMap.state.Equals(GameState.Lobby) && gameMap.isFull)
            {
                //Unregister LobbyCheck reminder
                await this.UnregisterReminderAsync(GetReminder("LobbyCheck"));
                //Prepare game
                gameMap.PrepareGame();
                //Saves "gamemap" state
                await this.StateManager.SetStateAsync("gamemap", gameMap);
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
