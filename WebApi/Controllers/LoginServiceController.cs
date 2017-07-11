using LoginService.Interfaces;
using LoginService.Interfaces.BasicClasses;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using ServerResponse;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;

namespace WebApi.Controllers
{
    [ServiceRequestActionFilter]
    public class LoginServiceController : ApiController
    {
        [Route("api/login")]
        public ServerResponseInfo<bool, SqlException> PostLogin([FromBody]List<string> i_info)
        {
            ILoginService login = ServiceProxy.Create<ILoginService>(new Uri("fabric:/BlindBotBattleField/LoginService"));
            return login.Login(i_info[0], i_info[1]).Result;
        }

        [Route("api/login/new")]
        public ServerResponseInfo<bool, SqlException> PostCreatePlayer([FromBody]List<string> i_info)
        {
            ILoginService login = ServiceProxy.Create<ILoginService>(new Uri("fabric:/BlindBotBattleField/LoginService"));
            return login.CreatePlayer(i_info[0], i_info[1]).Result;
        }

        [Route("api/login/games/new")]
        public ServerResponseInfo<bool, Exception> PostCreateGameAsync([FromBody]List<object> i_info)
        {
            ILoginService login = ServiceProxy.Create<ILoginService>(new Uri("fabric:/BlindBotBattleField/LoginService"));
            return login.CreateGameAsync((string)i_info[0], (int)i_info[1]).Result;
        }

        [Route("api/login/games")]
        public ServerResponseInfo<bool, SqlException, List<GameDefinition>> GetGameList()
        {
            ILoginService login = ServiceProxy.Create<ILoginService>(new Uri("fabric:/BlindBotBattleField/LoginService"));
            return login.GetGameList().Result;
        }
    }
}
