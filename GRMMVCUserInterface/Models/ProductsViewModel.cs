using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GRMMVCUserInterface.Models
{
    public class ProductsViewModel
    {
        public IEnumerable<string> SelectedProducts { get; set; }
        public IEnumerable<SelectListItem> Products { get; set; }
    }
}
