using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginService.Interfaces
{
    public class GameInfo
    {
        public string id;
        public int maxPlayers;
        public int players;
        public int map;

        public GameInfo()
        {
        }

        public GameInfo(string i_gameId, int i_maxPlayers)
        {
            id = i_gameId;
            maxPlayers = i_maxPlayers;
        }
    }

    public interface ILoginService: IService
    {
        Task<bool> Login(string i_player, string i_pass);

        Task<bool> CreatePlayer(string i_player, string i_pass);

        Task<bool> CreateGameAsync(GameInfo i_info);

        Task AddPlayerAsync(string i_gameId);

        Task RemovePlayerAsync(string i_gameId);

        Task DeleteGame(string i_id);

        Task<List<GameInfo>> GetGameList();

    }
}
