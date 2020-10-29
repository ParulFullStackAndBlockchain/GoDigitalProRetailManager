using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRMDataManager.Library.Models
{ 
    public partial class Product
    {
        public void AddSaleDetails(ICollection<SaleDetail> saleDetails)
        {
            HashSet<SaleDetail> saleDetailsHS = saleDetails as HashSet<SaleDetail>;
        }
    }

    public partial class User
    {
        public void AddSales(ICollection<Sale> sales)
        {
            HashSet<SaleDetail> saleDetailsHS = sales as HashSet<SaleDetail>;
        }
    }

    public partial class Sale
    {
        public void AddSaleDetails(ICollection<SaleDetail> saleDetails)
        {
            HashSet<SaleDetail> saleDetailsHS = saleDetails as HashSet<SaleDetail>;
        }
    }
}
