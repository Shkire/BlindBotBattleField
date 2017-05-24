using GameManagerActor.Interfaces;
using LoginService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
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
            Task.Run(() => { });
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
            p_gameManager.StartGame();
        }
    }

    class GameEventsHandler : IGameEvents
    {

        private GameManager p_gameManager;

        public GameEventsHandler(GameManager i_gameManager)
        {
            p_gameManager = i_gameManager;
        }

        public void BombHits(List<int[]> i_hitList)
        {
            p_gameManager.BombHitsRecieved(i_hitList);
        }

        public void PlayerDead(string i_playerId, int i_reason, int[] i_playerPos)
        {
            p_gameManager.PlayerDeadRecieved(i_playerPos);
            Console.WriteLine("Player "+i_playerId+" is dead");
        }

        public void PlayerKilled(string i_playerKilledId, string i_playerKillerId, int[] i_playerPos)
        {
            p_gameManager.PlayerDeadRecieved(i_playerPos);
            Console.WriteLine("Player "+i_playerKillerId+" killed "+i_playerKilledId);
        }

        public void RadarUsed(int[] i_playerPos)
        {
            p_gameManager.PlayerDetected(i_playerPos);
        }
    }

    class GameManager
    {
        public int state;
        private int[] p_playerPosReal;
        private int[] p_playerPosVirt;
        private CellContent[,] p_playerSight;
        private int p_playerSightRange = 5;
        private IGameManagerActor p_actor;
        private ILoginService p_service;
        public string playerName;
        public string pass;

        public IGameManagerActor actor
        {
            set
            {
                p_actor = value;
            }
        }

        public ILoginService service
        {
            set
            {
                p_service = value;
            }
        }

        public GameManager()
        {
            state = 1;
        }

        public bool PlayerRegistration()
        {
            return p_service.Login(playerName,pass).Result && p_actor.PlayerRegisterAsync(playerName).Result;
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

        public void StartGame()
        {
            Task.Run(() => { });
            state = 2;
            p_playerPosReal = new int[] { 0, 0 };
            /*
            Console.WriteLine("Getting Pos");
            Task<int[]> resTask = p_actor.GetPlayerPosAsync(playerName);
            while (!resTask.IsCompleted)
                Console.WriteLine("Waiting");
            p_playerPosReal = resTask.Result;
            Console.WriteLine("Pos gotten");
            */
            p_playerPosVirt = new int[] { (int)Math.Truncate(p_playerSightRange / 2f), (int)Math.Truncate(p_playerSightRange / 2f) };
            p_playerSight = new CellContent[p_playerSightRange, p_playerSightRange];
            SubscribeToGameEvents();
        }

        public void MovePlayer(int[] i_dir)
        {
            Task.WaitAll(p_actor.PlayerMovesAsync(i_dir, playerName));

            p_playerSight[p_playerPosVirt[0], p_playerPosVirt[1]] = CellContent.Floor;
            p_playerPosReal[0] += i_dir[0];
            p_playerPosReal[1] += i_dir[1];
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
            Console.Beep();
        }

        public void PlayerAttacks()
        {
            Task.WaitAll(p_actor.PlayerAttacksAsync(playerName));
            Console.Beep();
        }

        public void RadarUsed()
        {
            CellContent[][] res = p_actor.RadarActivatedAsync(playerName).Result;
            for (int i = 0; i < p_playerSight.GetLength(0); i++)
            {
                for (int j = 0; j < p_playerSight.GetLength(1); j++)
                {
                    p_playerSight[i, j] = res[i][j];
                }
            }
            RefreshClient();
            Console.Beep();
        }

        public void PlayerDeadRecieved(int[] i_deadPos)
        {
            int[] deadPosVirt = new int[] { i_deadPos[0] - p_playerPosReal[0] + p_playerPosVirt[0], i_deadPos[1] - p_playerPosReal[1] + p_playerPosVirt[1]};
            if (!(deadPosVirt[0] < 0 || deadPosVirt[0] >= p_playerSight.GetLength(0) || deadPosVirt[1] < 0 || deadPosVirt[1] >= p_playerSight.GetLength(1)))
                p_playerSight[deadPosVirt[0], deadPosVirt[1]] = CellContent.Dead;
            RefreshClient();
        }

        public void BombHitsRecieved(List<int[]> i_hitPos)
        {
            foreach (int[] hit in i_hitPos)
            {
                int[] hitPosVirt = new int[] { hit[0] - p_playerPosReal[0] + p_playerPosVirt[0], hit[1] - p_playerPosReal[1] + p_playerPosVirt[1] };
                if (!(hitPosVirt[0] < 0 || hitPosVirt[0] >= p_playerSight.GetLength(0) || hitPosVirt[1] < 0 || hitPosVirt[1] >= p_playerSight.GetLength(1)))
                    p_playerSight[hitPosVirt[0], hitPosVirt[1]] = CellContent.Hit;
            }
            RefreshClient();
        }

        public void PlayerDetected(int[] i_playerPos)
        {
            int[] playerPosVirt = new int[] { i_playerPos[0] - p_playerPosReal[0] + p_playerPosVirt[0], i_playerPos[1] - p_playerPosReal[1] + p_playerPosVirt[1] };
            if (!(playerPosVirt[0] < 0 || playerPosVirt[0] >= p_playerSight.GetLength(0) || playerPosVirt[1] < 0 || playerPosVirt[1] >= p_playerSight.GetLength(1)))
                p_playerSight[playerPosVirt[0], playerPosVirt[1]] = CellContent.Player;
            RefreshClient();
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
                            case CellContent.Dead:
                                res += "D ";
                                break;
                            case CellContent.Hit:
                                res += "B ";
                                break;
                            case CellContent.Player:
                                res += "E ";
                                break;
                            case CellContent.Hole:
                                res += "O ";
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
                    gameManager.service = ServiceProxy.Create<ILoginService>(new Uri(APP_NAME + "/LoginService"));
                    Console.WriteLine("Choose your player name:");
                    gameManager.playerName = Console.ReadLine();
                    Console.WriteLine("Password:");
                    gameManager.pass = Console.ReadLine();
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
                    Console.WriteLine("Game Started");
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
                    case ConsoleKey.Spacebar: gameManager.RadarUsed();
                        break;
                }
            }
        }
    }
}
