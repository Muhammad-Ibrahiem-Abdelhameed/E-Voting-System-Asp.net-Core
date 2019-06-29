using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EVotingSystem.Data;
using EVotingSystem.Models;
using EVotingSystem.Services;

namespace EVotingSystem.Controllers
{
    public class VotersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VotersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Voters
        public async Task<IActionResult> Index(string sortOrder, string currentFilter,
            string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["FullNameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "fullname_desc" : "";
            ViewData["PhoneSortParm"] = String.IsNullOrEmpty(sortOrder) ? "phone_desc" : "";
            ViewData["StateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "state_desc" : "";
            ViewData["NationalIdSortParm"] = String.IsNullOrEmpty(sortOrder) ? "national_desc" : "";

            
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var voters = _context.Voters.Select(v => v);

            if (!String.IsNullOrEmpty(searchString))
            {
                voters = voters.Where(u => u.FullName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "fullname_desc":
                    voters = voters.OrderByDescending(s => s.FullName);
                    break;
                case "phone_desc":
                    voters = voters.OrderByDescending(s => s.NationalId);
                    break;
                case "state_desc":
                    voters = voters.OrderByDescending(s => s.StateId);
                    break;
                case "national_desc":
                    voters = voters.OrderByDescending(s => s.StateId);
                    break;
                default:
                    voters = voters.OrderByDescending(s => s.FullName);
                    break;
            }
            int pageSize = 20;
            return View(await PaginatedList<Voter>.CreateAsync(
                voters.AsNoTracking(), pageNumber ?? 1, pageSize));

            /*var applicationDbContext = _context.Voters.Include(v => v.ApplicationUser).Include(v => v.State);
            return View(await applicationDbContext.ToListAsync());*/
        }

        // GET: Voters/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voter = await _context.Voters
                .Include(v => v.ApplicationUser)
                .Include(v => v.State)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voter == null)
            {
                return NotFound();
            }

            return View(voter);
        }

        // GET: Voters/Create
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Name");
            return View();
        }

        // POST: Voters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FullName,NationalId,StateId,BirthDate,AllowPhone,Fingerprint")] Voter voter)
        {
            if (ModelState.IsValid)
            {
                _context.Add(voter);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", voter.Id);
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Name", voter.StateId);
            return View(voter);
        }

        // GET: Voters/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voter = await _context.Voters.FindAsync(id);
            if (voter == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Id", voter.Id);
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Name", voter.StateId);
            return View(voter);
        }

        // POST: Voters/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FullName,NationalId,StateId,BirthDate,AllowPhone,Fingerprint")] Voter voter)
        {
            if (id != voter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoterExists(voter.Id))
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
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Id", voter.Id);
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Name", voter.StateId);
            return View(voter);
        }

        // GET: Voters/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voter = await _context.Voters
                .Include(v => v.ApplicationUser)
                .Include(v => v.State)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voter == null)
            {
                return NotFound();
            }

            return View(voter);
        }

        // POST: Voters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var voter = await _context.Voters.FindAsync(id);
            _context.Voters.Remove(voter);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VoterExists(string id)
        {
            return _context.Voters.Any(e => e.Id == id);
        }
    }
}
