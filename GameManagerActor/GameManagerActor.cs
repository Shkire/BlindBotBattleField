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
    internal class GameManagerActor : Actor, IGameManagerActor, IRemindable
    {
        //List of players on this game
        private List<string> p_playerList;

        //Players that has confirmed that are connected
        private List<string> p_connectedPlayers;

        private Dictionary<int, int[,]> p_playerPositions;

        private List<int>[,] p_gameMapInfo;

        private int p_maxPlayers = 4;

        /// <summary>
        /// Inicializa una instancia nueva de GameManagerActor
        /// </summary>
        /// <param name="actorService">El atributo Microsoft.ServiceFabric.Actors.Runtime.ActorService que hospedará esta instancia de actor.</param>
        /// <param name="actorId">El atributo Microsoft.ServiceFabric.Actors.ActorId de esta instancia de actor.</param>
        public GameManagerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            p_playerList = new List<string>();
            p_connectedPlayers = new List<string>();
        }

        public Task PlayerAttacks(string i_playerId)
        {
            throw new NotImplementedException();
        }

        public async Task PlayerDisconnectAsync(string i_playerId)
        {
            p_playerList.Remove(i_playerId);
            p_connectedPlayers.Remove(i_playerId);
            if (p_connectedPlayers.Count == 0)
            {
                IActorReminder reminder = GetReminder("LobbyCheck");
                await UnregisterReminderAsync(reminder);
            }
        }

        public Task PlayerMoves(int[,] i_dir, string i_playerId)
        {
            throw new NotImplementedException();
        }

        //!!!!!Player Id collision problem
        public async Task<bool> PlayerRegisterAsync(string i_playerId)
        {
            if (p_playerList.Count < p_maxPlayers)
            {
                if (p_playerList.Count == 0)
                    await this.RegisterReminderAsync("LobbyCheck", null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                p_playerList.Add(i_playerId);
                p_connectedPlayers.Add(i_playerId);
                return true;
            }
            return false;
        }

        public async Task PlayerStillConnectedAsync(string i_playerId)
        {
            if (!p_connectedPlayers.Contains(i_playerId))
                p_connectedPlayers.Add(i_playerId);
        }

        public async Task UpdateLobbyInfoAsync()
        {
            var ev = GetEvent<IGameLobbyEvents>();
            ev.GameLobbyInfoUpdate(p_playerList);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName.Equals("LobbyCheck"))
            {
                for (int i=0; i<p_playerList.Count; i++)
                {
                    if (p_connectedPlayers.Contains(p_playerList[i]))
                    {
                        p_connectedPlayers.Remove(p_playerList[i]);
                    }
                    else
                    {
                        await PlayerDisconnectAsync(p_playerList[i]);
                        i--;
                    }
                }
            }
            await UpdateLobbyInfoAsync();
        }
    }
}
