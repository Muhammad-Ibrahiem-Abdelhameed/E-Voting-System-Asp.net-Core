using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EVotingSystem.Data;
using EVotingSystem.Models;
using EVotingSystem.Services;
using EVotingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EVotingSystem.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,Assistant")]
    public class UsersVotersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public UsersVotersController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET:  UsersVoters/Index   1 to 0..1
        public async Task<IActionResult> Index(string sortOrder, string currentFilter,
            string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PhoneSortParm"] = String.IsNullOrEmpty(sortOrder) ? "phone_desc" : "";
            ViewData["StateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "state_desc" : "";
            ViewData["NationalSortParm"] = String.IsNullOrEmpty(sortOrder) ? "nationalId_desc" : "";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var usersv = _context.Users.Include(uv => uv.Voter).Include(uv => uv.Voter.State).Select(uv => uv);

            if (!String.IsNullOrEmpty(searchString))
            {
                usersv = usersv.Where(uv => uv.Email.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    usersv = usersv.OrderByDescending(s => s.Email);
                    break;
                case "phone_desc":
                    usersv = usersv.OrderByDescending(s => s.PhoneNumber);
                    break;
                case "state_desc":
                    usersv = usersv.OrderByDescending(s => s.Voter.State);
                    break;
                case "nationalId_desc":
                    usersv = usersv.OrderByDescending(s => s.Voter.NationalId);
                    break;
                default:
                    usersv = usersv.OrderByDescending(s => s.Email);
                    break;
            }
            int pageSize = 20;
            return View(await PaginatedList<ApplicationUser>.CreateAsync(
                usersv.AsNoTracking(), pageNumber ?? 1, pageSize));

            /*var applicationDbContext = _context.Users.Include(u => u.Voter).Include(u => u.Voter.State );            
            return View(await applicationDbContext.ToListAsync());*/
        }

        // GET: UsersVoters/Create
        public IActionResult Create()
        {            
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Name");
            return View();
        }

        // POST: Voters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,Password,ConfirmPassword,PhoneNumber,ConfirmPhoneNumber,TwoFactorEnabled,Voter")] UserVoterViewModel appUser)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = appUser.Email,
                    Email = appUser.Email,
                    PhoneNumber = appUser.PhoneNumber,
                    TwoFactorEnabled = appUser.TwoFactorEnabled
                };

                var result = await _userManager.CreateAsync(user, appUser.Password);

                if (result.Succeeded)
                {

                    //_logger.LogInformation("User created a new account with password.");
                    appUser.Voter.Id = user.Id;
                    _context.Voters.Add(appUser.Voter);
                    await _context.SaveChangesAsync();

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme
                        );

                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    return RedirectToAction(nameof(Index));
                }                 
            }            
                 
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Name", appUser.Voter.StateId);
            return View(appUser);
        }

        
    }
}