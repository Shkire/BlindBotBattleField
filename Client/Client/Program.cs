using ClientBasicClasses;
using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        //const string API_URI = "http://localhost:8603/api/";
        //const string API_URI = "http://blindbotbattlefield.westeurope.cloudapp.azure.com:8603/api/";

        static void Main(string[] args)
        {
            Console.WriteLine("Write server IP addres");
            string API_URI = Console.ReadLine();
            ClientGameManager gameManager = new ClientGameManager(API_URI);
            while (!gameManager.exit)
            {
                gameManager.Print();
                //Task.WaitAll(Task.Delay(500));
                var key = Console.ReadKey();
                gameManager.Manage(key);
                /*
                if (gameManager.state.Equals(ClientState.Start))
                {
                    Console.WriteLine("BlindBotBattlefield")
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
                else if (gameManager.state.Equals(ClientState.Game))
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
                */
            }
        }
    }
}
