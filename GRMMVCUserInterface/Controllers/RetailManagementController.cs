using GRMMVCUserInterface.Library.API;
using GRMMVCUserInterface.Library.Helpers;
using GRMMVCUserInterface.Library.Models;
using GRMMVCUserInterface.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
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
        private ISaleEndPoint _saleEndPoint;
        private IUserEndPoint _userEndPoint;

        public RetailManagementController(IProductEndpoint productEndpoint, IConfigHelper configHelper, ISaleEndPoint saleEndPoint
            ,IUserEndPoint userEndPoint)
        {
            _productEndpoint = productEndpoint;
            _configHelper = configHelper;
            _saleEndPoint = saleEndPoint;
            _userEndPoint = userEndPoint;
        }

        public async Task<List<ProductModel>> GetAllProducts()
        {
            return await _productEndpoint.GetAll();
        }

        public async Task<List<UserModel>> GetAllUsers()
        {
            return await _userEndPoint.GetAll();
        }

        [HttpGet]
        public async Task<ActionResult> Sales(string serializedModel)
        {
            try
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

                ProductsViewModel productsViewModel;
                if (null == serializedModel)
                {
                    productsViewModel = new ProductsViewModel()
                    {
                        AvailableProducts = listSelectListItems,
                        ProductsAddedToCart = new List<SelectListItem>(),
                        ProductsAddedToCartString = "",
                        Quantity = 1,
                        SubTotal = 0,
                        Tax = 0,
                        Total = 0
                    };
                }
                else
                {
                    productsViewModel = JsonConvert.DeserializeObject<ProductsViewModel>(serializedModel);
                    if (null == productsViewModel)
                    {
                        productsViewModel = new ProductsViewModel()
                        {
                            AvailableProducts = listSelectListItems,
                            ProductsAddedToCart = new List<SelectListItem>(),
                            ProductsAddedToCartString = "",
                            Quantity = 1,
                            SubTotal = 0,
                            Tax = 0,
                            Total = 0
                        };
                    }
                }

                return View(productsViewModel);
            }
            catch (Exception ex)
            {
                //TODO : Log the exception. 
                if (ex.Message.Equals("Unauthorized"))
                {
                    ViewBag.ErrorTitle = $"{ex.Message}";
                    ViewBag.ErrorMessage = "You do not have permission to interact with the sales form";                   
                }
                return View("Error");
            }
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
            List<SaleDetailModel> saleDetails = new List<SaleDetailModel>(); ;

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
                    int Id = Convert.ToInt32(selectedAvailableProduct);
                    ProductModel selectedProduct = productList.FirstOrDefault(p => p.Id == Id);
                    selectedProduct.QuantityInStock -= model.Quantity;
                    productsAddedToCartString.Append($"{selectedAvailableProduct}:{selectedProduct.ProductName}:{model.Quantity};");
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
                    productsAddedToCartString.Replace($"{selectedProductToBeRemoved}:{selectedProduct.ProductName}:{model.Quantity};", string.Empty);
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
                    string[] itemInfo = productsAddedToCartStringItem.Split(':');
                    int cartItemId = Convert.ToInt32(itemInfo[0]);
                    int cartItemQuantity = Convert.ToInt32(itemInfo[2]);

                    SelectListItem cartList = new SelectListItem()
                    {
                        Text = itemInfo[1],
                        Value = itemInfo[0]
                    };
                    listCartItems.Add(cartList);

                    SaleDetailModel saleDetailModel = new SaleDetailModel();
                    saleDetailModel.ProductId = cartItemId;
                    saleDetailModel.Quantity = cartItemQuantity;
                    saleDetails.Add(saleDetailModel);
                }
            }

            ProductsViewModel productsViewModel = new ProductsViewModel()
            {
                AvailableProducts = listAvailableItems,
                ProductsAddedToCart = listCartItems,
                ProductsAddedToCartString = productsAddedToCartString.ToString(),
                Quantity = model.Quantity,
                SubTotal = subTotal,
                Tax = tax,
                Total = total
            };

            if (null == model.SelectedAvailableProducts && null == model.SelectedProductsAddedToCart)
            {
                SaleModel sale = new SaleModel();
                sale.SaleDetails = saleDetails;
                await _saleEndPoint.PostSale(sale);

                ProductsViewModel resetProductsViewModel = new ProductsViewModel()
                {
                    AvailableProducts = listAvailableItems,
                    ProductsAddedToCart = new List<SelectListItem>(),
                    ProductsAddedToCartString = "",
                    Quantity = 1,
                    SubTotal = 0,
                    Tax = 0,
                    Total = 0
                };
                return RedirectToAction("Sales", new { serializedModel = JsonConvert.SerializeObject(resetProductsViewModel) });
            }

            return RedirectToAction("Sales", new { serializedModel = JsonConvert.SerializeObject(productsViewModel) });
        }

        public async Task<JsonResult> GetSelectedProductDetails(string productId)
        {
            var productList = await GetAllProducts();
            ProductModel product = productList.First(p => p.Id.Equals(Convert.ToInt32(productId)));
            return Json(new {name = product.ProductName, price = product.RetailPrice, quantity = product.QuantityInStock}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> UserDisplay()
        {
            try
            {
                var userList = await GetAllUsers();
                List<SelectListItem> listSelectListItems = new List<SelectListItem>();
                foreach (UserModel user in userList)
                {
                    string rolesList = string.Join(",", user.Roles.Select(x => x.Value));

                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = $"{user.Email} : {Environment.NewLine} {rolesList}",
                        Value = user.Id.ToString()
                    };
                    listSelectListItems.Add(selectList);
                }

                DisplayUserModel displayUserModel = new DisplayUserModel()
                {
                    Users = listSelectListItems
                };

                return View(displayUserModel);
            }
            catch (Exception ex)
            {
                //TODO : Log the exception. 
                if (ex.Message.Equals("Unauthorized"))
                {
                    ViewBag.ErrorTitle = $"{ex.Message}";
                    ViewBag.ErrorMessage = "You do not have permission to interact with the Users form";
                }
                return View("Error");
            }
        }
    }
}