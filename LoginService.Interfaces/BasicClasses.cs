namespace LoginService.Interfaces.BasicClasses
{
    /// <summary>
    /// Contains the definition of a game session
    /// </summary>
    public class GameDefinition
    {
        /// <summary>
        /// Name of the game session
        /// </summary>
        public string id;

        /// <summary>
        /// Maximum number of players in the game session
        /// </summary>
        public int maxPlayers;

        /// <summary>
        /// Number of players in the game session
        /// </summary>
        public int players;

        /// <summary>
        /// Map ID of the game session
        /// </summary>
        public int map;

        /// <summary>
        /// Empty constructor
        /// </summary>
        public GameDefinition()
        {
        }

        /// <summary>
        /// Creates a new GameDefinition
        /// </summary>
        /// <param name="i_gameId">ID of the game session</param>
        /// <param name="i_maxPlayers">Maximim number of players in the game session</param>
        public GameDefinition(string i_gameId, int i_maxPlayers)
        {
            id = i_gameId;
            maxPlayers = i_maxPlayers;
        }
    }
}
