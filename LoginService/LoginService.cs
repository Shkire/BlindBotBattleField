using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using LoginService.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using GameManagerActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using System.Threading;
using BasicClasses.Common;
using BasicClasses.LoginService;

namespace LoginService
{
    /// <summary>
    /// El runtime de Service Fabric crea una instancia de esta clase para cada instancia del servicio.
    /// </summary>
    internal sealed class LoginService : StatelessService, ILoginService
    {
        SqlConnectionStringBuilder builder;

        public LoginService(StatelessServiceContext context)
            : base(context)
        {
            builder = new SqlConnectionStringBuilder();
            builder.DataSource = "blindbotbattlefield.database.windows.net";
            builder.UserID = "radar";
            builder.Password = "blindHole17";
            builder.InitialCatalog = "PlayersAndGames";
        }

        /// <summary>
        /// Reemplazo opcional para crear agentes de escucha (por ejemplo, TCP, HTTP) para que esta réplica de servicio controle las solicitudes de cliente o usuario.
        /// </summary>
        /// <returns>Una colección de agentes de escucha.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(context =>
            this.CreateServiceRemotingListener(context)) };
        }

        /// <summary>
        /// Tries to log in player to the server
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <param name="i_pass">Player password</param>
        /// <returns>True if player was able to be logged in, false otherwise</returns>
        public Task<ServerResponseInfo<bool,SqlException>> Login(string i_player, string i_pass)
        {
            ServerResponseInfo<bool,SqlException> res = new ServerResponseInfo<bool, SqlException>();
            try
            {
                //Connects to the SQL Server and close when exiting
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    //Opens SQL connection
                    connection.Open();
                    //Creates query string
                    string query = "SELECT Pass FROM Players WHERE Player = '" + i_player + "'";
                    //Creates SQL Command using query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Creates SQL reader and executes query
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //If data to read
                            if (reader.Read())
                            {
                                //Returns if pass equals to pass read from SQL
                                res.info = reader.GetString(0).Equals(i_pass) ? true : false;
                                return Task.FromResult(res);
                            }
                            else
                                //If no data (no player with name "i_player" in SQL Server) returns false
                                res.info = false;
                                return Task.FromResult(res);
                        }
                    }
                }
            }
            //If SQL exception caught
            catch (SqlException e)
            {
                //Returns false
                res.info = false;
                res.exception = e;
                return Task.FromResult(res);
            }
        }

        /// <summary>
        /// Creates a new player on the server
        /// </summary>
        /// <param name="i_player">Player name</param>
        /// <param name="i_pass">Player password</param>
        /// <returns>True if player was able to be created, false otherwise</returns>
        public Task<ServerResponseInfo<bool,SqlException>> CreatePlayer(string i_player, string i_pass)
        {
            ServerResponseInfo<bool, SqlException> res = new ServerResponseInfo<bool, SqlException>();
            try
            {
                //Connects to the SQL Server and close when exiting
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    //Opens SQL connection
                    connection.Open();
                    //Creates query string
                    string query = "INSERT INTO Players VALUES('" + i_player + "','"+i_pass+"',0)";
                    //Creates SQL Command using query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Executes command
                        command.ExecuteNonQuery();
                        //Returns true
                        res.info = true;
                        return Task.FromResult(res);
                    }
                }
            }
            //If SQL Exception caught
            catch (SqlException e)
            {
                //Returns false and exception
                res.info = false;
                res.exception = e;
                return Task.FromResult(res);
            }
        }

        /// <summary>
        /// Creates a new game session on the server
        /// </summary>
        /// <param name="i_gameDef">Game session definition</param>
        /// <returns>True if game session was able to be created, false otherwise</returns>
        public async Task<ServerResponseInfo<bool,Exception>> CreateGameAsync(string i_gameId, int i_maxPlayers)
        {
            ServerResponseInfo<bool,Exception> res = new ServerResponseInfo<bool, Exception>();
            try
            {
                //Connects to the SQL Server and close when exiting
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    //Opens SQL connection
                    connection.Open();
                    //Creates query string
                    string query = "INSERT INTO Games VALUES('" + i_gameId + "'," + i_maxPlayers + ","+0+")";
                    //Creates SQL Command using query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Executes command
                        command.ExecuteNonQuery();
                    }
                }
                //Creates game actor
                IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(i_gameId));
                //Initializes game actor
                await actor.InitializeGameAsync(i_maxPlayers);
                //Returns true
                res.info = true;
                return res;
            }
            //If exception caught
            catch (Exception e)
            {
                //REMOVE GAME

                //Returns false with exception
                res.info = false;
                res.exception = e;
                return res;
            }
        }

        //RETURN VALUE??
        /// <summary>
        /// Increases in 1 the player counter of the game session (SQL register)
        /// </summary>
        /// <param name="i_gameId">Game session ID</param>
        public async Task AddPlayerAsync(string i_gameId)
        {
            try
            {
                //Connects to the SQL Server and close when exiting
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    //Sets player count to 0
                    int players = 0;
                    //Opens SQL connection
                    connection.Open();
                    //Creates query string
                    string query = "SELECT Players FROM Games WHERE Id = '" + i_gameId + "'";
                    //Creates SQL Command using query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Creates SQL reader and executes query
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //If data to read
                            if (reader.Read())
                            {
                                //Gets player count
                                players = reader.GetInt32(0);
                            }
                        }
                    }
                    //Increases player count
                    players++;
                    //Creates query string
                    query = "UPDATE Games SET Players = " + players + " WHERE Id = '" + i_gameId + "'";
                    //Creates SQL Command using query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Executes command
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException e)
            {
            }
        }

        //RETURN VALUE??
        /// <summary>
        /// Decreases in 1 the player counter of the game session (SQL register)
        /// </summary>
        /// <param name="i_gameId">Game session ID</param>
        public async Task RemovePlayerAsync(string i_gameId)
        {
            try
            {
                //Connects to the SQL Server and close when exiting
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    //Sets player count to 0
                    int players = 0;
                    //Opens SQL connection
                    connection.Open();
                    //Creates query string
                    string query = "SELECT Players FROM Games WHERE Id = '" + i_gameId + "'";
                    //Creates SQL Command using query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Creates SQL reader and executes query
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //If data to read
                            if (reader.Read())
                            {
                                //Gets player count
                                players = reader.GetInt32(0);
                            }
                        }
                    }
                    //Decreases player count
                    players--;
                    //Creates query string
                    query = "UPDATE Games SET Players = " + players + " WHERE Id = '" + i_gameId + "'";
                    //Creates SQL Command using query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Executes command
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException e)
            {
            }
        }

        //RETURN VALUE??
        /// <summary>
        /// Deletes a game session on the server (SQL register)
        /// </summary>
        /// <param name="i_gameId">Game session ID</param>
        public async Task DeleteGameAsync(string i_gameId, string i_uri)
        {
            try
            {
                IActorService actor = ActorServiceProxy.Create(new Uri(i_uri),new ActorId(i_gameId));
                await actor.DeleteActorAsync(new ActorId(i_gameId),CancellationToken.None);
                //Connects to the SQL Server and close when exiting
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    //Opens SQL connection
                    connection.Open();
                    //Creates query string
                    string query = "DELETE FROM Games WHERE Id ='" + i_gameId + "'";
                    //Creates SQL Command using query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Executes command
                        command.ExecuteNonQuery();
                    }
                };
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Returns a list with all game session definitions on the server
        /// </summary>
        public Task<ServerResponseInfo<bool, SqlException, List<GameDefinition>>> GetGameList()
        {
            ServerResponseInfo<bool, SqlException, List<GameDefinition>> res = new ServerResponseInfo<bool, SqlException, List<GameDefinition>>();
            List<GameDefinition> gameList = new List<GameDefinition>();
            try
            {
                //Connects to the SQL Server and close when exiting
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    //Opens SQL connection
                    connection.Open();
                    //Creates query string
                    string query = "SELECT * FROM Games;";
                    //Creates SQL Command using query and connection
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        //Creates SQL reader and executes query
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //While data to read
                            while (reader.Read())
                            {
                                //Creates new GameDefinition adds data and adds it to gameList
                                GameDefinition gameInfo = new GameDefinition();
                                gameInfo.id = reader.GetString(0);
                                gameInfo.maxPlayers = reader.GetInt32(1);
                                gameInfo.players = reader.GetInt32(2);
                                gameList.Add(gameInfo);
                            }
                        }
                    }
                }
                //Returns true and gameList
                res.info = true;
                res.additionalInfo = gameList;
                return Task.FromResult(res);
            }
            //If exception caught
            catch (SqlException e)
            {
                //Returns false ande exception
                res.info = false;
                res.exception = e;
                return Task.FromResult(res);
            }
        }
    }
}
