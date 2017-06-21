using Client.EventHandlers;
using GameManagerActor.Interfaces;
using GameManagerActor.Interfaces.BasicClasses;
using GameManagerActor.Interfaces.EventHandlers;
using LoginService.Interfaces;
using LoginService.Interfaces.BasicClasses;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using ServerResponse;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Client.BasicClasses
{
    public enum ClientState
    {
        Start,
        Start_HowTo,
        Login,
        Login_New,
        Login_Existing,
        GameSelection,
        GameSelection_Create,
        Lobby,
        Game,
        Spectator,
        Results
    }

    class ClientCellInfo
    {
        public DateTime time;
        public CellContent content;

        public ClientCellInfo()
        {
            time = DateTime.MinValue;
            content = CellContent.None;
        }
    }

    class ClientGameManager
    {
        #region VARIABLES

        private ClientState p_state;
        private int[] p_playerPosReal;
        private int[] p_playerPosVirt;
        private ClientCellInfo[,] p_playerSight;
        private int p_playerSightRange = 5;
        private IGameManagerActor p_actor;
        private ILoginService p_service;
        private string p_playerName;
        private bool p_exit;
        private int p_pointer;
        private string p_appName;
        private string p_loginService;
        private List<GameDefinition> p_games;
        private IGameLobbyEvents p_lobbyHandler;
        private IGameEvents p_gameHandler;
        private List<string> p_gameLog;
        private int p_logPointer;
        private bool p_dead;
        private bool p_gameFinished;
        private List<string> p_storedStringData;

        #endregion

        #region GETTERS_AND_SETTERS

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

        #endregion

        public ClientGameManager(string i_appName, string i_loginService)
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
                    #region PRINT_START
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
                    #endregion
                case ClientState.Start_HowTo:
                    #region PRINT_STARTHOWTO
                    break;
                    #endregion
                case ClientState.Login:
                    #region PRINT_LOGIN
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
                    #endregion
                case ClientState.Login_New:
                    #region PRINT_LOGIN_NEW
                    Console.Clear();
                    Console.WriteLine("Create new user\n");
                    if (p_pointer == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    Console.WriteLine("\tPlayer name: "+p_storedStringData[0]);
                    if (p_pointer == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tPassword: "+p_storedStringData[1]);
                    if (p_pointer == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tAccept");
                    if (p_pointer == 3)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tBack");
                    break;
                    #endregion
                case ClientState.Login_Existing:
                    #region PRINT_LOGIN_EXISTING
                    Console.Clear();
                    Console.WriteLine("Log In\n");
                    if (p_pointer == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    Console.WriteLine("\tPlayer name: " + p_storedStringData[0]);
                    if (p_pointer == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tPassword: " + p_storedStringData[1]);
                    if (p_pointer == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tAccept");
                    if (p_pointer == 3)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tBack");
                    break;
                    #endregion
                case ClientState.GameSelection:
                    #region PRINT_GAMESELECTION
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
                        Console.WriteLine("\t" + p_games[i].id + "\t" + p_games[i].players + "/" + p_games[i].maxPlayers);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\nF1:Refresh   F2:Create new game   Esc:Exit");
                    break;
                #endregion
                case ClientState.GameSelection_Create:
                    #region PRINT_GAMESELECTION_CREATE
                    Console.Clear();
                    Console.WriteLine("Create new game\n");
                    if (p_pointer == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    Console.WriteLine("\tGame name: " + p_storedStringData[0]);
                    if (p_pointer == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tMax players(2-8): " + p_storedStringData[1]);
                    if (p_pointer == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tAccept");
                    if (p_pointer == 3)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("->");
                    }
                    else
                        Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\tBack");
                    break;
                #endregion
                case ClientState.Lobby:
                    #region PRINT_LOBBY
                    break;
                    #endregion
                case ClientState.Game:
                    #region PRINT_GAME
                    RefreshClient();
                    break;
                    #endregion
                case ClientState.Spectator:
                    #region PRINT_SPECTATOR
                    break;
                    #endregion
                case ClientState.Results:
                    #region PRINT_RESULTS
                    break;
                    #endregion
            }
        }

        public void Manage(ConsoleKeyInfo i_key)
        {
            switch (p_state)
            {
                case ClientState.Start:
                    #region MANAGE_START
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
                #endregion
                case ClientState.Start_HowTo:
                    #region MANAGE_START_HOWTO
                    break;
                    #endregion
                case ClientState.Login:
                    #region MANAGE_LOGIN
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
                                    p_storedStringData = new List<string>(new string[] {string.Empty, string.Empty});
                                    break;
                                case 1:
                                    p_state = ClientState.Login_Existing;
                                    p_pointer = 0;
                                    p_storedStringData = new List<string>(new string[] { string.Empty, string.Empty });
                                    break;
                                case 2:
                                    p_state = ClientState.Start;
                                    p_pointer = 0;
                                    break;
                            }
                            break;
                    }
                    break;
                #endregion
                case ClientState.Login_New:
                    #region MANAGE_LOGIN_NEW
                    switch (i_key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (p_pointer > 0)
                                p_pointer--;
                            break;
                        case ConsoleKey.DownArrow:
                            if (p_pointer < 3)
                                p_pointer++;
                            break;
                        case ConsoleKey.Enter:
                            switch (p_pointer)
                            {
                                case 0:
                                    Console.Clear();
                                    Console.WriteLine("Create new user\n");
                                    Console.CursorTop = 3;
                                    Console.WriteLine("\tPassword: " + p_storedStringData[1]);
                                    Console.WriteLine("\tAccept");
                                    Console.WriteLine("\tBack");
                                    Console.CursorTop = 2;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Write("->\tPlayer name: ");
                                    Console.CursorVisible = true;
                                    p_storedStringData[0] = Console.ReadLine();
                                    Console.CursorVisible = false;
                                    Console.CursorTop = 0;
                                    break;
                                case 1:
                                    Console.Clear();
                                    Console.WriteLine("Create new user\n");
                                    Console.WriteLine("\tPlayer name: " + p_storedStringData[0]);
                                    Console.CursorTop = 4;
                                    Console.WriteLine("\tAccept");
                                    Console.WriteLine("\tBack");
                                    Console.CursorTop = 3;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Write("->\tPassword: ");
                                    Console.CursorVisible = true;
                                    p_storedStringData[1] = Console.ReadLine();
                                    Console.CursorVisible = false;
                                    Console.CursorTop = 0;
                                    break;
                                case 2:
                                    Console.ForegroundColor = ConsoleColor.White;
                                    if (!p_storedStringData[0].Equals(string.Empty) && !p_storedStringData[1].Equals(string.Empty) && !p_storedStringData[0].Equals(p_storedStringData[1]))
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Log in\n");
                                        Console.WriteLine("Connecting...");
                                        ServerResponseInfo<bool, SqlException> res = new ServerResponseInfo<bool, SqlException>();
                                        res.info = false;
                                        Exception exc = null;
                                        try
                                        {
                                            p_service = ServiceProxy.Create<ILoginService>(new Uri(p_appName + p_loginService));
                                            res = p_service.CreatePlayer(p_storedStringData[0], p_storedStringData[1]).Result;
                                        }
                                        catch (Exception e)
                                        {
                                            exc = e;
                                        }
                                        if (res.info)
                                        {
                                            Console.WriteLine("Success!");
                                            p_playerName = p_storedStringData[0];
                                            p_storedStringData = new List<string>(new string[] { string.Empty, string.Empty });
                                            p_pointer = 0;
                                            p_state = ClientState.GameSelection;
                                        }
                                        else
                                        {
                                            if (res.exception != null)
                                                Console.WriteLine("ERROR: " + res.exception);
                                            if (exc != null)
                                                Console.WriteLine("ERROR: " + exc);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("ERROR: Incorrect format");
                                    }
                                    Thread.Sleep(2000);
                                    break;
                                case 3:
                                    p_state = ClientState.Login;
                                    p_pointer = 0;
                                    break;
                            }
                            break;
                    }
                    break;
                #endregion
                case ClientState.Login_Existing:
                    #region MANAGE_LOGIN_EXISTING
                    switch (i_key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (p_pointer > 0)
                                p_pointer--;
                            break;
                        case ConsoleKey.DownArrow:
                            if (p_pointer < 3)
                                p_pointer++;
                            break;
                        case ConsoleKey.Enter:
                            switch (p_pointer)
                            {
                                case 0:
                                    Console.Clear();
                                    Console.WriteLine("Log In\n");
                                    Console.CursorTop = 3;
                                    Console.WriteLine("\tPassword: " + p_storedStringData[1]);
                                    Console.WriteLine("\tAccept");
                                    Console.WriteLine("\tBack");
                                    Console.CursorTop = 2;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Write("->\tPlayer name: ");
                                    Console.CursorVisible = true;
                                    p_storedStringData[0] = Console.ReadLine();
                                    Console.CursorVisible = false;
                                    Console.CursorTop = 0;
                                    break;
                                case 1:
                                    Console.Clear();
                                    Console.WriteLine("Log In\n");
                                    Console.WriteLine("\tPlayer name: " + p_storedStringData[0]);
                                    Console.CursorTop = 4;
                                    Console.WriteLine("\tAccept");
                                    Console.WriteLine("\tBack");
                                    Console.CursorTop = 3;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Write("->\tPassword: ");
                                    Console.CursorVisible = true;
                                    p_storedStringData[1] = Console.ReadLine();
                                    Console.CursorVisible = false;
                                    Console.CursorTop = 0;
                                    break;
                                case 2:
                                    Console.ForegroundColor = ConsoleColor.White;
                                    if (!p_storedStringData[0].Equals(string.Empty) && !p_storedStringData[1].Equals(string.Empty))
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Log in\n");
                                        Console.WriteLine("Connecting...");
                                        ServerResponseInfo<bool, SqlException> res = new ServerResponseInfo<bool, SqlException>();
                                        res.info = false;
                                        Exception exc = null;
                                        try
                                        {
                                            p_service = ServiceProxy.Create<ILoginService>(new Uri(p_appName + p_loginService));
                                            res = p_service.Login(p_storedStringData[0], p_storedStringData[1]).Result;
                                        }
                                        catch (Exception e)
                                        {
                                            exc = e;
                                        }
                                        if (res.info)
                                        {
                                            Console.WriteLine("Success!");
                                            p_playerName = p_storedStringData[0];
                                            p_storedStringData = new List<string>(new string[] { string.Empty, string.Empty });
                                            p_pointer = 0;
                                            p_state = ClientState.GameSelection;
                                        }
                                        else
                                        {
                                            if (res.exception != null)
                                                Console.WriteLine("ERROR: " + res.exception);
                                            if (exc != null)
                                                Console.WriteLine("ERROR: " + exc);
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("ERROR: Incorrect format");
                                    }
                                    Thread.Sleep(2000);
                                    break;
                                case 3:
                                    p_state = ClientState.Login;
                                    p_pointer = 0;
                                    break;
                            }
                            break;
                    }
                    break;
                    #endregion
                case ClientState.GameSelection:
                    #region MANAGE_GAMESELECTION
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
                                ServerResponseInfo<bool, Exception> res = new ServerResponseInfo<bool, Exception>();
                                res.info = false;
                                Exception exc = null;
                                Console.WriteLine("Join game {0}\n", p_games[p_pointer].id);
                                Console.WriteLine("Connecting...\n");
                                try
                                {
                                    p_actor = ActorProxy.Create<IGameManagerActor>(new ActorId(p_games[p_pointer].id), p_appName);
                                    res = p_actor.ConnectPlayerAsync(p_playerName).Result;
                                }
                                catch (Exception e)
                                {
                                    exc = e;
                                }
                                if (res.info)
                                {
                                    p_state = ClientState.Lobby;
                                    SubscribeToLobbyEvents();
                                    UpdateLobby();
                                    p_games = null;
                                }
                                else
                                {
                                    if (res.exception != null)
                                        Console.WriteLine("ERROR: " + res.exception);
                                    if (exc != null)
                                        Console.WriteLine("ERROR: " + exc);
                                    Thread.Sleep(2000);
                                }
                            }
                            break;
                        case ConsoleKey.F1:
                            GetGameList();
                            p_pointer = 0;
                            break;
                        case ConsoleKey.F2:
                            p_state = ClientState.GameSelection_Create;
                            p_pointer = 0;
                            break;
                        case ConsoleKey.Escape:
                            p_state = ClientState.Start;
                            p_pointer = 0;
                            p_games = null;
                            break;
                    }
                    break;
                    #endregion
                case ClientState.GameSelection_Create:
                    #region MANAGE_GAMESELECTION_CREATE
                    switch (i_key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (p_pointer > 0)
                                p_pointer--;
                            break;
                        case ConsoleKey.DownArrow:
                            if (p_pointer < 3)
                                p_pointer++;
                            break;
                        case ConsoleKey.Enter:
                            switch (p_pointer)
                            {
                                case 0:
                                    Console.Clear();
                                    Console.WriteLine("Create new game\n");
                                    Console.CursorTop = 3;
                                    Console.WriteLine("\tMax players(2-8): " + p_storedStringData[1]);
                                    Console.WriteLine("\tAccept");
                                    Console.WriteLine("\tBack");
                                    Console.CursorTop = 2;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Write("->\tGame name: ");
                                    Console.CursorVisible = true;
                                    p_storedStringData[0] = Console.ReadLine();
                                    Console.CursorVisible = false;
                                    Console.CursorTop = 0;
                                    break;
                                case 1:
                                    Console.Clear();
                                    Console.WriteLine("Create new game\n");
                                    Console.WriteLine("\tGame name: " + p_storedStringData[0]);
                                    Console.CursorTop = 4;
                                    Console.WriteLine("\tAccept");
                                    Console.WriteLine("\tBack");
                                    Console.CursorTop = 3;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.Write("->\tMax players(2-8): ");
                                    Console.CursorVisible = true;
                                    p_storedStringData[1] = Console.ReadLine();
                                    Console.CursorVisible = false;
                                    Console.CursorTop = 0;
                                    break;
                                case 2:
                                    Console.ForegroundColor = ConsoleColor.White;
                                    int max = 0;
                                    Exception exc = null;
                                    try
                                    {
                                        max = Int32.Parse(p_storedStringData[1]);
                                    }
                                    catch (Exception e)
                                    {
                                        exc = e;
                                    }
                                    if (exc==null && !p_storedStringData[0].Equals(string.Empty) && max>=2 && max<=8)
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Create game\n");
                                        Console.WriteLine("Connecting...");
                                        ServerResponseInfo<bool, Exception> res = new ServerResponseInfo<bool, Exception>();
                                        res.info = false;
                                        exc = null;
                                        try
                                        {
                                            p_service = ServiceProxy.Create<ILoginService>(new Uri(p_appName + p_loginService));
                                            res = p_service.CreateGameAsync(p_storedStringData[0], Int32.Parse(p_storedStringData[1])).Result;
                                        }
                                        catch (Exception e)
                                        {
                                            exc = e;
                                        }
                                        if (res.info)
                                        {
                                            Console.WriteLine("Success!");
                                            Thread.Sleep(2000);
                                            Console.Clear();
                                            Console.WriteLine("Join game {0}\n", p_storedStringData[0]);
                                            Console.WriteLine("Connecting...\n");
                                            try
                                            {
                                                p_actor = ActorProxy.Create<IGameManagerActor>(new ActorId(p_storedStringData[0]), p_appName);
                                                res = p_actor.ConnectPlayerAsync(p_playerName).Result;
                                            }
                                            catch (Exception e)
                                            {
                                                exc = e;
                                            }
                                            if (res.info)
                                            {
                                                p_state = ClientState.Lobby;
                                                SubscribeToLobbyEvents();
                                                UpdateLobby();
                                                p_games = null;
                                            }
                                            else
                                            {
                                                if (res.exception != null)
                                                    Console.WriteLine("ERROR: " + res.exception);
                                                if (exc != null)
                                                    Console.WriteLine("ERROR: " + exc);
                                                Thread.Sleep(2000);
                                            }
                                        }
                                        else
                                        {
                                            if (res.exception != null)
                                                Console.WriteLine("ERROR: " + res.exception);
                                            if (exc != null)
                                                Console.WriteLine("ERROR: " + exc);
                                            Thread.Sleep(2000);
                                        }
                                        p_storedStringData = new List<string>(new string[] { string.Empty, string.Empty });
                                    }
                                    else
                                    {
                                        Console.WriteLine("ERROR: Incorrect format");
                                        Thread.Sleep(2000);
                                    }
                                    break;
                                case 3:
                                    p_state = ClientState.GameSelection;
                                    p_games = null;
                                    p_pointer = 0;
                                    break;
                            }
                            break;
                    }
                    break;
                    #endregion
                case ClientState.Lobby:
                    #region MANAGE_LOBBY
                    switch (i_key.Key)
                    {
                        case ConsoleKey.Escape:
                            p_state = ClientState.GameSelection;
                            UnsubscribeToLobbyEvents();
                            break;
                    }
                    break;
                    #endregion
                case ClientState.Game:
                    #region MANAGE_GAME
                    switch (i_key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            if (!p_dead)
                                MovePlayer(new int[] { 0, 1 });
                            break;
                        case ConsoleKey.DownArrow:
                            if (!p_dead)
                                MovePlayer(new int[] { 0, -1 });
                            break;
                        case ConsoleKey.LeftArrow:
                            if (!p_dead)
                                MovePlayer(new int[] { -1, 0 });
                            break;
                        case ConsoleKey.RightArrow:
                            if (!p_dead)
                                MovePlayer(new int[] { 1, 0 });
                            break;
                        case ConsoleKey.Enter:
                            if (!p_dead)
                                RadarUsed();
                            break;
                        case ConsoleKey.Spacebar:
                            if (!p_dead)
                                PlayerAttacks();
                            break;
                        case ConsoleKey.Escape:
                            p_state = ClientState.GameSelection;
                            UnsubscribeToGameEvents();
                            break;
                        case ConsoleKey.Q:
                            GameLogUp();
                            break;
                        case ConsoleKey.A:
                            GameLogDown();
                            break;
                    }
                    break;
                    #endregion
                case ClientState.Spectator:
                    #region MANAGE_SPECTATOR
                    break;
                    #endregion
                case ClientState.Results:
                    #region MANAGE_RESULTS
                    break;
                    #endregion
            }
        }

        public void GetGameList()
        {
            ServerResponseInfo<bool, SqlException, List<GameDefinition>> response = new ServerResponseInfo<bool, SqlException, List<GameDefinition>>();
            response.info = false;
            Exception exc = null;
            try
            {
                response = p_service.GetGameList().Result;
            }
            catch (Exception e)
            {
                exc = e;
            }
            if (response.info)
            {
                p_games = response.additionalInfo;
            }
            else
            {
                if (response.exception != null)
                    Console.WriteLine("ERROR: " + response.exception);
                if (exc != null)
                    Console.WriteLine("ERROR: " + exc);
                p_state = ClientState.Login;
                Thread.Sleep(2000);
            }
        }

        public ServerResponseInfo<bool,Exception> CreateGame(string i_gameName, int i_maxPlayers)
        {
            return p_service.CreateGameAsync(i_gameName, i_maxPlayers).Result;
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

        public void StartGame(Dictionary<string,int[]> i_playerPositions)
        {
            p_state = ClientState.Game;
            p_logPointer = 0;
            p_gameLog = new List<string>();
            p_playerPosReal = i_playerPositions[playerName];
            p_dead = false;
            p_gameFinished = false;
            p_playerPosVirt = new int[] { (int)Math.Truncate(p_playerSightRange / 2f), (int)Math.Truncate(p_playerSightRange / 2f) };
            p_playerSight = new ClientCellInfo[p_playerSightRange, p_playerSightRange];
            for (int i = 0; i < p_playerSightRange; i++)
                for (int j = 0; j < p_playerSightRange; j++)
                    p_playerSight[i, j] = new ClientCellInfo();
            UnsubscribeToLobbyEvents();
            SubscribeToGameEvents();
            RefreshClient();
        }

        public void FinishGame(string i_winner)
        {
            p_gameLog.Insert(0, "Player " + i_winner + " has won the game");
            RefreshClient();
            p_gameFinished = true;
            Thread.Sleep(2000);
            p_state = ClientState.GameSelection;
            Print();
        }

        public void MovePlayer(int[] i_dir)
        {
            ServerResponseInfo<bool, Exception> response = new ServerResponseInfo<bool, Exception>();
            response.info = false;
            try
            {
                response = p_actor.PlayerMovesAsync(i_dir, p_playerName).Result;
            }
            catch (Exception e)
            {
                response.exception = e;
            }
            if (response.info)
            {
                p_playerSight[p_playerPosVirt[0], p_playerPosVirt[1]].content = CellContent.Floor;
                DateTime time = DateTime.Now;
                p_playerSight[p_playerPosVirt[0], p_playerPosVirt[1]].time = time;
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
                        p_playerSight[i, j] = (destX < 0 || destX > p_playerSight.GetLength(0) - 1 || destY < 0 || destY > p_playerSight.GetLength(1) - 1) ? new ClientCellInfo() : p_playerSight[destX, destY];
                    }
                }
                Console.Beep();
                RemoveMapInfoAsync(time,30000);
            }
            else
            {
                if (response.exception != null)
                    Console.WriteLine("ERROR: " + response.exception);
                p_state = ClientState.GameSelection;
                Thread.Sleep(2000);
            }
        }

        public void PlayerAttacks()
        {
            ServerResponseInfo<bool, Exception> response = new ServerResponseInfo<bool, Exception>();
            response.info = false;
            try
            {
                response = p_actor.PlayerAttacksAsync(p_playerName).Result;
            }
            catch (Exception e)
            {
                response.exception = e;
            }
            if (response.info)
            {
                Console.Beep();
            }
            else
            {
                if (response.exception != null)
                    Console.WriteLine("ERROR: " + response.exception);
                p_state = ClientState.GameSelection;
                Thread.Sleep(2000);
            }
        }

        public void RadarUsed()
        {
            ServerResponseInfo<bool, Exception,CellContent[][]> response = new ServerResponseInfo<bool, Exception,CellContent[][]>();
            response.info = false;
            try
            {
                response = p_actor.RadarActivatedAsync(p_playerName).Result;
            }
            catch (Exception e)
            {
                response.exception = e;
            }
            if (response.info)
            {
                DateTime time = DateTime.Now;
                for (int i = 0; i < p_playerSight.GetLength(0); i++)
                {
                    for (int j = 0; j < p_playerSight.GetLength(1); j++)
                    {
                        p_playerSight[i, j].content = response.additionalInfo[i][j];
                        p_playerSight[i, j].time = time;
                    }
                }
                Console.Beep();
                RemoveMapInfoAsync(time,3000);
            }
            else
            {
                if (response.exception != null)
                    Console.WriteLine("ERROR: " + response.exception);
                p_state = ClientState.GameSelection;
                Thread.Sleep(2000);
            }
        }

        public void PlayerDeadRecieved(int[] i_deadPos)
        {
            int[] deadPosVirt = new int[] { i_deadPos[0] - p_playerPosReal[0] + p_playerPosVirt[0], i_deadPos[1] - p_playerPosReal[1] + p_playerPosVirt[1] };
            if (!(deadPosVirt[0] < 0 || deadPosVirt[0] >= p_playerSight.GetLength(0) || deadPosVirt[1] < 0 || deadPosVirt[1] >= p_playerSight.GetLength(1)))
            {
                p_playerSight[deadPosVirt[0], deadPosVirt[1]].content = CellContent.Dead;
                DateTime time = DateTime.Now;
                p_playerSight[deadPosVirt[0], deadPosVirt[1]].time = time;
                RemoveMapInfoAsync(time,3000);
            }
        }

        public void PlayerDeadRecieved(int[] i_deadPos, string i_player,DeathReason i_reason)
        {
            PlayerDeadRecieved(i_deadPos);
            if (i_player.Equals(playerName))
                p_dead = true;
            switch (i_reason)
            {
                case DeathReason.Disconnect:
                    p_gameLog.Insert(0, "Player " + i_player + " exploded suddenly");
                    break;
                case DeathReason.Hole:
                    p_gameLog.Insert(0, "Player " + i_player + " fell into a hole");
                    break;
                case DeathReason.Turret:
                    p_gameLog.Insert(0, "Player " + i_player + " was destroyed by a turret");
                    break;
            }
            RefreshClient();
        }

        public void PlayerDeadRecieved(int[] i_deadPos, string i_player, string i_killer, DeathReason i_reason)
        {
            PlayerDeadRecieved(i_deadPos);
            if (i_player.Equals(playerName))
                p_dead = true;
            switch (i_reason)
            {
                case DeathReason.PlayerHit:
                    p_gameLog.Insert(0, "Player " + i_player + " was killed by a bomb thrown by " + i_killer);
                    break;
                case DeathReason.PlayerSmash:
                    p_gameLog.Insert(0, "Player " + i_player + " was smashed by " + i_killer);
                    break;
            }
            RefreshClient();
        }

        public void BombHitsRecieved(List<int[]> i_hitPos)
        {
            DateTime time = DateTime.Now;
            foreach (int[] hit in i_hitPos)
            {
                int[] hitPosVirt = new int[] { hit[0] - p_playerPosReal[0] + p_playerPosVirt[0], hit[1] - p_playerPosReal[1] + p_playerPosVirt[1] };
                if (!(hitPosVirt[0] < 0 || hitPosVirt[0] >= p_playerSight.GetLength(0) || hitPosVirt[1] < 0 || hitPosVirt[1] >= p_playerSight.GetLength(1)))
                {
                    p_playerSight[hitPosVirt[0], hitPosVirt[1]].content = CellContent.Hit;
                    p_playerSight[hitPosVirt[0], hitPosVirt[1]].time = time;
                }
            }
            RefreshClient();
            RemoveMapInfoAsync(time,3000);
        }

        public void PlayerDetected(int[] i_playerPos)
        {
            int[] playerPosVirt = new int[] { i_playerPos[0] - p_playerPosReal[0] + p_playerPosVirt[0], i_playerPos[1] - p_playerPosReal[1] + p_playerPosVirt[1] };
            if (!(playerPosVirt[0] < 0 || playerPosVirt[0] >= p_playerSight.GetLength(0) || playerPosVirt[1] < 0 || playerPosVirt[1] >= p_playerSight.GetLength(1)))
            {
                p_playerSight[playerPosVirt[0], playerPosVirt[1]].content = CellContent.Player;
                DateTime time = DateTime.Now;
                p_playerSight[playerPosVirt[0], playerPosVirt[1]].time = time;
                RemoveMapInfoAsync(time,3000);
            }
            RefreshClient();
        }

        public void TurretAiming(int[] i_aimPos)
        {
            int[] playerPosVirt = new int[] { i_aimPos[0] - p_playerPosReal[0] + p_playerPosVirt[0], i_aimPos[1] - p_playerPosReal[1] + p_playerPosVirt[1] };
            if (!(playerPosVirt[0] < 0 || playerPosVirt[0] >= p_playerSight.GetLength(0) || playerPosVirt[1] < 0 || playerPosVirt[1] >= p_playerSight.GetLength(1)))
            {
                p_playerSight[playerPosVirt[0], playerPosVirt[1]].content = CellContent.Aiming;
                DateTime time = DateTime.Now;
                p_playerSight[playerPosVirt[0], playerPosVirt[1]].time = time;
                RemoveMapInfoAsync(time,500);
            }
            Console.Beep();
            RefreshClient();
        }

        public void GameLogUp()
        {
            p_logPointer = p_logPointer > 0 ? p_logPointer - 1 : 0;
        }

        public void GameLogDown()
        {
            p_logPointer = p_logPointer < p_gameLog.Count - 1 ? p_logPointer + 1 : p_gameLog.Count - 1;
        }

        public void RefreshClient()
        {
            Console.Clear();
            Console.WriteLine("Playing\n");
            if (p_dead)
                Console.WriteLine("--YOU'RE DEAD--");
            else
                Console.WriteLine("--{0}'s Bot--", playerName);
            for (int j = p_playerSight.GetLength(1) - 1; j >= 0; j--)
            {
                string res = string.Empty;
                for (int i = 0; i < p_playerSight.GetLength(0); i++)
                {
                    if (i == p_playerPosVirt[0] && j == p_playerPosVirt[1] && !p_playerSight[p_playerPosVirt[0], p_playerPosVirt[1]].Equals(CellContent.Aiming))
                    {
                        res += "P ";
                    }
                    else
                    {
                        switch (p_playerSight[i, j].content)
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
                            case CellContent.Aiming:
                                res += "T ";
                                break;
                        }
                    }
                }
                Console.WriteLine(res);
            }
            Console.WriteLine("\nArrows: Move bot    Space: Drop bomb    Enter: Radar    Esc: Exit");
            Console.WriteLine("\nGame Log:");
            Console.WriteLine("\n------------------------------------------------------------------\n");
            for (int i = 0; i <= 5; i++)
            {
                string info = (p_gameLog != null && p_gameLog.Count > p_logPointer + i) ? p_gameLog[p_logPointer + i] : string.Empty;
                Console.WriteLine(info);
            }
            Console.WriteLine("\n------------------------------------------------------------------");
        }

        public async Task RemoveMapInfoAsync(DateTime i_time, int i_millis)
        {
            await Task.Delay(i_millis);
            foreach (ClientCellInfo info in p_playerSight)
            {
                if (info.time.Equals(i_time))
                {
                    info.time = DateTime.MinValue;
                    info.content = CellContent.None;
                }
            }
            RefreshClient();
        }
    }
}
