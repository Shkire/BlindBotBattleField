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
            builder.DataSource = "players-blindbotbattlefield.database.windows.net";
            builder.UserID = "players-admin";
            builder.Password = "blindBot17";
            builder.InitialCatalog = "Players";
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
        
    }
}
