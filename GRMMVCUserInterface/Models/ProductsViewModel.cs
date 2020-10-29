using GRMMVCUserInterface.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GRMMVCUserInterface.Models
{
    public class ProductsViewModel
    {
        public List<string> SelectedAvailableProducts { get; set; }
        public List<SelectListItem> AvailableProducts { get; set; }
        public List<string> SelectedProductsAddedToCart { get; set; }
        public List<SelectListItem> ProductsAddedToCart { get; set; }
        public int Quantity { get; set; }
        [HiddenInput(DisplayValue = false)]
        public string ProductsAddedToCartString { get; set; }
        [DataType(DataType.Currency)]
        public decimal SubTotal { get; set; }
        [DataType(DataType.Currency)]
        public decimal Tax { get; set; }
        [DataType(DataType.Currency)]
        public decimal Total { get; set; }
    }
}
