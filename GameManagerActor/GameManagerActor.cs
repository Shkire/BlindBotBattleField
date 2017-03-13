using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using GameManagerActor.Interfaces;

namespace GameManagerActor
{
    /// <remarks>
    /// Esta clase representa a un actor.
    /// Cada elemento ActorID se asigna a una instancia de esta clase.
    /// El atributo StatePersistence determina la persistencia y la replicación del estado del actor:
    ///  - Persisted: el estado se escribe en el disco y se replica.
    ///  - Volatile: el estado se conserva solo en la memoria y se replica.
    ///  - None: el estado se conserva solo en la memoria y no se replica.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class GameManagerActor : Actor, IGameManagerActor
    {
        /// <summary>
        /// Inicializa una instancia nueva de GameManagerActor
        /// </summary>
        /// <param name="actorService">El atributo Microsoft.ServiceFabric.Actors.Runtime.ActorService que hospedará esta instancia de actor.</param>
        /// <param name="actorId">El atributo Microsoft.ServiceFabric.Actors.ActorId de esta instancia de actor.</param>
        public GameManagerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// Cada vez que se activa un actor, se llama a este método.
        /// Un actor se activa la primera vez que se invoca alguno de sus métodos.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // StateManager es el almacén de estados privado de este actor.
            // Los datos almacenados en StateManager se replicarán para ofrecer alta disponibilidad a los actores que utilizan almacenamiento de estados volátil o persistente.
            // En StateManager se puede guardar cualquier objeto serializable.
            // Para obtener más información, vea https://aka.ms/servicefabricactorsstateserialization.

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        /// <summary>
        /// TODO: Reemplácelo por su propio método de actor.
        /// </summary>
        /// <returns></returns>
        Task<int> IGameManagerActor.GetCountAsync()
        {
            return this.StateManager.GetStateAsync<int>("count");
        }

        /// <summary>
        /// TODO: Reemplácelo por su propio método de actor.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task IGameManagerActor.SetCountAsync(int count)
        {
            // No se garantiza que las solicitudes se procesarán en orden, ni siquiera una vez.
            // Aquí, la función update comprueba si el recuento entrante es superior al recuento actual para mantener el orden.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value);
        }
    }
}
