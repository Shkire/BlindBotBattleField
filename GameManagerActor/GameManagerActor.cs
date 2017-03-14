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
        private List<string> p_playerIdMap;

        private Dictionary<int, int[,]> p_playerPositions;

        private List<int>[,] p_gameMapInfo;

        private int p_maxPlayers;

        /// <summary>
        /// Inicializa una instancia nueva de GameManagerActor
        /// </summary>
        /// <param name="actorService">El atributo Microsoft.ServiceFabric.Actors.Runtime.ActorService que hospedará esta instancia de actor.</param>
        /// <param name="actorId">El atributo Microsoft.ServiceFabric.Actors.ActorId de esta instancia de actor.</param>
        public GameManagerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public Task PlayerAttacks(string i_playerId)
        {
            throw new NotImplementedException();
        }

        public Task PlayerDisconnect(string i_playerId)
        {
            throw new NotImplementedException();
        }

        public Task PlayerMoves(int[,] i_dir, string i_playerId)
        {
            throw new NotImplementedException();
        }

        //!!!!!Player Id collision problem
        public async Task<bool> PlayerRegisterAsync(string i_playerId)
        {
            bool aux;
            if (p_playerIdMap.Count < p_maxPlayers)
            {
                aux = await Task.FromResult(true);
                return aux;
                //!!!!!Client must connect to lobby state events
            }
            aux = await Task.FromResult(false);
            return aux;
        }
    }
}
