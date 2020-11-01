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
    [Authorize(Roles = "Cashier")]
    public class ProductController : ApiController
    {
        public List<Product> Get()
        {
            ProductData data = new ProductData();
            return data.GetProducts();
        }
    }
}
