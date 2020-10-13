using GRMMVCUserInterface.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GRMMVCUserInterface.Library.API
{
    public interface IProductEndpoint
    {
        Task<List<ProductModel>> GetAll();
    }
}