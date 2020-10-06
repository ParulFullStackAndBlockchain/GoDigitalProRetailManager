using GRMMVCUserInterface.Helpers;
using GRMMVCUserInterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GRMMVCUserInterface.Controllers
{
    public class AccountController : Controller
    {
        private IAPIHelper _aPIHelper;

        public AccountController(IAPIHelper aPIHelper)
        {
            _aPIHelper = aPIHelper;
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string UserName, string Password)
        {
            //if (ModelState.IsValid)
            try
            {
                var result = await _aPIHelper.Authenticate(UserName, Password);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View();
            }
        }
    }
}