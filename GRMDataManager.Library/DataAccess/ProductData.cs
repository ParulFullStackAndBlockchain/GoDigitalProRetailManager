using GRMDataManager.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GRMDataManager.Library.DataAccess
{
    public class ProductData :IDisposable
    {
        private GRMContext _db;

        public ProductData()
        {
            _db = new GRMContext();
        }

        public List<Product> GetProducts()
        {
            var products = _db.Products.ToList();          
            return products;
        }

        public Product GetProductById(int productId)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            return product;
        }

        public void Dispose()
        {
            _db.Dispose();
            _db = null;
        }
    }
}
