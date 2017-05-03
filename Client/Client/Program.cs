using GameManagerActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{

    class LobbyEventsHandler : IGameLobbyEvents
    {
        private IGameManagerActor p_actor;

        private string p_playerId;

        private GameManager p_gameManager;

        public LobbyEventsHandler(GameManager i_gameManager, IGameManagerActor i_actor, string i_playerId)
        {
            p_actor = i_actor;
            p_playerId = i_playerId;
            p_gameManager = i_gameManager;
        }

        public void GameLobbyInfoUpdate(List<string> i_playerIdMap)
        {
            Console.Clear();
            for (int i = 0; i < i_playerIdMap.Count; i++)
            {
                Console.WriteLine((i+1).ToString()+". "+i_playerIdMap[i]);
            }
            p_actor.PlayerStillConnectedAsync(p_playerId);
        }

        public void GameStart()
        {
            p_gameManager.state = 2;
            p_actor.SubscribeAsync<IGameEvents>(new GameEventsHandler(p_gameManager, p_playerId));
        }
    }

    class GameEventsHandler : IGameEvents
    {

        private GameManager p_gameManager;

        private string p_playerName;

        public GameEventsHandler(GameManager i_gameManager, string i_playerId)
        {
            p_gameManager = i_gameManager;
            p_playerName = i_playerId;
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
        public int state = 1;
    }

    class Program
    {

        const string APP_NAME = "fabric:/BlindBotBattleField";

        static void Main(string[] args)
        {
            bool exit = false;
            GameManager gameManager = new GameManager();
            IGameManagerActor actor = null;
            string playerName = null;
            while (!exit)
            {
                if (gameManager.state == 1)
                {
                    Console.WriteLine("Choose your player name:");
                    playerName = Console.ReadLine();
                    Console.WriteLine("Connecting server...");
                    actor = ActorProxy.Create<IGameManagerActor>(new ActorId("Manager"), APP_NAME);
                    bool registrationSuccess = actor.PlayerRegisterAsync(playerName).Result;
                    if (registrationSuccess)
                    {
                        Console.WriteLine("Success");
                        actor.SubscribeAsync<IGameLobbyEvents>(new LobbyEventsHandler(gameManager, actor, playerName));
                        actor.UpdateLobbyInfoAsync();
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
                int[] desp = new int[] { 0,0};
                if (gameManager.state == 2 && key.Key.Equals(ConsoleKey.UpArrow))
                {
                    desp = new int[] { 0, 1 };
                    actor.PlayerMovesAsync(desp, playerName);
                }
                else if (gameManager.state == 2 && key.Key.Equals(ConsoleKey.DownArrow))
                {
                    desp = new int[] { 0, -1 };
                    actor.PlayerMovesAsync(desp, playerName);
                }
                else if (gameManager.state == 2 && key.Key.Equals(ConsoleKey.RightArrow))
                {
                    desp = new int[] { 1, 0 };
                    actor.PlayerMovesAsync(desp, playerName);
                }
                else if (gameManager.state == 2 && key.Key.Equals(ConsoleKey.LeftArrow))
                {
                    desp = new int[] { -1, 0 };
                    actor.PlayerMovesAsync(desp, playerName);
                }
                else if (gameManager.state == 2 && key.Key.Equals(ConsoleKey.Enter))
                    actor.PlayerAttacksAsync(playerName);
                Console.WriteLine("Mueve " + desp[0] + "," + desp[1]);
            }
        }
    }
}
