using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TpGestionEcole.Models;

namespace TpGestionEcole.Controllers
{
    public class HomeController : Controller
    {
        // class members
        private readonly ILogger<HomeController> _logger;
        private readonly EcoleDbEntities _context  ;

        // constructors
        public HomeController(ILogger<HomeController> logger, EcoleDbEntities context)
        {
            _logger = logger;
            _context = context; 
        }

        // controller actions
        public IActionResult Index()
        {
            var listParcours = from p in _context.Parcours select p;
            return View("Index", listParcours.ToList());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}