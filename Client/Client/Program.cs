using GameManagerActor.Interfaces;
using LoginService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Client.GameManager;
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
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Game Lobby\n");
            for (int i = 0; i < i_playerIdMap.Count; i++)
            {
                if (i_playerIdMap[i].Equals(p_gameManager.playerName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("->");
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\t"+i+1+". "+i_playerIdMap[i]);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nEsc: Exit Lobby");
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
        private ClientState p_state;
        private int[] p_playerPosReal;
        private int[] p_playerPosVirt;
        private CellContent[,] p_playerSight;
        private int p_playerSightRange = 5;
        private IGameManagerActor p_actor;
        private ILoginService p_service;
        private string p_playerName;
        private bool p_exit;
        private int p_pointer;
        private string p_appName;
        private string p_loginService;
        private List<GameInfo> p_games;
        private IGameLobbyEvents p_lobbyHandler;
        private IGameEvents p_gameHandler;

        public enum ClientState
        {
            Start,
            Start_HowTo,
            Login,
            Login_New,
            Login_Existing,
            GameSelection,
            Lobby,
            Game,
            Spectator,
            Results
        }

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

        public ClientState state
        {
            get
            {
                return p_state;
            }
        }

        public bool exit
        {
            get
            {
                return p_exit;
            }
        }

        public string playerName
        {
            get
            {
                return p_playerName;
            }
        }

        public GameManager(string i_appName, string i_loginService)
        {
            p_state = ClientState.Start;
            p_exit = false;
            p_pointer = 0;
            p_appName = i_appName;
            p_loginService = i_loginService;
        }

        public void Print()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = false;
            string playerName;
            string pass;
            switch (p_state)
            {
                case ClientState.Start:
                    Console.Clear();
                    Console.WriteLine("BlindBotBattleField\n");
                    if (p_pointer == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    Console.WriteLine("\tStart game");
                    if (p_pointer == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tHow to play");
                    if (p_pointer == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tExit");
                    break;
                case ClientState.Start_HowTo:
                    break;
                case ClientState.Login:
                    Console.Clear();
                    Console.WriteLine("Log in\n");
                    if (p_pointer == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    Console.WriteLine("\tNew Player");
                    if (p_pointer == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tExisting Player");
                    if (p_pointer == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tBack");
                    break;
                case ClientState.Login_New:
                    Console.Clear();
                    Console.WriteLine("Create new user\n");
                    Console.Write("Choose your player name: ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    playerName = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Choose your password: ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    pass = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Clear();
                    Console.WriteLine("Log in\n");
                    Console.WriteLine("Connecting...");
                    try
                    {
                        p_service = ServiceProxy.Create<ILoginService>(new Uri(p_appName + p_loginService));
                        if (PlayerRegister(playerName, pass))
                        {
                            Console.WriteLine("Success!");
                            Thread.Sleep(2000);
                            p_state = ClientState.GameSelection;
                            Print();
                        }
                        else
                        {
                            Console.WriteLine("ERROR! Player name already exists");
                            Thread.Sleep(2000);
                            p_state = ClientState.Login;
                            Print();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR! Server unavailable");
                        Thread.Sleep(2000);
                        p_state = ClientState.Login;
                        Print();
                    }
                    break;
                case ClientState.Login_Existing:
                    Console.Clear();
                    Console.WriteLine("Log in\n");
                    Console.Write("Player name: ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    playerName = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Password: ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    pass = Console.ReadLine();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Clear();
                    Console.WriteLine("Log in\n");
                    Console.WriteLine("Connecting...");
                    try
                    {
                        p_service = ServiceProxy.Create<ILoginService>(new Uri(p_appName + p_loginService));
                        if (PlayerLogIn(playerName, pass))
                        {
                            Console.WriteLine("Success!");
                            Thread.Sleep(2000);
                            p_state = ClientState.GameSelection;
                            Print();
                        }
                        else
                        {
                            Console.WriteLine("ERROR! Player name or password not correct");
                            Thread.Sleep(2000);
                            p_state = ClientState.Login;
                            Print();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ERROR! Server unavailable");
                        Thread.Sleep(2000);
                        p_state = ClientState.Login;
                        Print();
                    }
                    break;
                case ClientState.GameSelection:
                    if (p_games == null)
                        GetGameList();
                    Console.Clear();
                    Console.WriteLine("Select game to join\n");
                    Console.WriteLine("\tGame name\tPlayers\n");
                    for (int i = 0; i < p_games.Count; i++)
                    {
                        if (p_pointer == i)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("->");
                        }
                        else
                            Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\t"+p_games[i].id+"\t"+p_games[i].players+"/"+p_games[i].maxPlayers);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\nF1:Refresh   F2:Create new game   Esc:Exit");
                    break;
                case ClientState.Lobby:
                    break;
                case ClientState.Game:
                    RefreshClient();
                    break;
                case ClientState.Spectator:
                    break;
                case ClientState.Results:
                    break;
            }
        }

        public void Manage(ConsoleKeyInfo i_key)
        {
            switch (p_state)
            {
                case ClientState.Start:
                    switch (i_key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (p_pointer > 0)
                                p_pointer--;
                            break;
                        case ConsoleKey.DownArrow:
                            if (p_pointer < 2)
                                p_pointer++;
                            break;
                        case ConsoleKey.Enter:
                            switch (p_pointer)
                            {
                                case 0:
                                    p_state = ClientState.Login;
                                    p_pointer = 0;
                                    break;
                                case 1:
                                    break;
                                case 2:
                                    p_exit = true;
                                    break;
                            }
                            break;
                    }
                    break;
                case ClientState.Login:
                    switch (i_key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (p_pointer > 0)
                                p_pointer--;
                            break;
                        case ConsoleKey.DownArrow:
                            if (p_pointer < 2)
                                p_pointer++;
                            break;
                        case ConsoleKey.Enter:
                            switch (p_pointer)
                            {
                                case 0:
                                    p_state = ClientState.Login_New;
                                    p_pointer = 0;
                                    break;
                                case 1:
                                    p_state = ClientState.Login_Existing;
                                    p_pointer = 0;
                                    break;
                                case 2:
                                    p_state = ClientState.Start;
                                    p_pointer = 0;
                                    break;
                            }
                            break;
                    }
                    break;
                case ClientState.GameSelection:
                    switch (i_key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            p_pointer = (p_pointer > 0) ? p_pointer - 1 : 0;
                            break;
                        case ConsoleKey.DownArrow:
                            p_pointer = (p_pointer < p_games.Count - 1) ? p_pointer + 1 : p_games.Count - 1;
                            break;
                        case ConsoleKey.Enter:
                            if (p_games.Count > 0)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Clear();
                                Console.WriteLine("Join game {0}\n", p_games[p_pointer].id);
                                Console.WriteLine("Connecting...\n");
                                if (ConnectPlayerToGame(p_games[p_pointer].id))
                                {
                                    p_pointer = 0;
                                    p_state = ClientState.Lobby;
                                    SubscribeToLobbyEvents();
                                    UpdateLobby();
                                    p_games = null;
                                }
                                else
                                {
                                    p_pointer = 0;
                                    GetGameList();
                                }                                
                            }
                            break;
                        case ConsoleKey.F1:
                            GetGameList();
                            p_pointer = 0;
                            break;
                        case ConsoleKey.F2:
                            Console.Clear();
                            Console.WriteLine("Create new game\n");
                            Console.Write("Game name: ");
                            Console.ForegroundColor = ConsoleColor.Red;
                            string gameName = Console.ReadLine();
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Max players: ");
                            Console.ForegroundColor = ConsoleColor.Red;
                            //Change to arrow selector
                            int maxPlayers = Int32.Parse(Console.ReadLine());
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Clear();
                            Console.WriteLine("Create game {0}\n", gameName);
                            Console.WriteLine("Creating...");
                            p_pointer = 0;
                            try
                            {
                                p_service = ServiceProxy.Create<ILoginService>(new Uri(p_appName + p_loginService));
                                if (CreateGame(gameName,maxPlayers))
                                {
                                    Console.WriteLine("Success!");
                                    Thread.Sleep(2000);
                                    if (ConnectPlayerToGame(gameName))
                                    {
                                        p_state = ClientState.Lobby;
                                        SubscribeToLobbyEvents();
                                        UpdateLobby();
                                        p_games = null;
                                    }
                                    else
                                    {
                                        GetGameList();
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("ERROR! Game {0} already exists",gameName);
                                    Thread.Sleep(2000);
                                    GetGameList();
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("ERROR! Server unavailable");
                                Thread.Sleep(2000);
                            }
                            break;
                    }
                    break;
                case ClientState.Lobby:
                    switch (i_key.Key)
                    {
                        case ConsoleKey.Escape:
                            p_state = ClientState.GameSelection;
                            UnsubscribeToLobbyEvents();
                            break;
                    }
                    break;
                case ClientState.Game:
                    switch (i_key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            MovePlayer(new int[] { 0, 1 });
                            break;
                        case ConsoleKey.DownArrow:
                            MovePlayer(new int[] { 0, -1 });
                            break;
                        case ConsoleKey.LeftArrow:
                            MovePlayer(new int[] { -1, 0 });
                            break;
                        case ConsoleKey.RightArrow:
                            MovePlayer(new int[] { 1, 0 });
                            break;
                        case ConsoleKey.Enter:
                            RadarUsed();
                            break;
                        case ConsoleKey.Spacebar:
                            PlayerAttacks();
                            break;
                        case ConsoleKey.Escape:
                            p_state = ClientState.GameSelection;
                            UnsubscribeToGameEvents();
                            break;
                    }
                    break;
                case ClientState.Spectator:
                    break;
                case ClientState.Results:
                    break;
            }
        }

        public bool PlayerLogIn(string i_player, string i_pass)
        {
            bool res = p_service.Login(i_player, i_pass).Result;
            if (res)
            {
                p_playerName = i_player; 
            }
            return res;
        }

        public bool PlayerRegister(string i_player, string i_pass)
        {
            bool res = p_service.CreatePlayer(i_player, i_pass).Result;
            if (res)
            {
                p_playerName = i_player;
            }
            return res;
        }

        public void GetGameList()
        {
            p_games = p_service.GetGameList().Result;
        }

        public bool CreateGame(string i_gameName,int i_maxPlayers)
        {
            return p_service.CreateGameAsync(new GameInfo(i_gameName,i_maxPlayers)).Result;
        }

        public bool ConnectPlayerToGame(string i_gameId)
        {
            p_actor = ActorProxy.Create<IGameManagerActor>(new ActorId(i_gameId), p_appName);
            int res = p_actor.ConnectPlayerAsync(p_playerName).Result;
            switch (res)
            {
                case 0:
                    return true;
                    break;
                case 1:
                    Console.WriteLine("ERROR! Player limit reached for this game");
                    break;
                case2:
                    Console.WriteLine("ERROR! Game deleted");
                    break;
            }
            return false;
        }

        public void PlayerStillConnected()
        {
            p_actor.PlayerStillConnectedAsync(p_playerName);
        }

        public void SubscribeToLobbyEvents()
        {
            p_lobbyHandler = new LobbyEventsHandler(this);
            p_actor.SubscribeAsync<IGameLobbyEvents>(p_lobbyHandler);
        }

        public void SubscribeToGameEvents()
        {
            p_gameHandler = new GameEventsHandler(this);
            p_actor.SubscribeAsync<IGameEvents>(p_gameHandler);
        }

        public void UnsubscribeToLobbyEvents()
        {
            p_actor.UnsubscribeAsync<IGameLobbyEvents>(p_lobbyHandler);
        }

        public void UnsubscribeToGameEvents()
        {
            p_actor.UnsubscribeAsync<IGameEvents>(p_gameHandler);
        }

        public void UpdateLobby()
        {
            p_actor.UpdateLobbyInfoAsync();
        }

        public void StartGame()
        {
            p_state = ClientState.Game;
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
            UnsubscribeToLobbyEvents();
            SubscribeToGameEvents();
            RefreshClient();
        }

        public void MovePlayer(int[] i_dir)
        {
            Task.WaitAll(p_actor.PlayerMovesAsync(i_dir, p_playerName));

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
            //RefreshClient();
            Console.Beep();
        }

        public void PlayerAttacks()
        {
            Task.WaitAll(p_actor.PlayerAttacksAsync(p_playerName));
            Console.Beep();
        }

        public void RadarUsed()
        {
            CellContent[][] res = p_actor.RadarActivatedAsync(p_playerName).Result;
            for (int i = 0; i < p_playerSight.GetLength(0); i++)
            {
                for (int j = 0; j < p_playerSight.GetLength(1); j++)
                {
                    p_playerSight[i, j] = res[i][j];
                }
            }
            //RefreshClient();
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
            Console.WriteLine("Playing\n");
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
            Console.WriteLine("\nArrows: Move bot    Space: Drop bomb    Enter: Radar    Esc: Exit");
        }
    }

    class Program
    {
        const string APP_NAME = "fabric:/BlindBotBattleField";
        const string LOGIN_SERVICE = "/LoginService";

        static void Main(string[] args)
        {
            GameManager gameManager = new GameManager(APP_NAME, LOGIN_SERVICE);
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
