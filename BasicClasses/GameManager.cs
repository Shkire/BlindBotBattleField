using System;

namespace BasicClasses.GameManager
{
    /// <summary>
    /// Type of content on a map cell
    /// </summary>
    [Serializable]
    public enum CellContent
    {
        /// <summary>
        /// Unknown content
        /// </summary>
        None,

        /// <summary>
        /// Navigable floor
        /// </summary>
        Floor,

        /// <summary>
        /// Hole, kills player
        /// </summary>
        Hole,

        /// <summary>
        /// Player
        /// </summary>
        Player,

        /// <summary>
        /// Dead player
        /// </summary>
        Dead,

        /// <summary>
        /// Bomb hit
        /// </summary>
        Hit,
        Aiming
    }



    /// <summary>
    /// Contains information of a map cell content
    /// </summary>
    [Serializable]
    public class CellInfo
    {
        /// <summary>
        /// Type of content
        /// </summary>
        private CellContent p_content;

        /// <summary>
        /// Player ID if the content is a player
        /// </summary>
        private string p_playerId;

        /// <summary>
        /// Getter for cell content
        /// </summary>
        public CellContent content
        {
            get
            {
                return p_content;
            }
        }

        /// <summary>
        /// Getter for player ID
        /// </summary>
        public string playerId
        {
            get
            {
                return p_playerId;
            }
        }

        /// <summary>
        /// Creates a CellInfo with a player as content
        /// </summary>
        /// <param name="i_playerId">ID of the player</param>
        public CellInfo(string i_playerId)
        {
            p_content = CellContent.Player;
            p_playerId = i_playerId;
        }

        /// <summary>
        /// Creates a CellInfo with a hole as content
        /// </summary>
        public CellInfo()
        {
            p_content = CellContent.Hole;
        }
    }

    /// <summary>
    /// Possible reasons for a player death
    /// </summary>
    public enum DeathReason
    {
        /// <summary>
        /// Player fell into a hole
        /// </summary>
        Hole,

        /// <summary>
        /// Player was hit by other player bomb
        /// </summary>
        PlayerHit,

        /// <summary>
        /// Player was smashed by other player
        /// </summary>
        PlayerSmash,

        /// <summary>
        /// Player was hit by a turret bomb
        /// </summary>
        Turret,

        /// <summary>
        /// Player disconnected from game session
        /// </summary>
        Disconnect
    }
}
