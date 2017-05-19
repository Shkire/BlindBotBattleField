﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using GameManagerActor.Interfaces;
using static GameManagerActor.Interfaces.MapInfo;

namespace GameManagerActor
{
    class GameMap
    {
        //List of players on this game
        private List<string> p_playerList;

        //Players that has confirmed that are connected
        private List<string> p_connectedPlayers;

        private int p_maxPlayers = 2;

        private Dictionary<string, int[]> p_playerPositions;

        private MapInfo[,] p_gameMapInfo;

        private List<string> p_deadPlayers;

        private GameManagerActor p_actor;

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

        public GameMap(GameManagerActor i_actor)
        {
            p_playerList = new List<string>();
            p_connectedPlayers = new List<string>();
            p_playerPositions = new Dictionary<string, int[]>();
            p_deadPlayers = new List<string>();
            p_actor = i_actor;
        }

        public void Initialize()
        {
            p_gameMapInfo = new MapInfo[3, 3];
            p_gameMapInfo[0, 0] = new MapInfo(p_playerList[0]);
            p_playerPositions.Add(p_playerList[0], new int[] { 0, 0 });
            p_gameMapInfo[2, 2] = new MapInfo(p_playerList[1]);
            p_playerPositions.Add(p_playerList[1], new int[] { 2, 2 });
            p_gameMapInfo[1, 1] = new MapInfo();
        }

        public void KillPlayer(string i_playerId)
        {
            p_playerList.Remove(i_playerId);
            p_deadPlayers.Add(i_playerId);
        }

        public bool ConnectPlayer(string i_playerId)
        {
            if (p_playerList.Count < p_maxPlayers)
            {
                p_playerList.Add(i_playerId);
                p_connectedPlayers.Add(i_playerId);
                return true;
            }
            return false;
        }

        public void DisconnectPlayer(string i_playerId)
        {
            p_playerList.Remove(i_playerId);
            p_connectedPlayers.Remove(i_playerId);
        }

        public void PlayerConnected(string i_playerId)
        {
            if (!p_connectedPlayers.Contains(i_playerId))
                p_connectedPlayers.Add(i_playerId);
        }

        public async Task CheckLobbyAsync()
        {
            for (int i = 0; i < p_playerList.Count; i++)
            {
                if (p_connectedPlayers.Contains(p_playerList[i]))
                {
                    p_connectedPlayers.Remove(p_playerList[i]);
                }
                else
                {
                    await p_actor.PlayerDisconnectAsync(p_playerList[i]);
                    i--;
                }
            }
        }

        public int MovePlayer(int[] i_dir, string i_playerId, ref int[] o_playerPos, ref string o_killedPlayer)
        {
            int result = -1;
            if (!p_deadPlayers.Contains(i_playerId))
            {
                result = 0;
                int[] playerPos = p_playerPositions[i_playerId];
                int[] destPos = new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] };
                MapInfo destPosInfo = null;
                bool outOfBounds = false;
                if (destPos[0] < 0 || destPos[0] >= p_gameMapInfo.GetLength(0) || destPos[1] < 0 || destPos[1] >= p_gameMapInfo.GetLength(1))
                    outOfBounds = true;
                else
                {
                    destPosInfo = p_gameMapInfo[destPos[0], destPos[1]];
                }
                if (outOfBounds || (destPosInfo != null && destPosInfo.content.Equals(CellContent.Hole)))
                {
                    p_deadPlayers.Add(i_playerId);
                    p_gameMapInfo[playerPos[0], playerPos[1]] = null;
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
                    p_gameMapInfo[playerPos[0], playerPos[1]] = null;
                    p_gameMapInfo[playerPos[0] + i_dir[0], playerPos[1] + i_dir[1]] = destPosInfo;
                    p_playerPositions.Remove(i_playerId);
                    p_playerPositions.Add(i_playerId, new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] });
                }
            }
            return result;
        }

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
                        if (hitPos[0] < 0 || hitPos[0] >= p_gameMapInfo.GetLength(0) || hitPos[1] < 0 || hitPos[1] >= p_gameMapInfo.GetLength(1))
                            outOfBounds = true;
                        if (!outOfBounds && p_gameMapInfo[hitPos[0],hitPos[1]] != null && p_gameMapInfo[hitPos[0], hitPos[1]].content.Equals(CellContent.Player) && !p_gameMapInfo[hitPos[0], hitPos[1]].playerId.Equals(i_playerId))
                        {
                            string deadPlayer = p_gameMapInfo[p_playerPositions[i_playerId][0] + i, p_playerPositions[i_playerId][1] + j].playerId;
                            p_gameMapInfo[p_playerPositions[i_playerId][0] + i, p_playerPositions[i_playerId][1] + j] = null;
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

        public int[] GetPlayerPos(string i_playerId)
        {
            return p_playerPositions[i_playerId];
        }

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
                        if ((realPos[0] < 0) || (realPos[0] >= p_gameMapInfo.GetLength(0)) || (realPos[1] < 0) || (realPos[1] >= p_gameMapInfo.GetLength(1)))
                        {
                            res[i][j] = CellContent.Hole;
                        }
                        else
                        {
                            res[i][j] = p_gameMapInfo[realPos[0], realPos[1]] == null ? CellContent.Floor : p_gameMapInfo[realPos[0], realPos[1]].content;
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

        private GameMap p_gameMap;

        private int p_state;

        /// <summary>
        /// Inicializa una instancia nueva de GameManagerActor
        /// </summary>
        /// <param name="actorService">El atributo Microsoft.ServiceFabric.Actors.Runtime.ActorService que hospedará esta instancia de actor.</param>
        /// <param name="actorId">El atributo Microsoft.ServiceFabric.Actors.ActorId de esta instancia de actor.</param>
        public GameManagerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            p_gameMap = new GameMap(this);
            p_state = 0;
        }

        public async Task PlayerAttacksAsync(string i_playerId)
        {
            List<int[]> i_hitList;
            Dictionary<string, int[]> i_killedPlayersDict;
            p_gameMap.PlayerAttacks(i_playerId,PLAYER_ATTACK_RATE,out i_hitList,out i_killedPlayersDict);
            var ev = GetEvent<IGameEvents>();
            ev.BombHits(i_hitList);
            if (i_killedPlayersDict.Count > 0)
                foreach (string killedPlayerId in i_killedPlayersDict.Keys)
                {
                    ev.PlayerKilled(killedPlayerId, i_playerId, i_killedPlayersDict[killedPlayerId]);
                }
        }

        public async Task PlayerDisconnectAsync(string i_playerId)
        {
            p_gameMap.DisconnectPlayer(i_playerId);
            if (p_gameMap.connectedPlayerCount == 0)
            {
                IActorReminder reminder = GetReminder("LobbyCheck");
                await UnregisterReminderAsync(reminder);
            }

            //Actualizar para durante juego
        }

        public async Task PlayerMovesAsync(int[] i_dir, string i_playerId)
        {
            int[] playerPos = null;
            string killedPlayer = null;
            int result = p_gameMap.MovePlayer(i_dir, i_playerId, ref playerPos, ref killedPlayer);
            if (result > 0)
            {
                var ev = GetEvent<IGameEvents>();
                if (result == 1)
                    ev.PlayerDead(i_playerId, 0, playerPos);
                if (result == 2)
                    ev.PlayerKilled(killedPlayer, i_playerId, playerPos);
            }

        }
        //!!!!!Player Id collision problem
        public async Task<bool> PlayerRegisterAsync(string i_playerId)
        {
            int playerCount = p_gameMap.playerCount;
            bool result = p_gameMap.ConnectPlayer(i_playerId);
            if (playerCount == 0  && result)
                await this.RegisterReminderAsync("LobbyCheck", null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            if (p_gameMap.isFull)
                p_state = 1;
            return result;
        }

        public async Task PlayerStillConnectedAsync(string i_playerId)
        {
            p_gameMap.PlayerConnected(i_playerId);
        }

        public async Task UpdateLobbyInfoAsync()
        {
            var ev = GetEvent<IGameLobbyEvents>();
            ev.GameLobbyInfoUpdate(p_gameMap.playerList);
        }

        public async Task StartGameAsync()
        {
            var ev = GetEvent<IGameLobbyEvents>();
            ev.GameStart();
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName.Equals("LobbyCheck"))
                await p_gameMap.CheckLobbyAsync();
            if (p_state == 1 && p_gameMap.isFull)
            {
                await this.UnregisterReminderAsync(GetReminder("LobbyCheck"));
                p_gameMap.Initialize();
                await StartGameAsync();
            }
            else
                await UpdateLobbyInfoAsync();
        }

        public Task<int[]> GetPlayerPos(string i_playerId)
        {
            /*
            ActorEventSource.Current.Message("Getting position");
            ActorEventSource.Current.Message("Player {0}",i_playerId);
            ActorEventSource.Current.Message("Position {0},{1}", p_gameMap.GetPlayerPos(i_playerId)[0], p_gameMap.GetPlayerPos(i_playerId)[1]);
            */
            int[] res = p_gameMap.GetPlayerPos(i_playerId);
            return Task.FromResult(res);
        }

        public Task<CellContent[][]> RadarActivated(string i_playerId)
        {
            var ev = GetEvent<IGameEvents>();
            ev.RadarUsed(p_gameMap.GetPlayerPos(i_playerId));
            return Task.FromResult(p_gameMap.RadarActivated(i_playerId));
        }
    }
}
