using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DevSharp.TestWeb.Models;

namespace DevSharp.TestWeb.Controllers
{
    public class HomeController : Controller
    {
        private static readonly AllTestsModel Tests;

        static HomeController()
        {
            var types = new[]
            {
                typeof (Samples.Domains.Echoer),
                typeof (Samples.Domains.PingPong),
                typeof (Samples.Domains.Counter),
                typeof (Samples.Domains.TodoList),
                typeof (Samples.Domains.TodoListListProjection),
            };

            var tests = types.Select(t => new TestModel(t)).ToArray();

            Tests = new AllTestsModel {Tests = tests};
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Test(string test)
        {
            var result = Tests.Tests.FirstOrDefault(t => t.Id == test);

            if (result == null)
            {
                ViewBag.Message = "All tests";

                return View("AllTests", Tests);
            }
            else
            {
                ViewBag.Message = "Test " + result.DisplayName;
                
                return View("Test", result);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}