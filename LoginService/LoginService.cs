using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Text;
using LoginService.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using GameManagerActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;

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


        public  Task<bool> Login(string i_player, string i_pass)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT Pass FROM Players WHERE Pass = '"+i_player+"'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader.GetString(0).Equals(i_pass) ? Task.FromResult(true) : Task.FromResult(false);
                            }
                            else
                                return Task.FromResult(false);
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> CreatePlayer(string i_player, string i_pass)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Players VALUES('" + i_player + "','"+i_pass+"',0)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        return Task.FromResult(true);
                    }
                }
            }
            catch (SqlException e)
            {
                return Task.FromResult(false);
            }
        }

        public async Task<bool> CreateGameAsync(GameInfo i_info)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Games VALUES('" + i_info.id + "'," + i_info.maxPlayers + ","+0+","+i_info.map+")";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(i_info.id));
                await actor.InitializeGameAsync();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Task DeleteGame(string i_id)
        {
            throw new NotImplementedException();
        }

        public Task<List<GameInfo>> GetGameList()
        {
            List<GameInfo> gameList = new List<GameInfo>();
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Games;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GameInfo gameInfo = new GameInfo();
                                gameInfo.id = reader.GetString(0);
                                gameInfo.maxPlayers = reader.GetInt32(1);
                                gameInfo.players = reader.GetInt32(2);
                                gameInfo.map = reader.GetInt32(3);
                                gameList.Add(gameInfo);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {

            }

            return Task.FromResult(gameList);
        }

        public async Task AddPlayerAsync(string i_gameId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    int players = 0;
                    connection.Open();
                    string query = "SELECT Players FROM Games WHERE Id = '" + i_gameId + "'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                players = reader.GetInt32(0);
                            }
                        }
                    }
                    players++;
                    query = "UPDATE Games SET Players = "+players+" WHERE Id = '" + i_gameId + "'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException e)
            {
            }
        }

        public async Task RemovePlayerAsync(string i_gameId)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    int players = 0;
                    connection.Open();
                    string query = "SELECT Players FROM Games WHERE Id = '" + i_gameId + "'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                players = reader.GetInt32(0);
                            }
                        }
                    }
                    players--;
                    query = "UPDATE Games SET Players = " + players + " WHERE Id = '" + i_gameId + "'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException e)
            {
            }
        }
    }
}
