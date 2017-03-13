using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace GameManagerActor.Interfaces
{
    /// <summary>
    /// GameManager Actor Interface.
    /// Contains all actor methods and other actors or client can call it using this Interface.
    /// </summary>
    public interface IGameManagerActor : IActor
    {
        Task<bool> PlayerRegisterAsync(string i_playerId);

        Task PlayerMoves(int[,] i_dir, string i_playerId);

        Task PlayerAttacks(string i_playerId);

        Task PlayerDisconnect(string i_playerId);
    }
}