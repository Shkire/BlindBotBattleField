using BasicClasses.Common;
using ExtensionMethods;
using LoginService.Interfaces;
using LoginService.Interfaces.BasicClasses;
using Microsoft.ServiceFabric.Services.Remoting.Client;
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
        public ServerResponseInfo<bool, SqlException> PostLogin([FromBody]string i_info)
        {
            List<string> deserialized = i_info.DeserializeObject<List<string>>();
            ILoginService login = ServiceProxy.Create<ILoginService> (new Uri("fabric:/BlindBotBattleField/LoginService"));
            return login.Login(deserialized[0], deserialized[1]).Result;
        }

        [Route("api/login/new")]
        public ServerResponseInfo<bool, SqlException> PostCreatePlayer([FromBody]string i_info)
        {
            List<string> deserialized = i_info.DeserializeObject<List<string>>();
            ILoginService login = ServiceProxy.Create<ILoginService>(new Uri("fabric:/BlindBotBattleField/LoginService"));
            return login.CreatePlayer(deserialized[0], deserialized[1]).Result;
        }

        [Route("api/login/games/new")]
        public ServerResponseInfo<bool, Exception> PostCreateGameAsync([FromBody]string i_info)
        {
            List<string> deserialized = i_info.DeserializeObject<List<string>>();
            ILoginService login = ServiceProxy.Create<ILoginService>(new Uri("fabric:/BlindBotBattleField/LoginService"));
            return login.CreateGameAsync(deserialized[0].DeserializeObject<string>(), deserialized[1].DeserializeObject<int>()).Result;
        }

        [Route("api/login/games")]
        public ServerResponseInfo<bool, SqlException, List<GameDefinition>> GetGameList()
        {
            ILoginService login = ServiceProxy.Create<ILoginService>(new Uri("fabric:/BlindBotBattleField/LoginService"));
            return login.GetGameList().Result;
        }
    }
}
