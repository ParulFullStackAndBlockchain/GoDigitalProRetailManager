using GRMMVCUserInterface.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRMMVCUserInterface.Library.API
{
    public interface IUserEndPoint
    {
        Task<List<UserModel>> GetAll();
    }
}