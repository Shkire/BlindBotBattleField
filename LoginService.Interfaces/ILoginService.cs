﻿using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System;
using BasicClasses.Common;
using BasicClasses.LoginService;

namespace LoginService.Interfaces
{
    /// <summary>
    /// LoginService method declaration. Allows comunication with LoginService from client or other services
    /// </summary>
    public interface ILoginService: IService
    {
        /// <summary>
        /// Tries to log in player to the server
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <param name="i_pass">Player password</param>
        /// <returns>True if player was able to be logged in, false otherwise</returns>
        Task<ServerResponseInfo<bool,SqlException>> Login(string i_player, string i_pass);

        /// <summary>
        /// Creates a new player on the server
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <param name="i_pass">Player password</param>
        /// <returns>True if player was able to be created, false otherwise</returns>
        Task<ServerResponseInfo<bool,SqlException>> CreatePlayer(string i_player, string i_pass);

        /// <summary>
        /// Creates a new game session on the server
        /// </summary>
        /// <param name="i_gameDef">Game session definition</param>
        /// <returns>True if game session was able to be created, false otherwise</returns>
        Task<ServerResponseInfo<bool,Exception>> CreateGameAsync(string i_gameId, int i_maxPlayers);

        /// <summary>
        /// Increases in 1 the player counter of the game session (SQL register)
        /// </summary>
        /// <param name="i_gameId">Game session ID</param>
        Task AddPlayerAsync(string i_gameId);

        /// <summary>
        /// Decreases in 1 the player counter of the game session (SQL register)
        /// </summary>
        /// <param name="i_gameId">Game session ID</param>
        Task RemovePlayerAsync(string i_gameId);

        /// <summary>
        /// Deletes a game session on the server (SQL register)
        /// </summary>
        /// <param name="i_gameId">Game session ID</param>
        Task DeleteGameAsync(string i_gameId, string i_uri);

        /// <summary>
        /// Returns a list with all game session definitions on the server
        /// </summary>
        Task<ServerResponseInfo<bool,SqlException,List<GameDefinition>>> GetGameList();
    }
}
