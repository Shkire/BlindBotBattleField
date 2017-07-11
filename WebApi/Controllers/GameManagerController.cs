using GameManagerActor.Interfaces;
using GameManagerActor.Interfaces.BasicClasses;
using LoginService.Interfaces;
using LoginService.Interfaces.BasicClasses;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using ServerResponse;
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
        public ServerResponseInfo<bool, Exception> PostConnectPlayerAsync([FromBody] List<string> i_info)
        {
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(i_info[0]));
            return actor.ConnectPlayerAsync(i_info[1]).Result;
        }

        [Route("api/gamemanager/still")]
        public void PostPlayerStillConnectedAsync([FromBody] List<string> i_info)
        {
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(i_info[0]));
            Task.WaitAll(actor.PlayerStillConnectedAsync(i_info[1]));
        }

        [Route("api/gamemanager/lobby")]
        public void PostUpdateLobbyInfoAsync([FromBody] string i_gameId)
        {
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(i_gameId));
            Task.WaitAll(actor.UpdateLobbyInfoAsync());
        }

        [Route("api/gamemanager/move")]
        public ServerResponseInfo<bool, Exception> PostPlayerMovesAsync([FromBody] List<object> i_info)
        {
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId((string)i_info[0]));
            return actor.PlayerMovesAsync((int[])i_info[1], (string)i_info[2]).Result;
        }

        [Route("api/gamemanager/attack")]
        public ServerResponseInfo<bool, Exception> PostPlayerAttacksAsync([FromBody] List<string> i_info)
        {
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(i_info[0]));
            return actor.PlayerAttacksAsync(i_info[1]).Result;
        }

        [Route("api/gamemanager/radar")]
        public ServerResponseInfo<bool, Exception, CellContent[][]> PostRadarActivatedAsync([FromBody]List<string> i_info)
        {
            IGameManagerActor actor = ActorProxy.Create<IGameManagerActor>(new ActorId(i_info[0]));
            return actor.RadarActivatedAsync(i_info[1]).Result;
        }
    }
}

