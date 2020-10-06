using GRMMVCUserInterface.Models;
using System.Threading.Tasks;

namespace GRMMVCUserInterface.Helpers
{
    public interface IAPIHelper
    {
        Task<AuthenticatedUser> Authenticate(string username, string password);
    }
}