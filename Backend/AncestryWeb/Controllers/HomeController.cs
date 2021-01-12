using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;


namespace AncestryWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Activate()
        {
            return View();
        }
        public ActionResult Restore()
        {
            return View();
        }

    }
}
