using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace GameManagerActor.Interfaces
{
    /// <summary>
    /// Esta interfaz define los métodos expuestos por un actor.
    /// Los clientes utilizan esta interfaz para interactuar con el actor que la implementa.
    /// </summary>
    public interface IGameManagerActor : IActor
    {
        /// <summary>
        /// TODO: Reemplácelo por su propio método de actor.
        /// </summary>
        /// <returns></returns>
        Task<int> GetCountAsync();

        /// <summary>
        /// TODO: Reemplácelo por su propio método de actor.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task SetCountAsync(int count);
    }
}
