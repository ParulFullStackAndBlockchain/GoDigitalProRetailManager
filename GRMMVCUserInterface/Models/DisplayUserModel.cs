using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GRMMVCUserInterface.Models
{
    public class DisplayUserModel
    {
        public List<string> SelectedUsers { get; set; }
        public List<SelectListItem> Users { get; set; }
    }
}