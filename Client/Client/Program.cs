using Client.BasicClasses;
using System;

namespace Client
{
    class Program
    {
        const string APP_NAME = "fabric:/BlindBotBattleField";
        const string LOGIN_SERVICE = "/LoginService";

        static void Main(string[] args)
        {
            ClientGameManager gameManager = new ClientGameManager(APP_NAME, LOGIN_SERVICE);
            while (!gameManager.exit)
            {
                gameManager.Print();
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
