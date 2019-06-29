using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EVotingSystem.Data;
using EVotingSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace EVotingSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SupervisorsVotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupervisorsVotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SupervisorsVotes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SupervisorsVotes.Include(s => s.ApplicationUser).Include(s => s.Vote);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SupervisorsVotes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supervisorsVotes = await _context.SupervisorsVotes
                .Include(s => s.ApplicationUser)
                .Include(s => s.Vote)
                .FirstOrDefaultAsync(m => m.SupervisorId == id);
            if (supervisorsVotes == null)
            {
                return NotFound();
            }

            return View(supervisorsVotes);
        }

        // GET: SupervisorsVotes/Create
        public IActionResult Create()
        {
            ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["VoteId"] = new SelectList(_context.Votes, "Id", "Title");
            return View();
        }

        // POST: SupervisorsVotes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SupervisorId,VoteId")] SupervisorsVotes supervisorsVotes)
        {
            if (ModelState.IsValid)
            {
                _context.Add(supervisorsVotes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "Email", supervisorsVotes.SupervisorId);
            ViewData["VoteId"] = new SelectList(_context.Votes, "Id", "Title", supervisorsVotes.VoteId);
            return View(supervisorsVotes);
        }

        // GET: SupervisorsVotes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supervisorsVotes = await _context.SupervisorsVotes.FindAsync(id);
            if (supervisorsVotes == null)
            {
                return NotFound();
            }
            ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "Email", supervisorsVotes.SupervisorId);
            ViewData["VoteId"] = new SelectList(_context.Votes, "Id", "Title", supervisorsVotes.VoteId);
            return View(supervisorsVotes);
        }

        // POST: SupervisorsVotes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("SupervisorId,VoteId")] SupervisorsVotes supervisorsVotes)
        {
            if (id != supervisorsVotes.SupervisorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supervisorsVotes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupervisorsVotesExists(supervisorsVotes.SupervisorId))
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
            ViewData["SupervisorId"] = new SelectList(_context.Users, "Id", "Email", supervisorsVotes.SupervisorId);
            ViewData["VoteId"] = new SelectList(_context.Votes, "Id", "Title", supervisorsVotes.VoteId);
            return View(supervisorsVotes);
        }

        // GET: SupervisorsVotes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supervisorsVotes = await _context.SupervisorsVotes
                .Include(s => s.ApplicationUser)
                .Include(s => s.Vote)
                .FirstOrDefaultAsync(m => m.SupervisorId == id);
            if (supervisorsVotes == null)
            {
                return NotFound();
            }

            return View(supervisorsVotes);
        }

        // POST: SupervisorsVotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var supervisorsVotes = await _context.SupervisorsVotes.FindAsync(id);
            _context.SupervisorsVotes.Remove(supervisorsVotes);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SupervisorsVotesExists(string id)
        {
            return _context.SupervisorsVotes.Any(e => e.SupervisorId == id);
        }
    }
}
