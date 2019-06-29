using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EVotingSystem.Data;
using EVotingSystem.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity.UI.Services;
using EVotingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using EVotingSystem.Services;

namespace EVotingSystem.Controllers
{
    [Authorize(Roles = "Admin,Supervisor,Assistant")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public UsersController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)            
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Users
        public async Task<IActionResult> Index(string sortOrder, string currentFilter,
            string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PhoneSortParm"] = String.IsNullOrEmpty(sortOrder) ? "phone_desc" : "";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var users = _context.Users.Select(u => u);

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Email.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(s => s.Email);
                    break;
                case "phone_desc":
                    users = users.OrderByDescending(s => s.PhoneNumber);
                    break;
                default:
                    users = users.OrderByDescending(s => s.Email);
                    break;
            }
            int pageSize = 20;
            return View(await PaginatedList<ApplicationUser>.CreateAsync(
                users.AsNoTracking(), pageNumber ?? 1, pageSize));
            
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,Password,ConfirmPassword,PhoneNumber,ConfirmPhoneNumber,TwoFactorEnabled")] AppUserViewModel appUser)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = appUser.Email, Email = appUser.Email,
                    PhoneNumber = appUser.PhoneNumber, TwoFactorEnabled = appUser.TwoFactorEnabled };

                var result = await _userManager.CreateAsync(user, appUser.Password);

                if (result.Succeeded)
                {
                    //_logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);                    
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail", 
                        null,                        
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol : Request.Scheme
                        );
                    
                    await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //return LocalRedirect(returnUrl);
                }
                //_context.Add(appUser);
                //await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appUser);
        }

        // GET: Users/Edit/5
        /*public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _context.Users.FindAsync(id);
            if (appUser == null)
            {
                return NotFound();
            }
            return View(appUser);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Email,PhoneNumber,TwoFactorEnabled")] ApplicationUser appUser)
        {
            if (id != appUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _userManager.UpdateAsync(appUser);
                    //_context.Update(appUser);
                    //await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppUserExists(appUser.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(appUser);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appUser == null)
            {
                return NotFound();
            }

            return View(appUser);
        }*/

        // POST: Users/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var appUser = await _context.Users.FindAsync(id);
            _context.Users.Remove(appUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
