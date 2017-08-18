using BasicClasses.GameManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerBasicClasses.GameManager
{
    /// <summary>
    /// Different things that can happen when player moves
    /// </summary>
    public enum MovementResultType
    {
        /// <summary>
        /// Player is not connected to GameSession, cuoldn't be moved
        /// </summary>
        PlayerNotConnected,

        /// <summary>
        /// Player moved normally
        /// </summary>
        Nothing,

        /// <summary>
        /// Player died falling in a hole
        /// </summary>
        PlayerDied,

        /// <summary>
        /// Player killed other player
        /// </summary>
        KilledPlayer
    }

    /// <summary>
    /// Information about consecuences of a player movement
    /// </summary>
    public class MovementResult
    {
        /// <summary>
        /// Did something happen?
        /// </summary>
        public MovementResultType type;

        /// <summary>
        /// Dead player position vector
        /// </summary>
        public int[] playerPos;

        /// <summary>
        /// Killed player name
        /// </summary>
        public string killedPlayer;
    }

    /// <summary>
    /// Information about consecuences of a player attack
    /// </summary>
    public class AttackResult
    {
        /// <summary>
        /// True if player is connected to GameSession
        /// </summary>
        public bool success;

        /// <summary>
        /// Hit position vector list
        /// </summary>
        public List<int[]> hitPoints;

        /// <summary>
        /// Killed players dictionary with player name as key and death position vecctor as value
        /// </summary>
        public Dictionary<string, int[]> killedPlayersDict;
    }

    /// <summary>
    /// Information obtained with the use of radar
    /// </summary>
    public class RadarResult
    {
        /// <summary>
        /// True if player is connected to GameSession
        /// </summary>
        public bool success;

        /// <summary>
        /// Map info on player sight
        /// </summary>
        public CellContent[][] mapInfo;
    }

    /// <summary>
    /// States of the GameSession
    /// </summary>
    [Serializable]
    public enum GameState
    {
        /// <summary>
        /// Lobby accepting players
        /// </summary>
        Lobby,

        /// <summary>
        /// Playing the game
        /// </summary>
        Game,

        /// <summary>
        /// Game finished, showing results
        /// </summary>
        Results
    }

    /// <summary>
    /// Contains all information about game session and manages it
    /// </summary>
    [Serializable]
    public class GameSession
    {
        #region VARIABLES

        /// <summary>
        /// List of players on GameSession
        /// </summary>
        private List<string> p_playerList;

        /// <summary>
        /// Players that have notified they still connected
        /// </summary>
        private List<string> p_connectedPlayers;

        /// <summary>
        /// Maximum number of players in GameSession
        /// </summary>
        private int p_maxPlayers = 2;

        /// <summary>
        /// Dictionary with player name as key and current position as value
        /// </summary>
        private Dictionary<string, int[]> p_playerPositions;

        private Dictionary<string, byte[]> p_playerAddresses;

        /// <summary>
        /// Bidimensional array with current map cells status
        /// </summary>
        private CellInfo[][] p_mapInfo;

        /// <summary>
        /// List of dead players
        /// </summary>
        private List<string> p_deadPlayers;

        /// <summary>
        /// Current GameState
        /// </summary>
        public GameState state;

        #endregion

        #region GETTERS_AND_SETTERS

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

        public Dictionary<string, int[]> getPlayerPositions
        {
            get
            {
                return p_playerPositions;
            }
        }

        #endregion

        /// <summary>
        /// GameSession constructor. Initializes player lists, set maximum number of players and creates chosen map
        /// </summary>
        /// <param name="i_maxPlayers">Maximum number of players</param>
        /// <param name="i_mapIndex">Chosen map index</param>
        public GameSession(int i_maxPlayers)
        {
            p_maxPlayers = i_maxPlayers;
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            int map = rnd.Next(3);
            int rotation = rnd.Next(4);
            int mirror = rnd.Next(3);
            switch (i_maxPlayers)
            {
                case 2:
                    p_mapInfo = new CellInfo[8][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        p_mapInfo[i] = new CellInfo[8];
                    }
                    switch (map)
                    {
                        case 0:
                            foreach (int[] pos in new int[][] { new int[] {2,1},
                                                                new int[] {5,2},
                                                                new int[] {4,4},
                                                                new int[] {1,6},
                                                                new int[] {7,7}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 1:
                            foreach (int[] pos in new int[][] { new int[] {2,0},
                                                                new int[] {6,0},
                                                                new int[] {4,1},
                                                                new int[] {1,3},
                                                                new int[] {4,4},
                                                                new int[] {7,4},
                                                                new int[] {2,6},
                                                                new int[] {6,6},
                                                                new int[] {0,7}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 2:
                            foreach (int[] pos in new int[][] { new int[] {3,0},
                                                                new int[] {7,0},
                                                                new int[] {0,1},
                                                                new int[] {2,2},
                                                                new int[] {5,2},
                                                                new int[] {0,3},
                                                                new int[] {7,3},
                                                                new int[] {6,4},
                                                                new int[] {2,5},
                                                                new int[] {1,6},
                                                                new int[] {5,6},
                                                                new int[] {0,7},
                                                                new int[] {1,7},
                                                                new int[] {7,7}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                    }
                    break;
                case 3:
                    p_mapInfo = new CellInfo[10][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        p_mapInfo[i] = new CellInfo[10];
                    }
                    switch (map)
                    {
                        case 0:
                            foreach (int[] pos in new int[][] { new int[] {1,0},
                                                                new int[] {8,1},
                                                                new int[] {4,2},
                                                                new int[] {1,5},
                                                                new int[] {7,5},
                                                                new int[] {5,6},
                                                                new int[] {4,8},
                                                                new int[] {8,9}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 1:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {4,0},
                                                                new int[] {7,1},
                                                                new int[] {4,3},
                                                                new int[] {2,4},
                                                                new int[] {9,4},
                                                                new int[] {6,5},
                                                                new int[] {1,6},
                                                                new int[] {5,7},
                                                                new int[] {8,7},
                                                                new int[] {0,9},
                                                                new int[] {3,9},
                                                                new int[] {6,9}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 2:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {2,0},
                                                                new int[] {6,0},
                                                                new int[] {6,1},
                                                                new int[] {2,2},
                                                                new int[] {7,2},
                                                                new int[] {0,3},
                                                                new int[] {3,4},
                                                                new int[] {6,4},
                                                                new int[] {2,5},
                                                                new int[] {7,5},
                                                                new int[] {1,6},
                                                                new int[] {5,6},
                                                                new int[] {8,7},
                                                                new int[] {9,7},
                                                                new int[] {5,8},
                                                                new int[] {0,9},
                                                                new int[] {1,9},
                                                                new int[] {4,9},
                                                                new int[] {9,9}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                    }
                    break;
                case 4:
                    p_mapInfo = new CellInfo[12][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        p_mapInfo[i] = new CellInfo[10];
                    }
                    switch (map)
                    {
                        case 0:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {0,7},
                                                                new int[] {1,3},
                                                                new int[] {2,8},
                                                                new int[] {4,2},
                                                                new int[] {4,6},
                                                                new int[] {5,4},
                                                                new int[] {9,2},
                                                                new int[] {9,7},
                                                                new int[] {10,0},
                                                                new int[] {10,9}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 1:
                            foreach (int[] pos in new int[][] { new int[] {0,4},
                                                                new int[] {0,7},
                                                                new int[] {0,9},
                                                                new int[] {1,9},
                                                                new int[] {2,0},
                                                                new int[] {2,9},
                                                                new int[] {3,2},
                                                                new int[] {3,6},
                                                                new int[] {5,5},
                                                                new int[] {7,0},
                                                                new int[] {7,8},
                                                                new int[] {8,3},
                                                                new int[] {9,6},
                                                                new int[] {10,8},
                                                                new int[] {11,0},
                                                                new int[] {11,9}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 2:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {0,2},
                                                                new int[] {0,4},
                                                                new int[] {0,7},
                                                                new int[] {0,9},
                                                                new int[] {1,5},
                                                                new int[] {1,9},
                                                                new int[] {2,3},
                                                                new int[] {3,6},
                                                                new int[] {3,8},
                                                                new int[] {5,0},
                                                                new int[] {5,4},
                                                                new int[] {5,7},
                                                                new int[] {5,9},
                                                                new int[] {6,1},
                                                                new int[] {7,1},
                                                                new int[] {7,5},
                                                                new int[] {7,6},
                                                                new int[] {8,3},
                                                                new int[] {8,9},                                                       new int[] {5,},
                                                                new int[] {10,2},
                                                                new int[] {10,5},
                                                                new int[] {10,7},
                                                                new int[] {10,9},
                                                                new int[] {11,0},
                                                                new int[] {11,3},
                                                                new int[] {11,9}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                    }
                    break;
                case 5:
                    p_mapInfo = new CellInfo[12][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        p_mapInfo[i] = new CellInfo[12];
                    }
                    switch (map)
                    {
                        case 0:
                            foreach (int[] pos in new int[][] { new int[] {0,1},
                                                                new int[] {0,9},
                                                                new int[] {0,11},
                                                                new int[] {2,2},
                                                                new int[] {2,4},
                                                                new int[] {2,8},
                                                                new int[] {3,7},
                                                                new int[] {4,0},
                                                                new int[] {4,1},
                                                                new int[] {4,11},
                                                                new int[] {5,4},
                                                                new int[] {6,9},
                                                                new int[] {8,1},
                                                                new int[] {8,5},
                                                                new int[] {8,7},
                                                                new int[] {10,2},
                                                                new int[] {10,10},
                                                                new int[] {11,7},
                                                                new int[] {11,11} })
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 1:
                            foreach (int[] pos in new int[][] { new int[] {0,3},
                                                                new int[] {0,7},
                                                                new int[] {0,8},
                                                                new int[] {1,0},
                                                                new int[] {1,10},
                                                                new int[] {2,1},
                                                                new int[] {2,5},
                                                                new int[] {2,11},
                                                                new int[] {3,2},
                                                                new int[] {4,4},
                                                                new int[] {4,5},
                                                                new int[] {6,10},
                                                                new int[] {7,7},
                                                                new int[] {8,2},
                                                                new int[] {8,5},
                                                                new int[] {8,8},
                                                                new int[] {9,2},
                                                                new int[] {9,11},
                                                                new int[] {10,1},
                                                                new int[] {11,0},
                                                                new int[] {11,3},
                                                                new int[] {11,6},
                                                                new int[] {11,8},
                                                                new int[] {11,11}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 2:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {0,2},
                                                                new int[] {0,6},
                                                                new int[] {0,11},
                                                                new int[] {1,0},
                                                                new int[] {1,3},
                                                                new int[] {1,7},
                                                                new int[] {1,10},
                                                                new int[] {3,0},
                                                                new int[] {3,4},
                                                                new int[] {3,6},
                                                                new int[] {3,11},
                                                                new int[] {4,2},
                                                                new int[] {4,3},
                                                                new int[] {4,6},
                                                                new int[] {4,11},
                                                                new int[] {5,0},
                                                                new int[] {5,5},
                                                                new int[] {5,9},
                                                                new int[] {6,8},
                                                                new int[] {6,11},
                                                                new int[] {7,0},
                                                                new int[] {7,3},
                                                                new int[] {7,7},
                                                                new int[] {7,9},
                                                                new int[] {8,3},
                                                                new int[] {8,5},
                                                                new int[] {9,0},
                                                                new int[] {9,8},
                                                                new int[] {9,11},
                                                                new int[] {10,0},
                                                                new int[] {10,1},
                                                                new int[] {10,2},
                                                                new int[] {10,5},
                                                                new int[] {11,0},
                                                                new int[] {11,4},
                                                                new int[] {11,8},
                                                                new int[] {11,9},
                                                                new int[] {11,10}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                    }
                    break;
                case 6:
                    p_mapInfo = new CellInfo[14][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        p_mapInfo[i] = new CellInfo[12];
                    }
                    switch (map)
                    {
                        case 0:
                            foreach (int[] pos in new int[][] { new int[] {0,11},
                                                                new int[] {1,0},
                                                                new int[] {1,5},
                                                                new int[] {1,8},
                                                                new int[] {2,2},
                                                                new int[] {2,10},
                                                                new int[] {3,6},
                                                                new int[] {5,5},
                                                                new int[] {6,3},
                                                                new int[] {7,11},
                                                                new int[] {8,0},
                                                                new int[] {8,7},
                                                                new int[] {10,9},
                                                                new int[] {11,4},
                                                                new int[] {12,6},
                                                                new int[] {12,10},
                                                                new int[] {13,2},
                                                                new int[] {13,10},
                                                                new int[] {13,11}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 1:
                            foreach (int[] pos in new int[][] { new int[] {0,1},
                                                                new int[] {0,5},
                                                                new int[] {0,11},
                                                                new int[] {1,3},
                                                                new int[] {1,5},
                                                                new int[] {1,7},
                                                                new int[] {1,9},
                                                                new int[] {2,2},
                                                                new int[] {4,0},
                                                                new int[] {4,6},
                                                                new int[] {4,11},
                                                                new int[] {5,2},
                                                                new int[] {5,10},
                                                                new int[] {6,4},
                                                                new int[] {7,5},
                                                                new int[] {7,7},
                                                                new int[] {7,8},
                                                                new int[] {8,0},
                                                                new int[] {9,2},
                                                                new int[] {9,10},
                                                                new int[] {10,1},
                                                                new int[] {11,0},
                                                                new int[] {11,4},
                                                                new int[] {11,6},
                                                                new int[] {12,8},
                                                                new int[] {12,11},
                                                                new int[] {13,0},
                                                                new int[] {13,4},
                                                                new int[] {13,9},
                                                                new int[] {13,10},
                                                                new int[] {13,11}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 2:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {0,3},
                                                                new int[] {0,4},
                                                                new int[] {0,11},
                                                                new int[] {1,7},
                                                                new int[] {1,10},
                                                                new int[] {2,1},
                                                                new int[] {2,8},
                                                                new int[] {3,0},
                                                                new int[] {3,2},
                                                                new int[] {3,5},
                                                                new int[] {3,9},
                                                                new int[] {3,11},
                                                                new int[] {4,7},
                                                                new int[] {5,3},
                                                                new int[] {5,4},
                                                                new int[] {5,7},
                                                                new int[] {6,0},
                                                                new int[] {6,1},
                                                                new int[] {6,6},
                                                                new int[] {7,3},
                                                                new int[] {7,8},
                                                                new int[] {8,11},
                                                                new int[] {9,1},
                                                                new int[] {9,4},
                                                                new int[] {9,6},
                                                                new int[] {9,10},
                                                                new int[] {9,11},
                                                                new int[] {10,0},
                                                                new int[] {10,3},
                                                                new int[] {10,11},
                                                                new int[] {11,2},
                                                                new int[] {11,6},
                                                                new int[] {11,9},
                                                                new int[] {12,6},
                                                                new int[] {12,8},
                                                                new int[] {13,0},
                                                                new int[] {13,4},
                                                                new int[] {13,6},
                                                                new int[] {13,8},
                                                                new int[] {13,11}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                    }
                    break;
                case 7:
                    p_mapInfo = new CellInfo[14][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        p_mapInfo[i] = new CellInfo[14];
                    }
                    switch (map)
                    {
                        case 0:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {0,10},
                                                                new int[] {0,12},
                                                                new int[] {0,13},
                                                                new int[] {1,5},
                                                                new int[] {2,3},
                                                                new int[] {2,7},
                                                                new int[] {3,0},
                                                                new int[] {3,5},
                                                                new int[] {3,9},
                                                                new int[] {3,12},
                                                                new int[] {5,10},
                                                                new int[] {5,13},
                                                                new int[] {6,0},
                                                                new int[] {6,5},
                                                                new int[] {7,1},
                                                                new int[] {7,9},
                                                                new int[] {8,6},
                                                                new int[] {8,9},
                                                                new int[] {10,3},
                                                                new int[] {11,1},
                                                                new int[] {11,8},
                                                                new int[] {11,13},
                                                                new int[] {12,0},
                                                                new int[] {12,12},
                                                                new int[] {13,0},
                                                                new int[] {13,6},
                                                                new int[] {13,10}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 1:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {0,6},
                                                                new int[] {0,7},
                                                                new int[] {0,11},
                                                                new int[] {0,13},
                                                                new int[] {1,4},
                                                                new int[] {1,9},
                                                                new int[] {2,3},
                                                                new int[] {2,8},
                                                                new int[] {2,11},
                                                                new int[] {2,13},
                                                                new int[] {3,1},
                                                                new int[] {3,6},
                                                                new int[] {3,13},
                                                                new int[] {4,0},
                                                                new int[] {5,1},
                                                                new int[] {5,5},
                                                                new int[] {5,8},
                                                                new int[] {5,12},
                                                                new int[] {7,5},
                                                                new int[] {8,4},
                                                                new int[] {8,5},
                                                                new int[] {8,10},
                                                                new int[] {9,5},
                                                                new int[] {9,9},
                                                                new int[] {9,10},
                                                                new int[] {9,13},
                                                                new int[] {10,7},
                                                                new int[] {11,0},
                                                                new int[] {11,3},
                                                                new int[] {11,8},
                                                                new int[] {11,13},
                                                                new int[] {12,0},
                                                                new int[] {12,1},
                                                                new int[] {13,0},
                                                                new int[] {13,1},
                                                                new int[] {13,2},
                                                                new int[] {13,6},
                                                                new int[] {13,10},
                                                                new int[] {13,13}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 2:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {0,6},
                                                                new int[] {0,9},
                                                                new int[] {0,13},
                                                                new int[] {1,1},
                                                                new int[] {1,2},
                                                                new int[] {1,5},
                                                                new int[] {1,8},
                                                                new int[] {1,9},
                                                                new int[] {1,10},
                                                                new int[] {1,11},
                                                                new int[] {1,13},
                                                                new int[] {2,11},
                                                                new int[] {3,6},
                                                                new int[] {3,8},
                                                                new int[] {4,0},
                                                                new int[] {4,3},
                                                                new int[] {4,13},
                                                                new int[] {5,0},
                                                                new int[] {5,1},
                                                                new int[] {5,3},
                                                                new int[] {5,7},
                                                                new int[] {5,10},
                                                                new int[] {6,0},
                                                                new int[] {6,4},
                                                                new int[] {6,11},
                                                                new int[] {7,5},
                                                                new int[] {7,7},
                                                                new int[] {7,9},
                                                                new int[] {7,10},
                                                                new int[] {7,12},
                                                                new int[] {8,4},
                                                                new int[] {8,8},
                                                                new int[] {9,1},
                                                                new int[] {9,2},
                                                                new int[] {9,11},
                                                                new int[] {9,13},
                                                                new int[] {10,2},
                                                                new int[] {10,6},
                                                                new int[] {10,10},
                                                                new int[] {11,4},
                                                                new int[] {11,5},
                                                                new int[] {11,7},
                                                                new int[] {12,0},
                                                                new int[] {12,10},
                                                                new int[] {12,13},
                                                                new int[] {13,0},
                                                                new int[] {13,1},
                                                                new int[] {13,3},
                                                                new int[] {13,7},
                                                                new int[] {13,12},
                                                                new int[] {13,13}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                    }
                    break;
                case 8:
                    p_mapInfo = new CellInfo[16][];
                    for (int i = 0; i < p_mapInfo.Length; i++)
                    {
                        p_mapInfo[i] = new CellInfo[14];
                    }
                    switch (map)
                    {
                        case 0:
                            foreach (int[] pos in new int[][] { new int[] {0,4},
                                                                new int[] {0,13},
                                                                new int[] {1,3},
                                                                new int[] {1,8},
                                                                new int[] {1,12},
                                                                new int[] {2,1},
                                                                new int[] {3,0},
                                                                new int[] {3,6},
                                                                new int[] {4,0},
                                                                new int[] {4,6},
                                                                new int[] {4,9},
                                                                new int[] {4,11},
                                                                new int[] {5,3},
                                                                new int[] {6,7},
                                                                new int[] {6,9},
                                                                new int[] {6,12},
                                                                new int[] {7,6},
                                                                new int[] {8,2},
                                                                new int[] {8,12},
                                                                new int[] {9,3},
                                                                new int[] {9,12},
                                                                new int[] {10,9},
                                                                new int[] {11,5},
                                                                new int[] {11,13},
                                                                new int[] {12,0},
                                                                new int[] {12,10},
                                                                new int[] {13,3},
                                                                new int[] {13,6},
                                                                new int[] {13,12},
                                                                new int[] {14,0},
                                                                new int[] {14,9},
                                                                new int[] {14,13},
                                                                new int[] {15,4},
                                                                new int[] {15,8},
                                                                new int[] {15,13}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 1:
                            foreach (int[] pos in new int[][] { new int[] {0,1},
                                                                new int[] {0,3},
                                                                new int[] {0,9},
                                                                new int[] {0,12},
                                                                new int[] {0,13},
                                                                new int[] {1,1},
                                                                new int[] {1,7},
                                                                new int[] {1,8},
                                                                new int[] {2,5},
                                                                new int[] {2,11},
                                                                new int[] {3,0},
                                                                new int[] {3,3},
                                                                new int[] {4,2},
                                                                new int[] {4,8},
                                                                new int[] {4,12},
                                                                new int[] {5,3},
                                                                new int[] {5,12},
                                                                new int[] {6,1},
                                                                new int[] {6,7},
                                                                new int[] {6,13},
                                                                new int[] {7,4},
                                                                new int[] {7,10},
                                                                new int[] {8,6},
                                                                new int[] {8,8},
                                                                new int[] {9,2},
                                                                new int[] {10,4},
                                                                new int[] {10,6},
                                                                new int[] {10,11},
                                                                new int[] {11,0},
                                                                new int[] {11,3},
                                                                new int[] {11,13},
                                                                new int[] {12,3},
                                                                new int[] {12,9},
                                                                new int[] {12,12},
                                                                new int[] {13,6},
                                                                new int[] {13,8},
                                                                new int[] {14,4},
                                                                new int[] {14,13},
                                                                new int[] {15,0},
                                                                new int[] {15,6},
                                                                new int[] {15,12},
                                                                new int[] {15,13}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                        case 2:
                            foreach (int[] pos in new int[][] { new int[] {0,0},
                                                                new int[] {0,1},
                                                                new int[] {0,2},
                                                                new int[] {0,4},
                                                                new int[] {0,6},
                                                                new int[] {0,7},
                                                                new int[] {0,13},
                                                                new int[] {1,0},
                                                                new int[] {1,11},
                                                                new int[] {1,12},
                                                                new int[] {1,13},
                                                                new int[] {2,0},
                                                                new int[] {2,1},
                                                                new int[] {2,4},
                                                                new int[] {2,7},
                                                                new int[] {2,9},
                                                                new int[] {2,13},
                                                                new int[] {3,2},
                                                                new int[] {3,8},
                                                                new int[] {4,2},
                                                                new int[] {4,5},
                                                                new int[] {4,9},
                                                                new int[] {4,12},
                                                                new int[] {5,0},
                                                                new int[] {5,4},
                                                                new int[] {5,8},
                                                                new int[] {5,9},
                                                                new int[] {5,11},
                                                                new int[] {6,6},
                                                                new int[] {7,1},
                                                                new int[] {7,3},
                                                                new int[] {7,5},
                                                                new int[] {7,10},
                                                                new int[] {7,12},
                                                                new int[] {7,13},
                                                                new int[] {8,5},
                                                                new int[] {8,8},
                                                                new int[] {9,2},
                                                                new int[] {9,6},
                                                                new int[] {9,10},
                                                                new int[] {9,12},
                                                                new int[] {10,4},
                                                                new int[] {10,7},
                                                                new int[] {10,13},
                                                                new int[] {11,0},
                                                                new int[] {11,11},
                                                                new int[] {12,3},
                                                                new int[] {12,6},
                                                                new int[] {12,9},
                                                                new int[] {13,1},
                                                                new int[] {13,11},
                                                                new int[] {13,13},
                                                                new int[] {14,0},
                                                                new int[] {14,4},
                                                                new int[] {14,7},
                                                                new int[] {14,9},
                                                                new int[] {15,0},
                                                                new int[] {15,1},
                                                                new int[] {15,5},
                                                                new int[] {15,7},
                                                                new int[] {15,12},
                                                                new int[] {15,13}})
                            {
                                p_mapInfo[pos[0]][pos[1]] = new CellInfo();
                            }
                            break;
                    }
                    break;
            }
            for (int i = 0; i < rotation; i++)
                RotateMap();
            MirrorMap(mirror);
            p_playerList = new List<string>();
            p_connectedPlayers = new List<string>();
            p_playerPositions = new Dictionary<string, int[]>();
            p_playerAddresses = new Dictionary<string, byte[]>();
            p_deadPlayers = new List<string>();
            state = GameState.Lobby;
        }

        public void RotateMap()
        {
            int length = p_mapInfo[0].Length;
            CellInfo[][] res = new CellInfo[length][];
            for (int x = 0; x < length; x++)
            {
                res[x] = p_mapInfo.Select(p => p[x]).Reverse().ToArray();
            }
            p_mapInfo = res;
        }

        public void MirrorMap(int i_type)
        {
            if (i_type == 1)
            {
                p_mapInfo = p_mapInfo.Reverse().ToArray();
            }
            else if (i_type == 2)
            {
                for (int i = 0; i < p_mapInfo.Length; i++)
                {
                    p_mapInfo[i] = p_mapInfo[i].Reverse().ToArray();
                }
            }
        }

        /// <summary>
        /// Tries to add player to game
        /// </summary>
        /// <param name="i_player">Player to add</param>
        /// <returns>True if player could be added, false otherwise</returns>
        public bool AddPlayer(string i_player, byte[] i_address)
        {
            //If game is in LobbyState and number of players is lower than max
            if (state.Equals(GameState.Lobby) && p_playerList.Count < p_maxPlayers)
            {
                //Adds player to player list
                p_playerList.Add(i_player);
                //Adds player to list of players that still playing
                p_connectedPlayers.Add(i_player);

                p_playerAddresses.Add(i_player, i_address);

                //Player added correctly
                return true;
            }
            //otherwise
            return false;
        }

        /// <summary>
        /// Adds player to list of players who still playing the game (or waiting in the lobby)
        /// </summary>
        /// <param name="i_player">Player that stills playing</param>
        public void StillPlaying(string i_player)
        {
            //If player isn't on the list of players that still playing
            if (!p_connectedPlayers.Contains(i_player))
                //Adds it to the list
                p_connectedPlayers.Add(i_player);
        }

        //ADD WHILE PLAYING??
        /// <summary>
        /// Removes player from game
        /// </summary>
        /// <param name="i_player">Player to remove</param>
        public void RemovePlayer(string i_player)
        {
            //Removes player from list of players that still playing
            p_connectedPlayers.Remove(i_player);

            p_playerAddresses.Remove(i_player);

            //If game state is Lobby
            if (state.Equals(GameState.Lobby))
                //Removes player from player list
                p_playerList.Remove(i_player);
            //Else if player wasn't dead
            else if (!p_deadPlayers.Contains(i_player))
                //Adds player to dead players list
                p_deadPlayers.Add(i_player);
        }

        /// <summary>
        /// Checks which players still playing
        /// </summary>
        /// <returns>List of players that are not playing</returns>
        public List<string> CheckPlayers()
        {
            //Creates list of players to remove
            List<string> removedPlayers = new List<string>();
            //For each player in game
            foreach (string player in p_playerList)
            {
                //If player stills playing
                if (p_connectedPlayers.Contains(player))
                {
                    //Removes player from list of players that still playing
                    p_connectedPlayers.Remove(player);
                }
                //otherwise
                else
                {
                    //Adds player to list of players to remove
                    removedPlayers.Add(player);
                }
            }
            //Returns list of players to remove
            return removedPlayers;
        }

        //MODIFY IT ADDING PLAYER SPAWN POINTS. DON'T CREATE THE MAP HERE
        /// <summary>
        /// Prepares game for start playing
        /// </summary>
        public void PrepareGame()
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            foreach (string player in p_playerList)
            {
                bool success = false;
                do
                {
                    int xPos = rnd.Next(p_mapInfo.Length);
                    int yPos = rnd.Next(p_mapInfo[0].Length);
                    if (p_mapInfo[xPos][yPos] == null)
                    {
                        p_mapInfo[xPos][yPos] = new CellInfo(player);
                        p_playerPositions.Add(player, new int[] { xPos, yPos });
                        success = true;
                    }
                }
                while (!success);
            }
        }

        /// <summary>
        /// Moves the player and notifies of their movemente consecuences
        /// </summary>
        /// <param name="i_dir">Movement vector</param>
        /// <param name="i_player">Player to move</param>
        public MovementResult MovePlayer(int[] i_dir, string i_player)
        {
            MovementResult res = new MovementResult();
            //If player is connected
            if (p_playerList.Contains(i_player))
            {
                res.type = MovementResultType.Nothing;
                if (!p_deadPlayers.Contains(i_player))
                {
                    //Gets current player position vector
                    int[] playerPos = p_playerPositions[i_player];
                    //Calculates player destination position vector
                    int[] destPos = new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] };
                    //True if player position is out of map bounds
                    bool outOfBounds = (destPos[0] < 0 || destPos[0] >= p_mapInfo.Length || destPos[1] < 0 || destPos[1] >= p_mapInfo[0].Length);
                    //Gets player destination position cell info if player is not out of bounds
                    CellInfo destPosInfo = outOfBounds ? null : p_mapInfo[destPos[0]][destPos[1]];
                    //If player is out of bounds or destination cell is a hole
                    if (outOfBounds || (destPosInfo != null && destPosInfo.content.Equals(CellContent.Hole)))
                    {
                        //Adds player to dead players list
                        p_deadPlayers.Add(i_player);
                        //Eliminates player from original cell
                        p_mapInfo[playerPos[0]][playerPos[1]] = null;
                        //Adds player position and movement consecuences to result object
                        res.playerPos = new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] };
                        res.type = MovementResultType.PlayerDied;
                    }
                    //otherwise
                    else
                    {
                        //If destination cell has a player
                        if (destPosInfo != null && destPosInfo.content.Equals(CellContent.Player))
                        {
                            //Adds dead player to result object
                            res.killedPlayer = destPosInfo.playerId;
                            //Adds player to dead players list
                            p_deadPlayers.Add(res.killedPlayer);
                            //Adds player death position and movement consecuences to result object
                            res.playerPos = new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] };
                            res.type = MovementResultType.KilledPlayer;
                        }
                        //Moves player to destination position
                        p_mapInfo[playerPos[0] + i_dir[0]][playerPos[1] + i_dir[1]] = p_mapInfo[playerPos[0]][playerPos[1]];
                        //Eliminates player from original position
                        p_mapInfo[playerPos[0]][playerPos[1]] = null;
                        //Removes player position register
                        p_playerPositions.Remove(i_player);
                        //Adds a new player position register
                        p_playerPositions.Add(i_player, new int[] { playerPos[0] + i_dir[0], playerPos[1] + i_dir[1] });
                    }
                }
            }
            //Otherwise
            else
            {
                //Adds PlayerNotConnected as result type
                res.type = MovementResultType.PlayerNotConnected;
            }
            //Returns result object
            return res;
        }

        /// <summary>
        /// Manages player attack and notifies where attacked the player and which players killed
        /// </summary>
        /// <param name="i_player">Player that attacks</param>
        /// <param name="i_attackRange">Range area of the attack</param>
        public AttackResult PlayerAttacks(string i_player, int i_attackRange)
        {
            AttackResult res = new AttackResult();
            res.hitPoints = new List<int[]>();
            res.killedPlayersDict = new Dictionary<string, int[]>();
            //If player is connected
            if (p_playerList.Contains(i_player))
            {
                //Sets attack as succeeded
                res.success = true;
                //If player is not dead
                if (!p_deadPlayers.Contains(i_player))
                {
                    //Covers all attack positions
                    for (int i = -i_attackRange; i <= i_attackRange; i++)
                    {
                        for (int j = -(i_attackRange - Math.Abs(i)); j <= i_attackRange - Math.Abs(i); j++)
                        {
                            //Sets hit position vector
                            int[] hitPos = new int[] { p_playerPositions[i_player][0] + i, p_playerPositions[i_player][1] + j };
                            //True if hit position if ot of map bounds
                            bool outOfBounds = (hitPos[0] < 0 || hitPos[0] >= p_mapInfo.Length || hitPos[1] < 0 || hitPos[1] >= p_mapInfo[0].Length);
                            //If hit position is not out of bounds and has a player that is not attacking player
                            if (!outOfBounds && p_mapInfo[hitPos[0]][hitPos[1]] != null && p_mapInfo[hitPos[0]][hitPos[1]].content.Equals(CellContent.Player) && !p_mapInfo[hitPos[0]][hitPos[1]].playerId.Equals(i_player))
                            {
                                //Gets dead player name
                                string deadPlayer = p_mapInfo[p_playerPositions[i_player][0] + i][p_playerPositions[i_player][1] + j].playerId;
                                //Eliminates dead player from hit position
                                p_mapInfo[p_playerPositions[i_player][0] + i][p_playerPositions[i_player][1] + j] = null;
                                //Adds dead player to dead players list
                                p_deadPlayers.Add(deadPlayer);
                                //Adds dead player and hit position to killed players dictionary
                                res.killedPlayersDict.Add(deadPlayer, hitPos);
                            }
                            //Otherwise
                            else
                            {
                                //Adds hit position to hit points list
                                res.hitPoints.Add(hitPos);
                            }
                        }
                    }
                }
            }
            //otherwise
            else
            {
                //Attack not succedeed
                res.success = false;
            }
            return res;
        }

        /// <summary>
        /// Manages player attack and notifies where attacked the player and which players killed
        /// </summary>
        /// <param name="i_player">Player that attacks</param>
        /// <param name="i_attackRange">Range area of the attack</param>
        public AttackResult TurretAttacks(int[] i_aimPos, int i_attackRange)
        {
            AttackResult res = new AttackResult();
            res.hitPoints = new List<int[]>();
            res.killedPlayersDict = new Dictionary<string, int[]>();
            //Covers all attack positions
            for (int i = -i_attackRange; i <= i_attackRange; i++)
            {
                for (int j = -(i_attackRange - Math.Abs(i)); j <= i_attackRange - Math.Abs(i); j++)
                {
                    //Sets hit position vector
                    int[] hitPos = new int[] { i_aimPos[0] + i, i_aimPos[1] + j };
                    //True if hit position if ot of map bounds
                    bool outOfBounds = (hitPos[0] < 0 || hitPos[0] >= p_mapInfo.Length || hitPos[1] < 0 || hitPos[1] >= p_mapInfo[0].Length);
                    //If hit position is not out of bounds and has a player that is not attacking player
                    if (!outOfBounds && p_mapInfo[hitPos[0]][hitPos[1]] != null && p_mapInfo[hitPos[0]][hitPos[1]].content.Equals(CellContent.Player))
                    {
                        //Gets dead player name
                        string deadPlayer = p_mapInfo[hitPos[0]][hitPos[1]].playerId;
                        //Eliminates dead player from hit position
                        p_mapInfo[hitPos[0]][hitPos[1]] = null;
                        //Adds dead player to dead players list
                        p_deadPlayers.Add(deadPlayer);
                        //Adds dead player and hit position to killed players dictionary
                        res.killedPlayersDict.Add(deadPlayer, hitPos);
                    }
                    //Otherwise
                    else
                    {
                        //Adds hit position to hit points list
                        res.hitPoints.Add(hitPos);
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Manages player's radar
        /// </summary>
        /// <param name="i_player">Player that used radar</param>
        /// <returns>Map info for this player if succeeded</returns>
        public RadarResult RadarActivated(string i_player)
        {
            RadarResult res = new RadarResult();
            //If player is connected
            if (p_playerList.Contains(i_player))
            {
                //Action succeeded
                res.success = true;
                //If player is not dead
                if (!p_deadPlayers.Contains(i_player))
                {
                    //Initializes map info
                    res.mapInfo = new CellContent[5][];
                    for (int i = 0; i < 5; i++)
                    {
                        res.mapInfo[i] = new CellContent[5];
                    }
                    //Gets player real position
                    int[] playerPosReal = p_playerPositions[i_player];
                    //Gets player virtual position
                    int[] playerPosVirt = new int[] { (int)Math.Floor(res.mapInfo.Length / 2f), (int)Math.Floor(res.mapInfo[0].Length / 2f) };
                    //Covers all cells on player sight
                    for (int i = 0; i < res.mapInfo.Length; i++)
                    {
                        for (int j = 0; j < res.mapInfo[0].Length; j++)
                        {
                            //If cell is not player cell
                            if (i != playerPosVirt[0] || j != playerPosVirt[1])
                            {
                                //Gets cell real pos
                                int[] realPos = new int[] { playerPosReal[0] - playerPosVirt[0] + i, playerPosReal[1] - playerPosVirt[1] + j };
                                //If cell pos is out of map bounds
                                if ((realPos[0] < 0) || (realPos[0] >= p_mapInfo.Length) || (realPos[1] < 0) || (realPos[1] >= p_mapInfo[0].Length))
                                {
                                    //Cell content is a hole
                                    res.mapInfo[i][j] = CellContent.Hole;
                                }
                                //otherwise
                                else
                                {
                                    //If cell info is empty, cell content is Floor; cell info is copied otherwise;
                                    res.mapInfo[i][j] = p_mapInfo[realPos[0]][realPos[1]] == null ? CellContent.Floor : p_mapInfo[realPos[0]][realPos[1]].content;
                                }
                            }
                        }
                    }
                }
            }
            //Otherwise
            else
            {
                //Action not succeeded
                res.success = false;
            }
            //Returns result object
            return res;
        }

        /// <summary>
        /// Gets player position vector
        /// </summary>
        /// <param name="i_player">Player whose position will be returned</param>
        public int[] GetPlayerPos(string i_player)
        {
            return p_playerPositions[i_player];
        }

        /// <summary>
        /// Gets a list with all alive players
        /// </summary>
        public List<string> AlivePlayers()
        {
            List<string> res = new List<string>();
            foreach (string player in p_playerList)
            {
                if (!p_deadPlayers.Contains(player))
                {
                    res.Add(player);
                }
            }
            return res;
        }

        public int[] GetTurretAimPos(int i_attackRange)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            int attackPosX = rnd.Next(0 - i_attackRange, p_mapInfo.Length + i_attackRange);
            int attackPosY;
            if (attackPosX < 0)
                attackPosY = rnd.Next(0 - attackPosX - i_attackRange, p_mapInfo[0].Length + attackPosX + i_attackRange);
            else if (attackPosX > p_mapInfo.Length)
                attackPosY = rnd.Next(0 + attackPosX - p_mapInfo.Length - 1 - i_attackRange, p_mapInfo[0].Length - attackPosX + p_mapInfo.Length - 1 + i_attackRange);
            else
                attackPosY = rnd.Next(0 - i_attackRange, p_mapInfo[0].Length + i_attackRange);
            return new int[] { attackPosX, attackPosY };
        }

        public byte[] GetPlayerAddress(string i_player)
        {
            return p_playerAddresses[i_player];
        }
    }
}
