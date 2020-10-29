using GRMMVCUserInterface.Library.Models;
using System.Threading.Tasks;

namespace GRMMVCUserInterface.Library.API
{
    public interface ISaleEndPoint
    {
        Task PostSale(SaleModel sale);
    }
}