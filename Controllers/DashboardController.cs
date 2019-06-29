using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVotingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVotingSystem.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,Assistant")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewData["States"] = _context.States.Count();
            ViewData["Votes"] = _context.Votes.Count();
            ViewData["Users"] = _context.Users.Count();
            ViewData["Voters"] = _context.Voters.Count();
            
                       
            return View();
        }
    }
}