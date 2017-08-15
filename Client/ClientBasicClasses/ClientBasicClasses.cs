using BasicClasses.Common;
using BasicClasses.GameManager;
using BasicClasses.LoginService;
using ClientBasicClasses.Sockets;
using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ClientBasicClasses
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

    public class ClientGameManager
    {
        #region VARIABLES

        const int COOLDOWN_RADAR = 30;
        const int COOLDOWN_ATTACK = 10;
        private ClientState p_state;
        private GameTutorial p_tutorial;
        private int[] p_playerPosReal;
        private int[] p_playerPosVirt;
        private ClientCellInfo[,] p_playerSight;
        private int p_playerSightRange = 5;
        private string p_playerName;
        private bool p_exit;
        private int p_pointer;
        private HttpClient p_client;
        private string p_gameId;
        private List<GameDefinition> p_games;
        private List<string> p_gameLog;
        private int p_logPointer;
        private bool p_dead;
        private bool p_gameFinished;
        private List<string> p_storedStringData;
        private int radarTime;
        private int attackTime;
        public string ipAdress;
        public List<DateTime> p_consoleWriteLock;
        private Thread p_lobbyThread;
        private Thread p_gameSessionThread;

        #endregion

        #region GETTERS_AND_SETTERS

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

        public ClientGameManager(string i_apiUri)
        {
            p_consoleWriteLock = new List<DateTime>();
            p_state = ClientState.Start;
            p_exit = false;
            p_pointer = 0;
            p_client = new HttpClient();
            p_client.BaseAddress = new Uri(i_apiUri);
            p_client.DefaultRequestHeaders.Accept.Clear();
            p_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Print()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.CursorVisible = false;
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
                                    p_state = ClientState.Start_HowTo;
                                    p_tutorial = new GameTutorial(this);
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
                    p_tutorial.Manage(i_key);
                    //DELETE THIS LINE MAYBE
                    if (p_tutorial != null)
                        p_tutorial.Print();
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
                                    p_storedStringData = new List<string>(new string[] { string.Empty, string.Empty });
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
                                            res = CreatePlayer(p_storedStringData[0], p_storedStringData[1]);
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
                                            res = Login(p_storedStringData[0], p_storedStringData[1]);
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
                                    res = ConnectPlayerAsync(p_games[p_pointer].id, p_playerName);
                                }
                                catch (Exception e)
                                {
                                    exc = e;
                                }
                                if (res.info)
                                {
                                    p_gameId = p_games[p_pointer].id;
                                    p_state = ClientState.Lobby;
                                    StartListeningLobbyEvents();
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
                                    if (exc == null && !p_storedStringData[0].Equals(string.Empty) && max >= 2 && max <= 8)
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Create game\n");
                                        Console.WriteLine("Connecting...");
                                        ServerResponseInfo<bool, Exception> res = new ServerResponseInfo<bool, Exception>();
                                        res.info = false;
                                        exc = null;
                                        try
                                        {
                                            res = CreateGame(p_storedStringData[0], Int32.Parse(p_storedStringData[1]));
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
                                                res = ConnectPlayerAsync(p_storedStringData[0], p_playerName);
                                            }
                                            catch (Exception e)
                                            {
                                                exc = e;
                                            }
                                            if (res.info)
                                            {
                                                p_gameId = p_storedStringData[0];
                                                p_state = ClientState.Lobby;
                                                StartListeningLobbyEvents();
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
                            StopListeningLobbyEvents();
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
                            if (!p_dead && radarTime == 0)
                                RadarUsed();
                            break;
                        case ConsoleKey.Spacebar:
                            if (!p_dead && attackTime == 0)
                                PlayerAttacks();
                            break;
                        case ConsoleKey.Escape:
                            p_state = ClientState.GameSelection;
                            StopListeningGameEvents();
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
                string path = "login/games";
                HttpResponseMessage httpResponse = p_client.GetAsync(path).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    response = httpResponse.Content.ReadAsAsync<ServerResponseInfo<bool, SqlException, List<GameDefinition>>>().Result;
                }
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

        public ServerResponseInfo<bool, Exception> CreateGame(string i_gameName, int i_maxPlayers)
        {
            ServerResponseInfo<bool, Exception> res = new ServerResponseInfo<bool, Exception>();
            string path = "login/games/new";
            List<string> info = new List<string>();
            info.Add(i_gameName.SerializeObject());
            info.Add(i_maxPlayers.SerializeObject());
            HttpResponseMessage response = p_client.PostAsJsonAsync(path, info.SerializeObject()).Result;
            if (response.IsSuccessStatusCode)
            {
                res = response.Content.ReadAsAsync<ServerResponseInfo<bool, Exception>>().Result;
            }
            return res;
        }

        public void PlayerStillConnected()
        {
            string path = "gamemanager/still";
            List<string> info = new List<string>();
            info.Add(p_gameId);
            info.Add(playerName);
            Task.WaitAll(p_client.PostAsJsonAsync(path, info.SerializeObject()));
        }

        public void StartListeningLobbyEvents()
        {
            //p_lobbyHandler = new LobbyEventsHandler(this);
            //p_actor.SubscribeAsync<IGameLobbyEvents>(p_lobbyHandler);
            p_lobbyThread = new Thread(new ThreadStart(StartListeningLobby));
            p_lobbyThread.Start();
        }

        public void StartListeningGameEvents()
        {
            //p_gameHandler = new GameEventsHandler(this);
            //p_actor.SubscribeAsync<IGameEvents>(p_gameHandler);
            p_gameSessionThread = new Thread(new ThreadStart(StartListeningGameSession));
            p_gameSessionThread.Start();
        }

        public void StopListeningLobbyEvents()
        {
            //p_actor.UnsubscribeAsync<IGameLobbyEvents>(p_lobbyHandler);
            p_lobbyThread.Abort();
        }

        public void StopListeningGameEvents()
        {
            //p_actor.UnsubscribeAsync<IGameEvents>(p_gameHandler);
            p_gameSessionThread.Abort();
        }

        public void UpdateLobby()
        {
            string path = "gamemanager/lobby";
            Task.WaitAll(p_client.PostAsJsonAsync(path, p_gameId));
        }

        public void StartGame(Dictionary<string, int[]> i_playerPositions)
        {
            radarTime = 0;
            attackTime = 0;
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
            StopListeningLobbyEvents();
            StartListeningGameEvents();
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
                string path = "gamemanager/move";
                List<string> info = new List<string>();
                info.Add(p_gameId.SerializeObject());
                info.Add(i_dir.SerializeObject());
                info.Add(playerName.SerializeObject());
                HttpResponseMessage httpRes = p_client.PostAsJsonAsync(path, info.SerializeObject()).Result;
                if (httpRes.IsSuccessStatusCode)
                    response = httpRes.Content.ReadAsAsync<ServerResponseInfo<bool, Exception>>().Result;
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
                RemoveMapInfoAsync(time, 30000);
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
                string path = "gamemanager/attack";
                List<string> info = new List<string>();
                info.Add(p_gameId);
                info.Add(playerName);
                HttpResponseMessage httpRes = p_client.PostAsJsonAsync(path, info.SerializeObject()).Result;
                if (httpRes.IsSuccessStatusCode)
                    response = httpRes.Content.ReadAsAsync<ServerResponseInfo<bool, Exception>>().Result;
                AttackCooldown();
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
            ServerResponseInfo<bool, Exception, CellContent[][]> response = new ServerResponseInfo<bool, Exception, CellContent[][]>();
            response.info = false;
            try
            {
                string path = "gamemanager/radar";
                List<string> info = new List<string>();
                info.Add(p_gameId);
                info.Add(playerName);
                HttpResponseMessage httpRes = p_client.PostAsJsonAsync(path, info.SerializeObject()).Result;
                if (httpRes.IsSuccessStatusCode)
                    response = httpRes.Content.ReadAsAsync<ServerResponseInfo<bool, Exception, CellContent[][]>>().Result;
                RadarCooldown();
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
                RemoveMapInfoAsync(time, 3000);
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
                RemoveMapInfoAsync(time, 3000);
            }
        }

        public void PlayerDeadRecieved(int[] i_deadPos, string i_player, DeathReason i_reason)
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
            RemoveMapInfoAsync(time, 3000);
        }

        public void PlayerDetected(int[] i_playerPos)
        {
            int[] playerPosVirt = new int[] { i_playerPos[0] - p_playerPosReal[0] + p_playerPosVirt[0], i_playerPos[1] - p_playerPosReal[1] + p_playerPosVirt[1] };
            if (!(playerPosVirt[0] < 0 || playerPosVirt[0] >= p_playerSight.GetLength(0) || playerPosVirt[1] < 0 || playerPosVirt[1] >= p_playerSight.GetLength(1)))
            {
                p_playerSight[playerPosVirt[0], playerPosVirt[1]].content = CellContent.Player;
                DateTime time = DateTime.Now;
                p_playerSight[playerPosVirt[0], playerPosVirt[1]].time = time;
                RemoveMapInfoAsync(time, 3000);
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
                RemoveMapInfoAsync(time, 500);
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
            DateTime time = DateTime.Now;
            p_consoleWriteLock.Add(time);
            while (!p_consoleWriteLock[0].Equals(time))
            {
            }
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
            Console.Write("\nArrows: Move bot    ");
            if (attackTime > 0)
                Console.Write("Drop bomb: " + ((attackTime < 10) ? " " + attackTime : attackTime.ToString()) + "       ");
            else
                Console.Write("Space: Drop bomb    ");
            if (radarTime > 0)
                Console.Write("Radar: " + ((radarTime < 10) ? " " + radarTime : radarTime.ToString()) + "       ");
            else
                Console.Write("Space: Drop bomb    ");
            Console.WriteLine("Esc: Exit");
            Console.WriteLine("\nGame Log:");
            Console.WriteLine("\n------------------------------------------------------------------\n");
            for (int i = 0; i <= 5; i++)
            {
                string info = (p_gameLog != null && p_gameLog.Count > p_logPointer + i) ? p_gameLog[p_logPointer + i] : string.Empty;
                Console.WriteLine(info);
            }
            Console.WriteLine("\n------------------------------------------------------------------");
            p_consoleWriteLock.RemoveAt(0);
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

        public async Task RadarCooldown()
        {
            radarTime = COOLDOWN_RADAR;
            while (radarTime > 0)
            {
                RefreshClient();
                await Task.Delay(1000);
                radarTime--;
            }
        }

        public async Task AttackCooldown()
        {
            attackTime = COOLDOWN_ATTACK;
            while (attackTime > 0)
            {
                RefreshClient();
                await Task.Delay(1000);
                attackTime--;
            }
        }

        public ServerResponseInfo<bool, SqlException> CreatePlayer(string i_player, string i_pass)
        {
            ServerResponseInfo<bool, SqlException> res = new ServerResponseInfo<bool, SqlException>();
            string path = "login/new";
            List<string> info = new List<string>();
            info.Add(i_player);
            info.Add(i_pass);
            HttpResponseMessage response = p_client.PostAsJsonAsync(path, info.SerializeObject()).Result;
            if (response.IsSuccessStatusCode)
            {
                res = response.Content.ReadAsAsync<ServerResponseInfo<bool, SqlException>>().Result;
            }
            return res;
        }

        public ServerResponseInfo<bool, SqlException> Login(string i_player, string i_pass)
        {
            ServerResponseInfo<bool, SqlException> res = new ServerResponseInfo<bool, SqlException>();
            List<string> info = new List<string>();
            info.Add(i_player);
            info.Add(i_pass);
            string path = "login";
            HttpResponseMessage response = p_client.PostAsJsonAsync(path, info.SerializeObject()).Result;
            if (response.IsSuccessStatusCode)
            {
                res = response.Content.ReadAsAsync<ServerResponseInfo<bool, SqlException>>().Result;
            }
            return res;
        }

        public ServerResponseInfo<bool, Exception> ConnectPlayerAsync(string i_actor, string i_player)
        {
            ServerResponseInfo<bool, Exception> res = new ServerResponseInfo<bool, Exception>();
            string path = "gamemanager/connect";
            List<string> info = new List<string>();
            info.Add(i_actor.SerializeObject());
            info.Add(i_player.SerializeObject());
            //string myIp = new WebClient().DownloadString(@"http://icanhazip.com").Trim();
            //string[] addressArray = myIp.Split('.');
            //byte[] byteArray = new byte[] { Byte.Parse(addressArray[0]), Byte.Parse(addressArray[1]), Byte.Parse(addressArray[2]), Byte.Parse(addressArray[3]) };
            //byte[] byteArray = new byte[] { Byte.Parse("192"), Byte.Parse("168"), Byte.Parse("1"), Byte.Parse("132") };
            IPHostEntry ipHostInfo = //Dns.GetHostEntry(Dns.GetHostName());
                Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            info.Add(ipAddress.GetAddressBytes().SerializeObject());
            HttpResponseMessage response = p_client.PostAsJsonAsync(path, info.SerializeObject()).Result;
            if (response.IsSuccessStatusCode)
            {
                res = response.Content.ReadAsAsync<ServerResponseInfo<bool, Exception>>().Result;
            }
            return res;
        }

        /// <summary>
        /// Refreshes lobby info
        /// </summary>
        /// <param name="i_playersList">Players in the lobby</param>
        public void GameLobbyInfoUpdate(List<string> i_playersList)
        {
            //Clears console and sets color to white
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            //Shows all players in lobby
            Console.WriteLine("Game Lobby\n");
            for (int i = 0; i < i_playersList.Count; i++)
            {
                if (i_playersList[i].Equals(playerName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("->");
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\t" + (i + 1) + ". " + i_playersList[i]);
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nEsc: Exit Lobby");
            Console.WriteLine(DateTime.Now);
            //Notifies server that player stills connected
            PlayerStillConnected();
        }

        /// <summary>
        /// Notifies that a player was killed by other
        /// </summary>
        /// <param name="i_player">Name of the killed player</param>
        /// <param name="i_killer">Name of the player that killed it</param>
        /// <param name="i_playerPos">Position vector of the killed player</param>
        /// <param name="i_reason">Reason for the player death</param>
        public void PlayerKilled(string i_player, string i_killer, int[] i_playerPos, DeathReason i_reason)
        {
            PlayerDeadRecieved(i_playerPos, i_player, i_killer, i_reason);
        }

        /// <summary>
        /// Notifies that a player died
        /// </summary>
        /// <param name="i_player">Name of the dead player</param>
        /// <param name="i_playerPos">Position vector of the killed player</param>
        /// <param name="i_reason">Reason for the player death</param>
        public void PlayerDead(string i_player, int[] i_playerPos, DeathReason i_reason)
        {
            PlayerDeadRecieved(i_playerPos, i_player, i_reason);
        }

        /// <summary>
        /// Notifies of a list of positions where a bomb hit
        /// </summary>
        /// <param name="i_hitList">List of bomb hit position vectors</param>
        public void BombHits(List<int[]> i_hitList)
        {
            BombHitsRecieved(i_hitList);
        }

        /// <summary>
        /// Notifies that a player used radar
        /// </summary>
        /// <param name="o_playerPos">Player position vector</param>
        public void RadarUsed(int[] i_playerPos)
        {
            PlayerDetected(i_playerPos);
        }

        /// <summary>
        /// Notifies that game session was finished
        /// </summary>
        /// <param name="o_winner">Winner player name</param>
        public void GameFinished(string i_winner)
        {
            FinishGame(i_winner);
        }

        public void StartListeningLobby()
        {
            LobbyListener.StartListening(this);
        }

        public void StartListeningGameSession()
        {
            GameSessionListener.StartListening(this);
        }

        public void ExitTutorial()
        {
            p_state = ClientState.Start;
            p_tutorial = null;
        }
    }

    public class GameTutorial
    {
        #region VARIABLES

        /// <summary>
        /// Dictionary with player name as key and current position as value
        /// </summary>
        private Dictionary<string, int[]> p_playerPositions;

        /// <summary>
        /// Bidimensional array with current map cells status
        /// </summary>
        private CellInfo[][] p_mapInfo;

        private enum TutorialState
        {
            BasicConcepts,
            BasicMovement,
            BasicMovement_Example,
            Holes,
            Holes_Example,
            Bombs,
            Bombs_Example,
            Turrets,
            Turrets_Example,
            SightRange,
            SightRange_Example,
            Blindness,
            Blindness_Example
        }

        /// <summary>
        /// Current GameState
        /// </summary>
        private TutorialState p_state;

        private ClientGameManager p_gameManager;


        private int[] p_playerPosReal;
        private int[] p_playerPosVirt;

        private ClientCellInfo[][] p_mapView;

        private List<string> p_deadPlayers;

        private List<int[]> p_turretList;

        private bool p_turretsActive;

        #endregion

        /// <summary>
        /// GameSession constructor. Initializes player lists, set maximum number of players and creates chosen map
        /// </summary>
        /// <param name="i_maxPlayers">Maximum number of players</param>
        /// <param name="i_mapIndex">Chosen map index</param>
        public GameTutorial(ClientGameManager i_gameManager)
        {
            p_gameManager = i_gameManager;
            p_state = TutorialState.BasicConcepts;
            Console.ForegroundColor = ConsoleColor.White;
            Print();
        }

        public void Manage(ConsoleKeyInfo i_key)
        {
            switch (i_key.Key)
            {
                case ConsoleKey.UpArrow:
                    if (p_state.Equals(TutorialState.BasicMovement_Example) || p_state.Equals(TutorialState.Holes_Example) || p_state.Equals(TutorialState.Bombs_Example) || p_state.Equals(TutorialState.Turrets_Example) || p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example))
                    {
                        MovePlayer(new int[] { 0, 1 });
                        Print();
                        CheckPlayers();
                        //CHECK ENEMY ALIVE
                    }
                    break;
                case ConsoleKey.DownArrow:
                    if (p_state.Equals(TutorialState.BasicMovement_Example) || p_state.Equals(TutorialState.Holes_Example) || p_state.Equals(TutorialState.Bombs_Example) || p_state.Equals(TutorialState.Turrets_Example) || p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example))
                    {
                        MovePlayer(new int[] { 0, -1 });
                        Print();
                        CheckPlayers();
                        //CHECK ENEMY ALIVE
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (p_state.Equals(TutorialState.BasicMovement_Example) || p_state.Equals(TutorialState.Holes_Example) || p_state.Equals(TutorialState.Bombs_Example) || p_state.Equals(TutorialState.Turrets_Example) || p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example))
                    {
                        MovePlayer(new int[] { 1, 0 });
                        Print();
                        CheckPlayers();
                        //CHECK ENEMY ALIVE
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    if (p_state.Equals(TutorialState.BasicMovement_Example) || p_state.Equals(TutorialState.Holes_Example) || p_state.Equals(TutorialState.Bombs_Example) || p_state.Equals(TutorialState.Turrets_Example) || p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example))
                    {
                        MovePlayer(new int[] { -1, 0 });
                        Print();
                        CheckPlayers();
                        //CHECK ENEMY ALIVE
                    }
                    break;
                case ConsoleKey.F1:
                    if (p_state.Equals(TutorialState.BasicMovement_Example) || p_state.Equals(TutorialState.Holes_Example) || p_state.Equals(TutorialState.Bombs_Example) || p_state.Equals(TutorialState.Turrets_Example) || p_state.Equals(TutorialState.SightRange_Example))
                    {
                        p_state++;
                        Print();
                        if (p_state.Equals(TutorialState.Turrets_Example))
                            p_turretsActive = false;
                    }
                    else if (p_state.Equals(TutorialState.Blindness_Example))
                    {
                        p_gameManager.ExitTutorial();
                    }
                    break;
                case ConsoleKey.Escape:
                    p_gameManager.ExitTutorial();
                    break;
                case ConsoleKey.Enter:
                    if (p_state.Equals(TutorialState.BasicConcepts) || p_state.Equals(TutorialState.BasicMovement) || p_state.Equals(TutorialState.Holes) || p_state.Equals(TutorialState.Bombs) || p_state.Equals(TutorialState.Turrets) || p_state.Equals(TutorialState.SightRange) || p_state.Equals(TutorialState.Blindness))
                    {
                        p_state++;
                        PrepareExample();
                    }
                    else if (p_state.Equals(TutorialState.Blindness_Example))
                        RadarUsed();
                    break;
                case ConsoleKey.Spacebar:
                    if (p_state.Equals(TutorialState.Bombs_Example) || p_state.Equals(TutorialState.Turrets_Example) || p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example))
                    {
                        PlayerAttacks();
                        Print();
                        CheckPlayers();
                    }
                    break;
            }
        }

        public void Print()
        {
            Console.Clear();
            switch (p_state)
            {
                case TutorialState.BasicConcepts:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Lesson 0 - Basic Concepts");
                    Console.WriteLine("--------");
                    Console.WriteLine("Here you have some basic concepts of the game explained.");
                    Console.WriteLine("Each of these symbols gives information of a cell on the map:");
                    Console.WriteLine("\t?\t->\tUnknown info.");
                    Console.WriteLine("\tX\t->\tNavigable cell.");
                    Console.WriteLine("\tO\t->\tHole.");
                    Console.WriteLine("\tP\t->\tYour position.");
                    Console.WriteLine("\tE\t->\tOther player position.");
                    Console.WriteLine("\tB\t->\tBombhit.");
                    Console.WriteLine("\tD\t->\tDead player.");
                    Console.WriteLine("\tT\t->\tTurret aiming to this cell.");
                    Console.WriteLine("\nEnter: Next lesson   Esc: Exit tutorial");
                    break;
                case TutorialState.BasicMovement:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Lesson 1 - Basic Movement");
                    Console.WriteLine("--------");
                    Console.WriteLine("Use arrows to move around navigable cells on the map (X).");
                    Console.WriteLine("Place yourself on the same cell that other player to kill him.");
                    Console.WriteLine("Let's try!.");
                    Console.WriteLine("\nEnter: Try it!   Esc: Exit tutorial");
                    break;
                case TutorialState.BasicMovement_Example:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Example 1 - Basic Movement");
                    Console.WriteLine("--------");
                    Console.WriteLine("Kill the other player\n");
                    PaintMap();
                    Console.WriteLine("\nArrows: Move bot    F1: Skip example   Esc: Exit tutorial");
                    break;
                case TutorialState.Holes:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Lesson 2 - Holes");
                    Console.WriteLine("--------");
                    Console.WriteLine("If you fall into a hole (O) you will die.");
                    Console.WriteLine("Map limits are sorrounded by holes.");
                    Console.WriteLine("Let's try!.");
                    Console.WriteLine("\nEnter: Try it!   Esc: Exit tutorial");
                    break;
                case TutorialState.Holes_Example:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Example 2 - Holes");
                    Console.WriteLine("--------");
                    Console.WriteLine("Avoid holes and kill the other player\n");
                    PaintMap();
                    Console.WriteLine("\nArrows: Move bot    F1: Skip example   Esc: Exit tutorial");
                    break;
                case TutorialState.Bombs:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Lesson 3 - Bombs");
                    Console.WriteLine("--------");
                    Console.WriteLine("There's other way to kill an enemy. Dropping a bomb.");
                    Console.WriteLine("Press SPACE to drop a bomb. Bomb will hit (B) all cells at a distance of 2 cells from player.");
                    Console.WriteLine("During game bomb attack has a cooldown of 10 seconds.");
                    Console.WriteLine("Let's try!.");
                    Console.WriteLine("\nEnter: Try it!   Esc: Exit tutorial");
                    break;
                case TutorialState.Bombs_Example:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Example 3 - Bombs");
                    Console.WriteLine("--------");
                    Console.WriteLine("Kill the other player\n");
                    PaintMap();
                    Console.WriteLine("\nArrows: Move bot    Space: Bomb    F1: Skip example   Esc: Exit tutorial");
                    break;
                case TutorialState.Turrets:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Lesson 4 - Turrets");
                    Console.WriteLine("--------");
                    Console.WriteLine("Turrets can drop bombs on every cell on the map.");
                    Console.WriteLine("Turrets start aiming to a cell (T) and after a sort time it drops the bomb.");
                    Console.WriteLine("Let's try!.");
                    Console.WriteLine("\nEnter: Try it!   Esc: Exit tutorial");
                    break;
                case TutorialState.Turrets_Example:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Example 4 - Turrets");
                    Console.WriteLine("--------");
                    Console.WriteLine("Avoid turrets and kill the other player\n");
                    PaintMap();
                    Console.WriteLine("\nArrows: Move bot    Space: Bomb    F1: Skip example   Esc: Exit tutorial");
                    break;
                case TutorialState.SightRange:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Lesson 5 - Sight range");
                    Console.WriteLine("--------");
                    Console.WriteLine("Your bot has a limited sight range. It can't see the whole map and only see a 5x5 cell grid.");
                    Console.WriteLine("Your bot will be always at the center of this grid.");
                    Console.WriteLine("Let's try!.");
                    Console.WriteLine("\nEnter: Try it!   Esc: Exit tutorial");
                    break;
                case TutorialState.SightRange_Example:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Example 5 - Sight range");
                    Console.WriteLine("--------");
                    Console.WriteLine("Locate and kill the other player\n");
                    PaintMap();
                    Console.WriteLine("\nArrows: Move bot    Space: Bomb    F1: Skip example   Esc: Exit tutorial");
                    break;
                case TutorialState.Blindness:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Lesson 6 - Bot blindness and memory");
                    Console.WriteLine("--------");
                    Console.WriteLine("Your bot can't see anything. It only can notice bombhits (B), turret aim points (T) and players that had used the radar (E).");
                    Console.WriteLine("In addition, your bot has a limited memory and recieved info will be deleted after a little period of time.");
                    Console.WriteLine("Most of the time your grid will be full of unknonw info (?) but you can press ENTER to use your radar and recieve all cell info on your grid. During game radar has a cooldown of 30 seconds.");
                    Console.WriteLine("Let's try!.");
                    Console.WriteLine("\nEnter: Try it!   Esc: Exit tutorial");
                    break;
                case TutorialState.Blindness_Example:
                    Console.WriteLine("TUTORIAL");
                    Console.WriteLine("--------");
                    Console.WriteLine("Example 6 - Bot blindness and memory");
                    Console.WriteLine("--------");
                    Console.WriteLine("Locate and kill the other player\n");
                    PaintMap();
                    Console.WriteLine("\nArrows: Move bot    Space: Bomb    Enter: Radar    F1: Skip example   Esc: Exit tutorial");
                    break;
            }
        }

        public void PaintMap()
        {
            for (int j = p_mapView[0].Length - 1; j >= 0; j--)
            {
                string res = string.Empty;
                for (int i = 0; i < p_mapView.Length; i++)
                {
                    if ((p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example)) && i == p_playerPosVirt[0] && j == p_playerPosVirt[1] && (p_mapView[p_playerPosVirt[0]][p_playerPosVirt[1]] == null || !p_mapView[p_playerPosVirt[0]][ p_playerPosVirt[1]].Equals(CellContent.Aiming)))
                    {
                        res += "P ";
                    }
                    else
                    {
                        switch (p_mapView[i][j].content)
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
                                if (!(p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example)) && p_playerPositions["Player"][0] == i && p_playerPositions["Player"][1] == j)
                                    res += "P ";
                                else
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
        }

        public void PrepareExample()
        {
            p_deadPlayers = new List<string>();
            switch (p_state)
            {
                case TutorialState.BasicMovement_Example:
                    #region PREPARE_BASIC_MOVEMENT_EXAMPLE
                    p_playerPositions = new Dictionary<string, int[]>();
                    p_mapInfo = new CellInfo[8][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                        p_mapInfo[i] = new CellInfo[p_mapInfo.Length];
                    p_mapInfo[6][1] = new CellInfo("Player");
                    p_playerPositions.Add("Player", new int[] { 6, 1 });
                    p_mapInfo[1][6] = new CellInfo("Enemy");
                    p_playerPositions.Add("Enemy", new int[] { 1, 6 });

                    p_mapView = new ClientCellInfo[p_mapInfo.Length][];
                    for (int i = 0; i < p_mapView.Length; i++)
                        p_mapView[i] = new ClientCellInfo[p_mapView.Length];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        for (int j = 0; j < p_mapInfo.Length; j++)
                        {
                            p_mapView[i][j] = new ClientCellInfo();
                            p_mapView[i][j].content = (p_mapInfo[i][j] == null) ? CellContent.Floor : p_mapInfo[i][j].content;
                        }
                    }
                    #endregion
                    break;
                case TutorialState.Holes_Example:
                    #region PREPARE_HOLES_EXAMPLE
                    p_playerPositions = new Dictionary<string, int[]>();
                    p_mapInfo = new CellInfo[9][];

                    for (int i = 0; i < p_mapInfo.Length; i++)
                        p_mapInfo[i] = new CellInfo[p_mapInfo.Length];
                    p_mapInfo[0][0] = new CellInfo();
                    p_mapInfo[p_mapInfo.Length - 1][0] = new CellInfo();
                    p_mapInfo[0][p_mapInfo.Length - 1] = new CellInfo();
                    p_mapInfo[p_mapInfo.Length - 1][p_mapInfo.Length - 1] = new CellInfo();
                    for (int i = 1; i < p_mapInfo.Length - 1; i++)
                    {
                        p_mapInfo[0][i] = new CellInfo();
                        p_mapInfo[p_mapInfo.Length - 1][i] = new CellInfo();
                        p_mapInfo[i][0] = new CellInfo();
                        p_mapInfo[i][p_mapInfo.Length - 1] = new CellInfo();
                    }

                    p_mapInfo[4][2] = new CellInfo("Player");
                    p_playerPositions.Add("Player", new int[] { 4, 2 });
                    p_mapInfo[4][5] = new CellInfo("Enemy");
                    p_playerPositions.Add("Enemy", new int[] { 4, 5 });
                    p_mapInfo[3][5] = new CellInfo();
                    p_mapInfo[3][6] = new CellInfo();
                    p_mapInfo[4][4] = new CellInfo();
                    p_mapInfo[5][5] = new CellInfo();
                    p_mapInfo[5][6] = new CellInfo();

                    p_mapView = new ClientCellInfo[p_mapInfo.Length][];
                    for (int i = 0; i < p_mapView.Length; i++)
                        p_mapView[i] = new ClientCellInfo[p_mapView.Length];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        for (int j = 0; j < p_mapInfo.Length; j++)
                        {
                            p_mapView[i][j] = new ClientCellInfo();
                            p_mapView[i][j].content = (p_mapInfo[i][j] == null) ? CellContent.Floor : p_mapInfo[i][j].content;
                        }
                    }
                    #endregion
                    break;
                case TutorialState.Bombs_Example:
                    #region PREPARE_BOMBS_EXAMPLE
                    p_playerPositions = new Dictionary<string, int[]>();
                    p_mapInfo = new CellInfo[9][];

                    for (int i = 0; i < p_mapInfo.Length; i++)
                        p_mapInfo[i] = new CellInfo[p_mapInfo.Length];
                    p_mapInfo[0][0] = new CellInfo();
                    p_mapInfo[p_mapInfo.Length - 1][0] = new CellInfo();
                    p_mapInfo[0][p_mapInfo.Length - 1] = new CellInfo();
                    p_mapInfo[p_mapInfo.Length - 1][p_mapInfo.Length - 1] = new CellInfo();
                    for (int i = 1; i < p_mapInfo.Length - 1; i++)
                    {
                        p_mapInfo[0][i] = new CellInfo();
                        p_mapInfo[p_mapInfo.Length - 1][i] = new CellInfo();
                        p_mapInfo[i][0] = new CellInfo();
                        p_mapInfo[i][p_mapInfo.Length - 1] = new CellInfo();
                    }

                    p_mapInfo[4][2] = new CellInfo("Player");
                    p_playerPositions.Add("Player", new int[] { 4, 2 });
                    p_mapInfo[4][5] = new CellInfo("Enemy");
                    p_playerPositions.Add("Enemy", new int[] { 4, 5 });
                    p_mapInfo[3][5] = new CellInfo();
                    p_mapInfo[4][4] = new CellInfo();
                    p_mapInfo[4][6] = new CellInfo();
                    p_mapInfo[5][5] = new CellInfo();

                    p_mapView = new ClientCellInfo[p_mapInfo.Length][];
                    for (int i = 0; i < p_mapView.Length; i++)
                        p_mapView[i] = new ClientCellInfo[p_mapView.Length];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        for (int j = 0; j < p_mapInfo.Length; j++)
                        {
                            p_mapView[i][j] = new ClientCellInfo();
                            p_mapView[i][j].content = (p_mapInfo[i][j] == null) ? CellContent.Floor : p_mapInfo[i][j].content;
                        }
                    }
                    #endregion
                    break;
                case TutorialState.Turrets_Example:
                    #region PREPARE_TURRETS_EXAMPLE
                    p_playerPositions = new Dictionary<string, int[]>();
                    p_mapInfo = new CellInfo[11][];

                    for (int i = 0; i < p_mapInfo.Length; i++)
                        p_mapInfo[i] = new CellInfo[p_mapInfo.Length];
                    p_mapInfo[0][0] = new CellInfo();
                    p_mapInfo[p_mapInfo.Length - 1][0] = new CellInfo();
                    p_mapInfo[0][p_mapInfo.Length - 1] = new CellInfo();
                    p_mapInfo[p_mapInfo.Length - 1][p_mapInfo.Length - 1] = new CellInfo();
                    for (int i = 1; i < p_mapInfo.Length - 1; i++)
                    {
                        p_mapInfo[0][i] = new CellInfo();
                        p_mapInfo[p_mapInfo.Length - 1][i] = new CellInfo();
                        p_mapInfo[i][0] = new CellInfo();
                        p_mapInfo[i][p_mapInfo.Length - 1] = new CellInfo();
                    }

                    p_mapInfo[1][1] = new CellInfo("Player");
                    p_playerPositions.Add("Player", new int[] { 1, 1 });
                    p_mapInfo[5][5] = new CellInfo("Enemy");
                    p_playerPositions.Add("Enemy", new int[] { 5, 5 });

                    p_turretList = new List<int[]>();
                    p_turretList.Add(new int[] { 5, 2 });
                    p_turretList.Add(new int[] { 5, 8 });
                    p_turretList.Add(new int[] { 2, 5 });
                    p_turretList.Add(new int[] { 8, 5 });

                    PrepareTurrets();

                    p_mapView = new ClientCellInfo[p_mapInfo.Length][];
                    for (int i = 0; i < p_mapView.Length; i++)
                        p_mapView[i] = new ClientCellInfo[p_mapView.Length];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        for (int j = 0; j < p_mapInfo.Length; j++)
                        {
                            p_mapView[i][j] = new ClientCellInfo();
                            p_mapView[i][j].content = (p_mapInfo[i][j] == null) ? CellContent.Floor : p_mapInfo[i][j].content;
                        }
                    }
                    #endregion
                    break;
                case TutorialState.SightRange_Example:
                    #region PREPARE_SIGHT_RANGE_EXAMPLE
                    p_playerPositions = new Dictionary<string, int[]>();
                    p_mapInfo = new CellInfo[8][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                        p_mapInfo[i] = new CellInfo[p_mapInfo.Length];

                    p_mapInfo[6][1] = new CellInfo("Player");
                    p_playerPositions.Add("Player", new int[] { 6, 1 });
                    p_mapInfo[1][6] = new CellInfo("Enemy");
                    p_playerPositions.Add("Enemy", new int[] { 1, 6 });

                    p_mapInfo[1][4] = new CellInfo();
                    p_mapInfo[2][2] = new CellInfo();
                    p_mapInfo[2][5] = new CellInfo();
                    p_mapInfo[3][6] = new CellInfo();
                    p_mapInfo[4][1] = new CellInfo();
                    p_mapInfo[5][2] = new CellInfo();
                    p_mapInfo[5][5] = new CellInfo();
                    p_mapInfo[6][3] = new CellInfo();

                    p_mapView = new ClientCellInfo[5][];
                    for (int i = 0; i < p_mapView.Length; i++)
                        p_mapView[i] = new ClientCellInfo[p_mapView.Length];
                    int[] playerPosReal = p_playerPositions["Player"];
                    //Gets player virtual position
                    p_playerPosVirt = new int[] { (int)Math.Truncate(p_mapView.Length / 2f), (int)Math.Truncate(p_mapView.Length / 2f) };
                    //Covers all cells on player sight
                    for (int i = 0; i < p_mapView.Length; i++)
                    {
                        for (int j = 0; j < p_mapView[0].Length; j++)
                        {
                            //If cell is not player cell
                            if (i != p_playerPosVirt[0] || j != p_playerPosVirt[1])
                            {
                                //Gets cell real pos
                                int[] realPos = new int[] { playerPosReal[0] - p_playerPosVirt[0] + i, playerPosReal[1] - p_playerPosVirt[1] + j };
                                //If cell pos is out of map bounds
                                if ((realPos[0] < 0) || (realPos[0] >= p_mapInfo.Length) || (realPos[1] < 0) || (realPos[1] >= p_mapInfo[0].Length))
                                {
                                    //Cell content is a hole
                                    p_mapView[i][j] = new ClientCellInfo();
                                    p_mapView[i][j].content = CellContent.Hole;
                                }
                                //otherwise
                                else
                                {
                                    //If cell info is empty, cell content is Floor; cell info is copied otherwise;
                                    p_mapView[i][j] = new ClientCellInfo();
                                    p_mapView[i][j].content = p_mapInfo[realPos[0]][realPos[1]] == null ? CellContent.Floor : p_mapInfo[realPos[0]][realPos[1]].content;
                                }
                            }
                        }
                    }
                    #endregion
                    break;
                case TutorialState.Blindness_Example:
                    #region PREPARE_BLINDNESS_EXAMPLE
                    p_playerPositions = new Dictionary<string, int[]>();
                    p_mapInfo = new CellInfo[8][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                        p_mapInfo[i] = new CellInfo[p_mapInfo.Length];

                    p_mapInfo[6][1] = new CellInfo("Player");
                    p_playerPositions.Add("Player", new int[] { 6, 1 });
                    p_mapInfo[1][6] = new CellInfo("Enemy");
                    p_playerPositions.Add("Enemy", new int[] { 1, 6 });

                    p_mapInfo[1][4] = new CellInfo();
                    p_mapInfo[2][2] = new CellInfo();
                    p_mapInfo[2][5] = new CellInfo();
                    p_mapInfo[3][6] = new CellInfo();
                    p_mapInfo[4][1] = new CellInfo();
                    p_mapInfo[5][2] = new CellInfo();
                    p_mapInfo[5][5] = new CellInfo();
                    p_mapInfo[6][3] = new CellInfo();

                    p_mapView = new ClientCellInfo[5][];
                    for (int i = 0; i < p_mapView.Length; i++)
                    {
                        p_mapView[i] = new ClientCellInfo[p_mapView.Length];
                        for (int j = 0; j < p_mapView[0].Length; j++)
                            p_mapView[i][j] = new ClientCellInfo();
                    }
                    p_playerPosVirt = new int[] { (int)Math.Truncate(p_mapView.Length / 2f), (int)Math.Truncate(p_mapView.Length / 2f) };
                    //COPY FROM CLIENT
                    #endregion
                    break;
            }
            Print();
        }

        public void MovePlayer(int[] i_dir)
        {
            //Gets current player position vector
            int[] playerPos = p_playerPositions["Player"];
            //Calculates player destination position vector
            int[] destPos = new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] };
            //True if player position is out of map bounds
            bool outOfBounds = (destPos[0] < 0 || destPos[0] >= p_mapInfo.Length || destPos[1] < 0 || destPos[1] >= p_mapInfo[0].Length);
            //Gets player destination position cell info if player is not out of bounds
            CellInfo destPosInfo = outOfBounds ? null : p_mapInfo[destPos[0]][destPos[1]];
            //If player is out of bounds or destination cell is a hole
            if ((outOfBounds && (p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example))) || (destPosInfo != null && destPosInfo.content.Equals(CellContent.Hole)))
            {
                //Adds player to dead players list
                p_deadPlayers.Add("Player");
                //Eliminates player from original cell
                p_mapInfo[playerPos[0]][playerPos[1]] = null;
                switch (p_state)
                {
                    //COPY FROM CLIENT
                    case TutorialState.SightRange_Example:
                        break;
                    case TutorialState.Blindness_Example:
                        /*
                        p_mapView[p_playerPosVirt[0]][p_playerPosVirt[1]].content = CellContent.Floor;
                        //DateTime time = DateTime.Now;
                        //p_mapView[p_playerPosVirt[0]][p_playerPosVirt[1]].time = time;
                        //p_playerPosReal[0] += i_dir[0];
                        //p_playerPosReal[1] += i_dir[1];
                        int iniXPos = 0;
                        int iniYPos = 0;
                        int finXPos = p_mapView.Length - 1;
                        int finYPos = p_mapView[0].Length - 1;
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
                        for (int i = iniXPos, contX = 0; contX < p_mapView.Length; i = i + xInc, contX++)
                        {
                            for (int j = iniYPos, contY = 0; contY < p_mapView[0].Length; j = j + yInc, contY++)
                            {
                                int destX = i + i_dir[0];
                                int destY = j + i_dir[1];
                                p_mapView[i][j] = (destX < 0 || destX > p_mapView.Length - 1 || destY < 0 || destY > p_mapView[0].Length - 1) ? new ClientCellInfo() : p_mapView[destX][destY];
                            }
                        }
                        */
                        break;
                    default:
                        p_mapView[playerPos[0]][playerPos[1]].content = CellContent.Floor;
                        break;
                }
            }
            else if (!outOfBounds)
            {
                //If destination cell has a player
                if (destPosInfo != null && destPosInfo.content.Equals(CellContent.Player))
                {
                    //Adds player to dead players list
                    p_deadPlayers.Add(destPosInfo.playerId);
                }
                //Moves player to destination position
                p_mapInfo[playerPos[0] + i_dir[0]][playerPos[1] + i_dir[1]] = p_mapInfo[playerPos[0]][playerPos[1]];
                //Eliminates player from original position
                p_mapInfo[playerPos[0]][playerPos[1]] = null;
                int iniXPos;
                int iniYPos;
                int finXPos;
                int finYPos;
                int xInc;
                int yInc;
                switch (p_state)
                {
                    //COPY FROM CLIENT
                    case TutorialState.SightRange_Example:
                        p_mapView[p_playerPosVirt[0]][p_playerPosVirt[1]] = new ClientCellInfo();
                        p_mapView[p_playerPosVirt[0]][p_playerPosVirt[1]].content = CellContent.Floor;
                        //DateTime time = DateTime.Now;
                        //p_mapView[p_playerPosVirt[0]][p_playerPosVirt[1]].time = time;
                        //p_playerPosReal[0] += i_dir[0];
                        //p_playerPosReal[1] += i_dir[1];
                        iniXPos = 0;
                        iniYPos = 0;
                        finXPos = p_mapView.Length - 1;
                        finYPos = p_mapView[0].Length - 1;
                        xInc = 1;
                        yInc = 1;
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
                        for (int i = iniXPos, contX = 0; contX < p_mapView.Length; i = i + xInc, contX++)
                        {
                            for (int j = iniYPos, contY = 0; contY < p_mapView[0].Length; j = j + yInc, contY++)
                            {
                                int destX = i + i_dir[0];
                                int destY = j + i_dir[1];
                                if (destX < 0 || destX > p_mapView.Length - 1 || destY < 0 || destY > p_mapView[0].Length - 1)
                                {
                                    p_mapView[i][j] = new ClientCellInfo();
                                    int xFin = i - p_playerPosVirt[0] + (playerPos[0] + i_dir[0]);
                                    int yFin = j - p_playerPosVirt[1] + (playerPos[1] + i_dir[1]);
                                    p_mapView[i][j].content = (xFin <0 || xFin >= p_mapInfo.Length || yFin <0 || yFin >= p_mapInfo[0].Length) ? CellContent.Hole : (p_mapInfo[xFin][yFin] != null) ? p_mapInfo[xFin][yFin].content : CellContent.Floor;
                                }
                                else
                                    p_mapView[i][j] = p_mapView[destX][destY];
                            }
                        }
                        break;
                    case TutorialState.Blindness_Example:
                        DateTime time = DateTime.Now;
                        p_mapView[p_playerPosVirt[0]][p_playerPosVirt[1]].content = CellContent.Floor;
                        p_mapView[p_playerPosVirt[0]][p_playerPosVirt[1]].time = time;
                        //DateTime time = DateTime.Now;
                        //p_mapView[p_playerPosVirt[0]][p_playerPosVirt[1]].time = time;
                        //p_playerPosReal[0] += i_dir[0];
                        //p_playerPosReal[1] += i_dir[1];
                        iniXPos = 0;
                        iniYPos = 0;
                        finXPos = p_mapView.Length - 1;
                        finYPos = p_mapView[0].Length - 1;
                        xInc = 1;
                        yInc = 1;
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
                        for (int i = iniXPos, contX = 0; contX < p_mapView.Length; i = i + xInc, contX++)
                        {
                            for (int j = iniYPos, contY = 0; contY < p_mapView[0].Length; j = j + yInc, contY++)
                            {
                                int destX = i + i_dir[0];
                                int destY = j + i_dir[1];
                                p_mapView[i][j] = (destX < 0 || destX > p_mapView.Length - 1 || destY < 0 || destY > p_mapView[0].Length - 1) ? new ClientCellInfo() : p_mapView[destX][destY];
                            }
                        }
                        RemoveMapInfoAsync(time, 3000);
                        break;
                    default:
                        p_mapView[playerPos[0] + i_dir[0]][playerPos[1] + i_dir[1]].content = CellContent.Player;
                        p_mapView[playerPos[0]][playerPos[1]].content = CellContent.Floor;
                        break;
                }
                //Removes player position register
                p_playerPositions.Remove("Player");
                //Adds a new player position register
                p_playerPositions.Add("Player", new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] });
            }
        }

        public void CheckPlayers()
        {
            if (p_deadPlayers.Contains("Enemy"))
            {
                if (p_state.Equals(TutorialState.BasicMovement_Example) || p_state.Equals(TutorialState.Holes_Example) || p_state.Equals(TutorialState.Bombs_Example) || p_state.Equals(TutorialState.Turrets_Example) || p_state.Equals(TutorialState.SightRange_Example))
                {
                    p_state++;
                    Print();
                    if (p_state.Equals(TutorialState.Turrets_Example))
                        p_turretsActive = false;
                }
                else if (p_state.Equals(TutorialState.Blindness_Example))
                {
                    p_gameManager.ExitTutorial();
                }
            }
            else if (p_deadPlayers.Contains("Player"))
                PrepareExample();

        }

        public void PlayerAttacks()
        {
            List<int[]> hitPoints = new List<int[]>();
            //Covers all attack positions
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -(2 - Math.Abs(i)); j <= 2 - Math.Abs(i); j++)
                {
                    //Sets hit position vector
                    int[] hitPos = new int[] { p_playerPositions["Player"][0] + i, p_playerPositions["Player"][1] + j };
                    //True if hit position if ot of map bounds
                    bool outOfBounds = (hitPos[0] < 0 || hitPos[0] >= p_mapInfo.Length || hitPos[1] < 0 || hitPos[1] >= p_mapInfo[0].Length);
                    //If hit position is not out of bounds and has a player that is not attacking player
                    if (!outOfBounds && p_mapInfo[hitPos[0]][hitPos[1]] != null && p_mapInfo[hitPos[0]][hitPos[1]].content.Equals(CellContent.Player) && !p_mapInfo[hitPos[0]][hitPos[1]].playerId.Equals("Player"))
                    {
                        //Gets dead player name
                        string deadPlayer = p_mapInfo[p_playerPositions["Player"][0] + i][p_playerPositions["Player"][1] + j].playerId;
                        //Eliminates dead player from hit position
                        p_mapInfo[p_playerPositions["Player"][0] + i][p_playerPositions["Player"][1] + j] = null;
                        //Adds dead player to dead players list
                        p_deadPlayers.Add(deadPlayer);
                    }
                    //Otherwise
                    else
                    {
                        //Adds hit position to hit points list
                        hitPoints.Add(hitPos);
                    }
                }
            }
            DateTime time = DateTime.Now;
            foreach (int[] hitPoint in hitPoints)
            {
                if (!(p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example)))
                {
                    if ((hitPoint[0] != p_playerPositions["Player"][0] || hitPoint[1] != p_playerPositions["Player"][1]) && hitPoint[0] >= 0 && hitPoint[1] >= 0 && hitPoint[0] < p_mapView.Length && hitPoint[1] < p_mapView[0].Length)
                    {
                        p_mapView[hitPoint[0]][hitPoint[1]] = new ClientCellInfo();
                        p_mapView[hitPoint[0]][hitPoint[1]].content = CellContent.Hit;
                        p_mapView[hitPoint[0]][hitPoint[1]].time = time;
                    }
                }
                else
                {
                    int[] hitPosVirt = new int[] { hitPoint[0] - p_playerPositions["Player"][0] + p_playerPosVirt[0], hitPoint[1] - p_playerPositions["Player"][1] + p_playerPosVirt[1] };
                    if (!(hitPosVirt[0] < 0 || hitPosVirt[0] >= p_mapView.Length || hitPosVirt[1] < 0 || hitPosVirt[1] >= p_mapView[0].Length) && (hitPosVirt[0] != p_playerPosVirt[0] || hitPosVirt[1] != p_playerPosVirt[1]))
                    {
                        p_mapView[hitPosVirt[0]][hitPosVirt[1]] = new ClientCellInfo();
                        p_mapView[hitPosVirt[0]][hitPosVirt[1]].content = CellContent.Hit;
                        p_mapView[hitPosVirt[0]][hitPosVirt[1]].time = time;
                    }
                }
            }
            RemoveMapInfoAsync(time, 3000);
        }

        public void TurretAttacks(int[] i_pos)
        {
            List<int[]> hitPoints = new List<int[]>();
            //Covers all attack positions
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -(2 - Math.Abs(i)); j <= 2 - Math.Abs(i); j++)
                {
                    //Sets hit position vector
                    int[] hitPos = new int[] { i_pos[0] + i, i_pos[1] + j };
                    //True if hit position if ot of map bounds
                    bool outOfBounds = (hitPos[0] < 0 || hitPos[0] >= p_mapInfo.Length || hitPos[1] < 0 || hitPos[1] >= p_mapInfo[0].Length);
                    //If hit position is not out of bounds and has a player that is not attacking player
                    if (!outOfBounds && p_mapInfo[hitPos[0]][hitPos[1]] != null && p_mapInfo[hitPos[0]][hitPos[1]].content.Equals(CellContent.Player))
                    {
                        //Gets dead player name
                        string deadPlayer = p_mapInfo[i_pos[0] + i][i_pos[1] + j].playerId;
                        //Eliminates dead player from hit position
                        p_mapInfo[i_pos[0] + i][i_pos[1] + j] = null;
                        //Adds dead player to dead players list
                        p_deadPlayers.Add(deadPlayer);
                    }
                    //Otherwise
                    else
                    {
                        //Adds hit position to hit points list
                        hitPoints.Add(hitPos);
                    }
                }
            }
            DateTime time = DateTime.Now;
            foreach (int[] hitPoint in hitPoints)
            {
                if (!(p_state.Equals(TutorialState.SightRange_Example) || p_state.Equals(TutorialState.Blindness_Example)))
                {
                    if (hitPoint[0] >= 0 && hitPoint[1] >= 0 && hitPoint[0] < p_mapView.Length && hitPoint[1] < p_mapView[0].Length)
                    {
                        p_mapView[hitPoint[0]][hitPoint[1]] = new ClientCellInfo();
                        p_mapView[hitPoint[0]][hitPoint[1]].content = CellContent.Hit;
                        p_mapView[hitPoint[0]][hitPoint[1]].time = time;
                    }
                }
                else
                {
                    int[] hitPosVirt = new int[] { hitPoint[0] - p_playerPositions["Player"][0] + p_playerPosVirt[0], hitPoint[1] - p_playerPositions["Player"][1] + p_playerPosVirt[1] };
                    if (!(hitPosVirt[0] < 0 || hitPosVirt[0] >= p_mapView.Length || hitPosVirt[1] < 0 || hitPosVirt[1] >= p_mapView[0].Length))
                    {
                        p_mapView[hitPosVirt[0]][hitPosVirt[1]] = new ClientCellInfo();
                        p_mapView[hitPosVirt[0]][hitPosVirt[1]].content = CellContent.Hit;
                        p_mapView[hitPosVirt[0]][hitPosVirt[1]].time = time;
                    }
                }
            }
            RemoveMapInfoAsync(time, 3000);
        }

        public void RadarUsed()
        {
            DateTime time = DateTime.Now;
            int[] playerPosReal = p_playerPositions["Player"];
            //Covers all cells on player sight
            for (int i = 0; i < p_mapView.Length; i++)
            {
                for (int j = 0; j < p_mapView[0].Length; j++)
                {
                    //If cell is not player cell
                    if (i != p_playerPosVirt[0] || j != p_playerPosVirt[1])
                    {
                        //Gets cell real pos
                        int[] realPos = new int[] { playerPosReal[0] - p_playerPosVirt[0] + i, playerPosReal[1] - p_playerPosVirt[1] + j };
                        //If cell pos is out of map bounds
                        if ((realPos[0] < 0) || (realPos[0] >= p_mapInfo.Length) || (realPos[1] < 0) || (realPos[1] >= p_mapInfo[0].Length))
                        {
                            //Cell content is a hole
                            p_mapView[i][j] = new ClientCellInfo();
                            p_mapView[i][j].content = CellContent.Hole;
                        }
                        //otherwise
                        else
                        {
                            //If cell info is empty, cell content is Floor; cell info is copied otherwise;
                            p_mapView[i][j] = new ClientCellInfo();
                            p_mapView[i][j].content = p_mapInfo[realPos[0]][realPos[1]] == null ? CellContent.Floor : p_mapInfo[realPos[0]][realPos[1]].content;
                        }
                        p_mapView[i][j].time = time;
                    }
                }
            }
            RemoveMapInfoAsync(time, 3000);
        }

        public async Task RemoveMapInfoAsync(DateTime i_time, int i_millis)
        {
            await Task.Delay(i_millis);
            for(int i=0; i<p_mapView.Length; i++)
            {
                for(int j=0; j<p_mapView[0].Length; j++)
                {
                    if (p_mapView[i][j].time.Equals(i_time))
                    {
                        p_mapView[i][j].time = DateTime.MinValue;
                        if (p_state.Equals(TutorialState.Blindness_Example))
                            p_mapView[i][j].content = CellContent.None;
                        else if (p_state.Equals(TutorialState.SightRange_Example))
                        {
                            //Gets cell real pos
                            int[] realPos = new int[] { p_playerPositions["Player"][0] - p_playerPosVirt[0] + i, p_playerPositions["Player"][1] - p_playerPosVirt[1] + j };
                            //If cell pos is out of map bounds
                            if ((realPos[0] < 0) || (realPos[0] >= p_mapInfo.Length) || (realPos[1] < 0) || (realPos[1] >= p_mapInfo[0].Length))
                            {
                                p_mapView[i][j].content = CellContent.Hole;
                            }
                            //otherwise
                            else
                            {
                                p_mapView[i][j].content = p_mapInfo[realPos[0]][realPos[1]] == null ? CellContent.Floor : p_mapInfo[realPos[0]][realPos[1]].content;
                            }
                        }
                        else
                        {
                            p_mapView[i][j].content = (p_mapInfo[i][j] == null) ? CellContent.Floor : p_mapInfo[i][j].content;
                        }
                    }
                }
            }
            Print();
        }

        public async Task PrepareTurrets()
        {
            p_turretsActive = true;
            while (p_turretsActive)
            {
                for (int i = 0; i <= 3; i++)
                {
                    await Task.Delay(1000);
                    DateTime time = DateTime.Now;
                    foreach (int[] pos in p_turretList)
                    {
                        p_mapView[pos[0]][pos[1]].content = CellContent.Aiming;
                        p_mapView[pos[0]][pos[1]].time = time;
                        Print();
                    }
                    RemoveMapInfoAsync(time, 500);
                }
                await Task.Delay(1000);
                foreach (int[] pos in p_turretList)
                {
                    TurretAttacks(pos);
                }
                Print();
            }
        }
    }
}
