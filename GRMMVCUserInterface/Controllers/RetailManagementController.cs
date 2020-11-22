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
using System.Web.Security;

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

        public async Task<List<SelectListItem>> GetAllRoles()
        {
            var roles = await _userEndPoint.GetAllRoles();

            List<SelectListItem> listRoles = new List<SelectListItem>();
            foreach (var role in roles)
            {
                SelectListItem selectList = new SelectListItem()
                {
                    Text = role.Value,
                    Value = role.Key
                };
                listRoles.Add(selectList); 
            }
            return listRoles;
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
        public async Task<ActionResult> UserDisplay(string serializedModel)
        {
            try
            {

                var userList = await GetAllUsers();
                var rolesList = await GetAllRoles();

                List<SelectListItem> listUsersWithRoles = new List<SelectListItem>();
                DisplayUserModel displayUserModel;

                if (null == serializedModel)
                {
                    foreach (UserModel user in userList)
                    {                        
                        string userRolesList = string.Join(",", user.Roles.Select(x => x.Value));
                        SelectListItem selectList = new SelectListItem()
                        {
                            Text = $"{user.Email} : {Environment.NewLine} {userRolesList}",
                            Value = user.Id.ToString()
                        };
                        listUsersWithRoles.Add(selectList);
                    }

                    displayUserModel = new DisplayUserModel()
                    {
                        Users = listUsersWithRoles
                    };
                }
                else
                {
                    displayUserModel = JsonConvert.DeserializeObject<DisplayUserModel>(serializedModel);
                    if (null == displayUserModel)
                    {
                        foreach (UserModel user in userList)
                        {                           
                            string userRolesList = string.Join(",", user.Roles.Select(x => x.Value));
                            SelectListItem selectList = new SelectListItem()
                            {
                                Text = $"{user.Email} : {Environment.NewLine} {userRolesList}",
                                Value = user.Id.ToString()
                            };
                            listUsersWithRoles.Add(selectList);
                        }

                        displayUserModel = new DisplayUserModel()
                        {
                            Users = listUsersWithRoles
                        };
                    }
                }

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

        [HttpPost]
        public async Task<ActionResult> UserDisplay(DisplayUserModel model)
        {
            try
            {
                var userList = await GetAllUsers();
                var rolesList = await GetAllRoles();

                List<SelectListItem> listUsersWithRoles = new List<SelectListItem>();
                List<SelectListItem> listExistingRoles = new List<SelectListItem>();
                List<SelectListItem> listOtherRoles = new List<SelectListItem>();
                string emailId = null;
                DisplayUserModel displayUserModel;

                foreach (UserModel user in userList)
                {
                    if (user.Id == model.SelectedUsers.First())
                    {
                        listExistingRoles = (from r in user.Roles
                                             select new SelectListItem
                                             {
                                                 Text = r.Value,
                                                 Value = r.Key
                                             }).ToList();


                        foreach (var role in rolesList)
                        {
                            if (!(listExistingRoles.Any(x => x.Value == role.Value)))
                            {
                                listOtherRoles.Add(role);
                            }
                        }

                        emailId = user.Email;
                    }

                    string userRolesList = string.Join(",", user.Roles.Select(x => x.Value));
                    SelectListItem selectList = new SelectListItem()
                    {
                        Text = $"{user.Email} : {Environment.NewLine} {userRolesList}",
                        Value = user.Id.ToString()
                    };
                    listUsersWithRoles.Add(selectList);
                }

                if (null != model.SelectedExistingRoles
                        && null == model.SelectedOtherRoles)
                {
                    SelectListItem existingRoleToBeRemoved = 
                        listExistingRoles.FirstOrDefault(x => x.Value == model.SelectedExistingRoles.First());
                    await _userEndPoint.RemoveUserFromRole(model.SelectedUsers.First(), existingRoleToBeRemoved.Text);
                    listExistingRoles.Remove(existingRoleToBeRemoved);
                    listOtherRoles.Add(existingRoleToBeRemoved);
                }

                if (null != model.SelectedOtherRoles
                        && null == model.SelectedExistingRoles)
                {
                    SelectListItem otherRoleToBeAdded =
                        listOtherRoles.FirstOrDefault(x => x.Value == model.SelectedOtherRoles.First());
                    await _userEndPoint.AddUserToRole(model.SelectedUsers.First(), otherRoleToBeAdded.Text);
                    listOtherRoles.Remove(otherRoleToBeAdded);
                    listExistingRoles.Add(otherRoleToBeAdded);
                }

                displayUserModel = new DisplayUserModel()
                {
                    Users = listUsersWithRoles,
                    SelectedUsers = model.SelectedUsers,
                    EmailId = emailId,
                    ExistingRoles = listExistingRoles,
                    OtherRoles = listOtherRoles
                };

                return RedirectToAction("UserDisplay", new { serializedModel = JsonConvert.SerializeObject(displayUserModel)});
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