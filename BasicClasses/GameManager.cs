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
