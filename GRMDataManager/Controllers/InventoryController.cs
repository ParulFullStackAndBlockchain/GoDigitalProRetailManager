using GRMDataManager.Library.DataAccess;
using GRMDataManager.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GRMDataManager.Controllers
{
    [Authorize]
    public class InventoryController : ApiController
    {
        [Authorize(Roles = "Admin,Manager")]
        public List<Inventory> GetInventoryData()
        {
            InventoryData data = new InventoryData();
            return data.GetInventory();
        }

        [Authorize(Roles = "Admin")]
        public void Post(Inventory item)
        {
            InventoryData data = new InventoryData();
            data.SaveInventoryRecord(item);
        }
    }
}
