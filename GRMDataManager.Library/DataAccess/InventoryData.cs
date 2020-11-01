using GRMDataManager.Library.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GRMDataManager.Library.DataAccess
{
    public class InventoryData : IDisposable
    {
        private GRMContext _db;

        public InventoryData()
        {
            _db = new GRMContext();
        }

        public List<Inventory> GetInventory()
        {
            var inventories = _db.Inventories.ToList();
            return inventories;
        }

        public void SaveInventoryRecord(Inventory item)
        {
            Product product = _db.Products.FirstOrDefault(x => x.Id == item.ProductId);
            item.Product = product;
            product.Inventories.Add(item);

            //Save the Inventory
            _db.Entry(item.Product).State = EntityState.Modified;
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
            _db = null;
        }
    }
}
