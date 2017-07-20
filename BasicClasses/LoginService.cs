namespace BasicClasses.LoginService
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
    }
}
