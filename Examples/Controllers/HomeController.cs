using Examples.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Examples.Controllers
{
    public class HomeController : Controller
    {
        private IDataRepository _dataRepository;

        public HomeController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Customers()
        {
            var customers = _dataRepository.GetCustomers();
            return View(customers);
        }
    }
}
