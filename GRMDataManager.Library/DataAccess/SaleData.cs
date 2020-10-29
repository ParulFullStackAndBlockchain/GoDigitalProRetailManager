using GRMDataManager.Library.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GRMDataManager.Library.DataAccess
{
    public class SaleData :IDisposable
    {
        private GRMContext _db;

        public SaleData()
        {
            _db = new GRMContext();
        }

        public void SaveSale(SaleModel saleInfo, string cashierId)
        {          
                //TODO: Make this SOLID/DRY/Better

                //Start filling in the sale detail models we will save to the database

                //Fill in the available information
                List<SaleDetail> details = new List<SaleDetail>();
                ProductData productData = new ProductData();
                var taxRate = ConfigHelper.GetTaxRate() / 100;

                foreach (var item in saleInfo.SaleDetails)
                {
                    var detail = new SaleDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    };

                    // Get the information about this product
                    var productInfo = productData.GetProductById(item.ProductId);

                    if (productInfo == null)
                    {
                        throw new Exception($"The product Id of {item.ProductId} could not be found in the database.");
                    }

                    detail.PurchasePrice = productInfo.RetailPrice * detail.Quantity;
                    if (productInfo.IsTaxable)
                    {
                        detail.Tax = detail.PurchasePrice * taxRate;
                    }

                    details.Add(detail);
                }

                //Create the sale model
                Sale sale = new Sale
                {
                    SubTotal = details.Sum(x => x.PurchasePrice),
                    Tax = details.Sum(x => x.Tax),
                    CashierId = cashierId
                };

                sale.Total = sale.SubTotal + sale.Tax;
                User user = _db.Users.FirstOrDefault(x => x.Id == cashierId);
                sale.User = user;
                _db.Users.Attach(user);
                user.Sales.Add(sale);

                //Save the sale model

                //_db.Sales.Add(sale);
                //_db.Entry(sale.User).State = EntityState.Modified;
                _db.SaveChanges();

                //Get the id from the sale mode
                // Timeline 1:32 to 1:37

                //Finish filling in the sale detail models and Save the sale detail model
                foreach (var item in details)
                {
                    item.SaleId = sale.Id;

                    Product product = _db.Products.FirstOrDefault(x => x.Id == item.ProductId);
                    item.Product = product;
                    _db.Products.Attach(product);
                    product.SaleDetails.Add(item);

                    item.Sale = sale;
                    _db.Sales.Attach(sale);
                    sale.SaleDetails.Add(item);

                    //_db.SaleDetails.Add(item);
                    //_db.Entry(item.Product).State = EntityState.Modified;
                    //_db.Entry(item.Sale).State = EntityState.Modified;

                    _db.SaveChanges();
                }          
        }

        public void Dispose()
        {
            _db.Dispose();
            _db = null;
        }
    }
}
