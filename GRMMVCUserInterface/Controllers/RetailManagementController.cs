using GRMMVCUserInterface.Library.API;
using GRMMVCUserInterface.Library.Models;
using GRMMVCUserInterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace GRMMVCUserInterface.Controllers
{
    public class RetailManagementController : Controller
    {
        private IProductEndpoint _productEndpoint;

        public RetailManagementController(IProductEndpoint productEndpoint )
        {
            _productEndpoint = productEndpoint;
        }

        public async Task<ActionResult> Sales()
        {
            var productList = await _productEndpoint.GetAll();

            List<SelectListItem> listSelectListItems = new List<SelectListItem>();

            foreach (ProductModel product in productList)
            {
                SelectListItem selectList = new SelectListItem()
                {
                    Text = product.ProductName,
                    Value = product.Id.ToString()
                    //Selected = product.IsSelected
                };
                listSelectListItems.Add(selectList);
            }

            ProductsViewModel productsViewModel = new ProductsViewModel()
            {
                Products = listSelectListItems
            };

            return View(productsViewModel);
        }
    }
}