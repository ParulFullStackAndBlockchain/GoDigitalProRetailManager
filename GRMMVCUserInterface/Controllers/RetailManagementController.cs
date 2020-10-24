using GRMMVCUserInterface.Library.API;
using GRMMVCUserInterface.Library.Helpers;
using GRMMVCUserInterface.Library.Models;
using GRMMVCUserInterface.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace GRMMVCUserInterface.Controllers
{
    public class RetailManagementController : Controller
    {
        private IProductEndpoint _productEndpoint;
        private IConfigHelper _configHelper;
        //private List<ProductModel> _productList;

        public RetailManagementController(IProductEndpoint productEndpoint, IConfigHelper configHelper)
        {
            _productEndpoint = productEndpoint;
            _configHelper = configHelper;
        }

        public async Task<List<ProductModel>> GetAllProducts()
        {
            //_productList = await _productEndpoint.GetAll();
            return await _productEndpoint.GetAll();
        }

        public async Task<ActionResult> Sales()
        {
            var productList = await GetAllProducts();

            List<SelectListItem> listSelectListItems = new List<SelectListItem>();       

            foreach (ProductModel product in productList)
            {
                SelectListItem selectList = new SelectListItem()
                {
                    Text = product.ProductName,
                    Value = product.Id.ToString()
                };
                listSelectListItems.Add(selectList);
            }

            ProductsViewModel productsViewModel = new ProductsViewModel()
            {
                AvailableProducts = listSelectListItems,
                ProductsAddedToCart = new List<SelectListItem>(),
                ProductsAddedToCartString = "",
                Quantity = 1,
                SubTotal = 0,
                Tax = 0,
                Total = 0
            };

            return View(productsViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Sales(ProductsViewModel model)
        {
            var productList = await GetAllProducts();
            StringBuilder productsAddedToCartString = new StringBuilder(model.ProductsAddedToCartString);       
            List<SelectListItem> listAvailableItems = new List<SelectListItem>();
            List<SelectListItem> listCartItems = new List<SelectListItem>();
            decimal subTotal = model.SubTotal;
            decimal taxRate = _configHelper.GetTaxRate() / 100;
            decimal tax = model.Tax;
            decimal total = model.Total;

            foreach (ProductModel product in productList)
            {
                SelectListItem availableList = new SelectListItem()
                {
                    Text = product.ProductName,
                    Value = product.Id.ToString()
                };
                listAvailableItems.Add(availableList);
            }

            if (model.SelectedAvailableProducts != null)
            {
                foreach (var selectedAvailableProduct in model.SelectedAvailableProducts)
                {                   
                    ProductModel selectedProduct = productList.FirstOrDefault(p => p.Id == Convert.ToInt32(selectedAvailableProduct));
                    selectedProduct.QuantityInStock -= model.Quantity;
                    //productsAddedToCartString.Append($"{selectedAvailableProduct}:{selectedProduct.ProductName};");
                    subTotal += selectedProduct.RetailPrice * model.Quantity;
                    if (selectedProduct.IsTaxable)
                    {
                        tax += selectedProduct.RetailPrice * model.Quantity * taxRate;
                    }
                }
            }

            if (model.SelectedProductsAddedToCart != null)
            {
                foreach (var selectedProductToBeRemoved in model.SelectedProductsAddedToCart)
                {                                      
                    ProductModel selectedProduct = productList.FirstOrDefault(p => p.Id == Convert.ToInt32(selectedProductToBeRemoved));
                    selectedProduct.QuantityInStock += model.Quantity;
                    //productsAddedToCartString.Replace($"{selectedProductToBeRemoved}:{selectedProduct.ProductName};", string.Empty);
                    subTotal -= selectedProduct.RetailPrice * model.Quantity;
                    if (selectedProduct.IsTaxable)
                    {
                        tax -= selectedProduct.RetailPrice * model.Quantity * taxRate;
                    }
                }
            }

            total = subTotal + tax;

            string[] productsAddedToCartStringArray = productsAddedToCartString.ToString().Split(';');
            foreach (string productsAddedToCartStringItem in productsAddedToCartStringArray)
            {
                if (!productsAddedToCartStringItem.IsNullOrWhiteSpace())
                {
                    SelectListItem cartList = new SelectListItem()
                    {
                        Text = productsAddedToCartStringItem.Split(':')[1],
                        Value = productsAddedToCartStringItem.Split(':')[0]
                    };
                    listCartItems.Add(cartList);
                }
            }

            ProductsViewModel productsViewModel = new ProductsViewModel()
            {
                AvailableProducts = listAvailableItems,
                ProductsAddedToCart = listCartItems,
                ProductsAddedToCartString = productsAddedToCartString.ToString(),
                Quantity = 1,
                SubTotal = subTotal,
                Tax = tax,
                Total = total
            };

            return View(productsViewModel);
        }

        public async Task<JsonResult> GetSelectedProductDetails(string productId)
        {
            var productList = await GetAllProducts();
            ProductModel product = productList.First(p => p.Id.Equals(Convert.ToInt32(productId)));
            return Json(new {name = product.ProductName, price = product.RetailPrice, quantity = product.QuantityInStock}, JsonRequestBehavior.AllowGet);
        }
      
    }
}