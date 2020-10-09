using GRMMVCUserInterface.Library.Models;
using System.Threading.Tasks;

namespace GRMMVCUserInterface.Library.API
{
    public interface IAPIHelper
    {
        Task<AuthenticatedUser> Authenticate(string username, string password);
        Task GetLoggedInUserInfo(string token);
    }
}