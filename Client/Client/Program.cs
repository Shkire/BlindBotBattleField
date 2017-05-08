﻿using GameManagerActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameManagerActor.Interfaces.MapInfo;

namespace Client
{

    class LobbyEventsHandler : IGameLobbyEvents
    {
        private GameManager p_gameManager;

        public LobbyEventsHandler(GameManager i_gameManager)
        {
            p_gameManager = i_gameManager;
        }

        public void GameLobbyInfoUpdate(List<string> i_playerIdMap)
        {
            Console.Clear();
            for (int i = 0; i < i_playerIdMap.Count; i++)
            {
                Console.WriteLine((i+1).ToString()+". "+i_playerIdMap[i]);
            }
            p_gameManager.PlayerStillConnected();
        }

        public void GameStart()
        {
            p_gameManager.state = 2;
            p_gameManager.Init(new int[] { 0, 0 }, 5);
            p_gameManager.SubscribeToGameEvents();
        }
    }

    class GameEventsHandler : IGameEvents
    {

        private GameManager p_gameManager;

        public GameEventsHandler(GameManager i_gameManager)
        {
            p_gameManager = i_gameManager;
        }

        public void BombHits(List<int[]> o_hitList)
        {
            throw new NotImplementedException();
        }

        public void PlayerDead(string i_playerId, int i_reason, int[] i_playerPos)
        {
            Console.WriteLine("El jugador "+i_playerId+" ha muerto");
        }

        public void PlayerKilled(string i_playerKilledId, string i_playerKillerId, int[] i_playerPos)
        {
            Console.WriteLine("El jugador "+i_playerKillerId+" ha matado al jugador "+i_playerKilledId);
        }
    }

    class GameManager
    {
        public int state;
        private int[] p_playerPosReal;
        private int[] p_playerPosVirt;
        private CellContent[,] p_playerSight;
        private IGameManagerActor p_actor;
        public string playerName;

        public IGameManagerActor actor
        {
            set
            {
                p_actor = value;
            }
        }

        public GameManager()
        {
            state = 1;
        }

        public bool PlayerRegistration()
        {
            return p_actor.PlayerRegisterAsync(playerName).Result;
        }

        public void PlayerStillConnected()
        {
            p_actor.PlayerStillConnectedAsync(playerName);
        }

        public void SubscribeToLobbyEvents()
        {
            p_actor.SubscribeAsync<IGameLobbyEvents>(new LobbyEventsHandler(this));
        }

        public void SubscribeToGameEvents()
        {
            p_actor.SubscribeAsync<IGameEvents>(new GameEventsHandler(this));
        }

        public void UpdateLobby()
        {
            p_actor.UpdateLobbyInfoAsync();
        }

        public void Init(int[] i_iniPlayerPos, int i_playerSightRange)
        {
            p_playerPosReal = i_iniPlayerPos;
            p_playerPosVirt = new int[] { (int)Math.Truncate(i_playerSightRange / 2f), (int)Math.Truncate(i_playerSightRange / 2f) };
            p_playerSight = new CellContent[i_playerSightRange, i_playerSightRange];
        }

        public void MovePlayer(int[] i_dir)
        {
            Task.WaitAll(p_actor.PlayerMovesAsync(i_dir, playerName));

            p_playerSight[p_playerPosVirt[0], p_playerPosVirt[1]] = CellContent.Floor;
            int iniXPos = 0;
            int iniYPos = 0;
            int finXPos = p_playerSight.GetLength(0) - 1;
            int finYPos = p_playerSight.GetLength(1) - 1;
            int xInc = 1;
            int yInc = 1;
            if (i_dir[0] < 0)
            {
                iniXPos = finXPos;
                finXPos = 0;
                xInc = -1;
            }
            if (i_dir[1] < 0)
            {
                iniYPos = finYPos;
                finYPos = 0;
                yInc = -1;
            }
            for (int i = iniXPos, contX = 0; contX < p_playerSight.GetLength(0); i = i + xInc, contX++)
            {
                for (int j = iniYPos, contY = 0; contY < p_playerSight.GetLength(0); j = j + yInc, contY++)
                {
                    int destX = i + i_dir[0];
                    int destY = j + i_dir[1];
                    p_playerSight[i, j] = (destX < 0 || destX > p_playerSight.GetLength(0) - 1 || destY < 0 || destY > p_playerSight.GetLength(1) - 1) ? CellContent.None : p_playerSight[destX, destY];
                }
            }
            RefreshClient();
        }

        public void PlayerAttacks()
        {
            p_actor.PlayerAttacksAsync(playerName);
        }

        public void RefreshClient()
        {
            Console.Clear();
            for (int j = p_playerSight.GetLength(1) - 1; j >= 0; j--)
            {
                string res = string.Empty;
                for (int i = 0; i < p_playerSight.GetLength(0); i++)
                {
                    if (i == p_playerPosVirt[0] && j == p_playerPosVirt[1])
                        res += "P ";
                    else
                    {
                        switch (p_playerSight[i, j])
                        {
                            case CellContent.None:
                                res += "? ";
                                break;
                            case CellContent.Floor:
                                res += "X ";
                                break;
                        }
                    }
                }
                Console.WriteLine(res);
            }
        }
    }

    class Program
    {

        const string APP_NAME = "fabric:/BlindBotBattleField";

        static void Main(string[] args)
        {
            bool exit = false;
            GameManager gameManager = new GameManager();
            while (!exit)
            {
                if (gameManager.state == 1)
                {
                    Console.WriteLine("Choose your player name:");
                    gameManager.playerName = Console.ReadLine();
                    Console.WriteLine("Connecting server...");
                    gameManager.actor = ActorProxy.Create<IGameManagerActor>(new ActorId("Manager"), APP_NAME);
                    bool registrationSuccess = gameManager.PlayerRegistration();
                    if (registrationSuccess)
                    {
                        Console.WriteLine("Success");
                        gameManager.SubscribeToLobbyEvents();
                        gameManager.UpdateLobby();
                        Console.WriteLine("Waiting");
                    }
                    else
                    {
                        Console.WriteLine("Not Success");
                    }
                }
                else if (gameManager.state == 2)
                {
                    Console.WriteLine("Juego Comenzado");
                }
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow: gameManager.MovePlayer(new int[] { 0, 1 }); ;
                        break;
                    case ConsoleKey.DownArrow: gameManager.MovePlayer(new int[] { 0, -1 });
                        break;
                    case ConsoleKey.RightArrow: gameManager.MovePlayer(new int[] { 1, 0 });
                        break;
                    case ConsoleKey.LeftArrow: gameManager.MovePlayer(new int[] { -1, 0 });
                        break;
                    case ConsoleKey.Enter: gameManager.PlayerAttacks();
                        break;
                }
            }
        }
    }
}
