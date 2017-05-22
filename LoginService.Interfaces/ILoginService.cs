using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginService.Interfaces
{
    public interface ILoginService: IService
    {
        Task<bool> Login(string i_player, string i_pass);

    }
}
