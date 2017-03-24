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
        public void GameLobbyInfoUpdate(List<string> i_playerIdMap)
        {
            Console.Clear();
            for (int i = 0; i < i_playerIdMap.Count; i++)
            {
                Console.WriteLine((i+1).ToString()+". "+i_playerIdMap[i]);
            }
        }
    }

    class Program
    {

        const string APP_NAME = "fabric:/BlindBotBattleField";

        static void Main(string[] args)
        {
            Console.WriteLine("Choose your player name:");
            string playerName = Console.ReadLine();
            Console.WriteLine("Connecting server...");
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId("Manager"),APP_NAME);
            bool registrationSuccess = actor.PlayerRegister(playerName).Result;
            if (registrationSuccess)
            {
                Console.WriteLine("Success");
                actor.SubscribeAsync<IGameLobbyEvents>(new LobbyEventsHandler());
                actor.UpdateLobbyInfo();
                Console.WriteLine("Waiting");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Not Success");
            }
        }
    }
}
