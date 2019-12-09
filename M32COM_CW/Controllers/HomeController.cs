using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using M32COM_CW.Models;
using Microsoft.AspNetCore.Authorization;


namespace M32COM_CW.Controllers
{
    public class HomeController : Controller
    {
        private readonly M32COM_CWContext _context;

        public HomeController(M32COM_CWContext context)
        {
            _context = context;
        }
        //GET Events
        public IActionResult Index()
        {
            var date = _context.Event.Where(m => m.EventStartDateTime >= DateTime.UtcNow).ToList();
            return View(date);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
