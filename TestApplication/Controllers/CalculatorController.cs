using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TestApplication.Controllers
{
    public class CalculatorController : Controller
    {
        //
        // GET: /Calculator/

        public ActionResult Index()
        {
            if (HttpContext.Request.QueryString.AllKeys.Contains("Language"))
            {
                string language = HttpContext.Request.QueryString["Language"];
                ViewBag.Language = language;
                ViewBag.Title = (language.Equals("FR", StringComparison.OrdinalIgnoreCase) ? "Calculatrice" : "Calculator");
            }
            else
            {
                ViewBag.Language = "Unknown";
                ViewBag.Title = "Calculator";
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(int summandOne, int summandTwo)
        {
            return View(summandOne + summandTwo);
        }

        

    }
}
