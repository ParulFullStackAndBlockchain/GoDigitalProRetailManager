using GRMDataManager.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRMDataManager.Library.DataAccess
{
    public class ProductData
    {
        private GRMContext db = new GRMContext();

        public List<Product> GetProducts()
        {
            var products = db.Products.ToList();
            return products;
        }
    }
}
