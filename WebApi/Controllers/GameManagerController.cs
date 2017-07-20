using BasicClasses.Common;
using ExtensionMethods;
using GameManagerActor.Interfaces;
using GameManagerActor.Interfaces.BasicClasses;
using LoginService.Interfaces;
using LoginService.Interfaces.BasicClasses;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi.Controllers
{
    [ServiceRequestActionFilter]
    public class GameManagerController : ApiController
    {
        [Route("api/gamemanager/connect")]
        public ServerResponseInfo<bool, Exception> PostConnectPlayerAsync([FromBody] string i_info)
        {
            List<string> deserialized = i_info.DeserializeObject<List<string>>();
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(deserialized[0].DeserializeObject<string>()));
            return actor.ConnectPlayerAsync(deserialized[1].DeserializeObject<string>(),deserialized[2].DeserializeObject<byte[]>()).Result;
        }

        [Route("api/gamemanager/still")]
        public void PostPlayerStillConnectedAsync([FromBody] string i_info)
        {
            List<string> deserialized = i_info.DeserializeObject<List<string>>();
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(deserialized[0]));
            Task.WaitAll(actor.PlayerStillConnectedAsync(deserialized[1]));
        }

        [Route("api/gamemanager/lobby")]
        public void PostUpdateLobbyInfoAsync([FromBody] string i_gameId)
        {
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(i_gameId));
            Task.WaitAll(actor.UpdateLobbyInfoAsync());
        }

        [Route("api/gamemanager/move")]
        public ServerResponseInfo<bool, Exception> PostPlayerMovesAsync([FromBody] string i_info)
        {
            List<string> deserialized = i_info.DeserializeObject<List<string>>();
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(deserialized[0].DeserializeObject<string>()));
            return actor.PlayerMovesAsync(deserialized[1].DeserializeObject<int[]>(), deserialized[2].DeserializeObject<string>()).Result;
        }

        [Route("api/gamemanager/attack")]
        public ServerResponseInfo<bool, Exception> PostPlayerAttacksAsync([FromBody] string i_info)
        {
            List<string> deserialized = i_info.DeserializeObject<List<string>>();
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(deserialized[0]));
            return actor.PlayerAttacksAsync(deserialized[1]).Result;
        }

        [Route("api/gamemanager/radar")]
        public ServerResponseInfo<bool, Exception, CellContent[][]> PostRadarActivatedAsync([FromBody] string i_info)
        {
            List<string> deserialized = i_info.DeserializeObject<List<string>>();
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(deserialized[0]));
            return actor.RadarActivatedAsync(deserialized[1]).Result;
        }
    }
}

