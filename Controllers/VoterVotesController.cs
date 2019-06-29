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
    [Authorize(Roles = "Admin,Supervisor,Assistant")]
    public class VoterVotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VoterVotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VoterVotes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.VoterVotes.Include(v => v.Vote).Include(v => v.Voter);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: VoterVotes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voterVotes = await _context.VoterVotes
                .Include(v => v.Vote)
                .Include(v => v.Voter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voterVotes == null)
            {
                return NotFound();
            }

            return View(voterVotes);
        }

        // GET: VoterVotes/Create
        public IActionResult Create()
        {
            ViewData["VoteId"] = new SelectList(_context.Votes, "Id", "Title");
            ViewData["VoterId"] = new SelectList(_context.Voters, "Id", "NationalId");
            return View();
        }

        // POST: VoterVotes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,VoterId,VoteId")] VoterVotes voterVotes)
        {
            if (ModelState.IsValid)
            {
                _context.Add(voterVotes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VoteId"] = new SelectList(_context.Votes, "Id", "Title", voterVotes.VoteId);
            ViewData["VoterId"] = new SelectList(_context.Voters, "Id", "NationalId", voterVotes.VoterId);
            return View(voterVotes);
        }

        // GET: VoterVotes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voterVotes = await _context.VoterVotes.FindAsync(id);
            if (voterVotes == null)
            {
                return NotFound();
            }
            ViewData["VoteId"] = new SelectList(_context.Votes, "Id", "Title", voterVotes.VoteId);
            ViewData["VoterId"] = new SelectList(_context.Voters, "Id", "NationalId", voterVotes.VoterId);
            return View(voterVotes);
        }

        // POST: VoterVotes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,VoterId,VoteId")] VoterVotes voterVotes)
        {
            if (id != voterVotes.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voterVotes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoterVotesExists(voterVotes.Id))
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
            ViewData["VoteId"] = new SelectList(_context.Votes, "Id", "Title", voterVotes.VoteId);
            ViewData["VoterId"] = new SelectList(_context.Voters, "Id", "NationalId", voterVotes.VoterId);
            return View(voterVotes);
        }

        // GET: VoterVotes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voterVotes = await _context.VoterVotes
                .Include(v => v.Vote)
                .Include(v => v.Voter)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voterVotes == null)
            {
                return NotFound();
            }

            return View(voterVotes);
        }

        // POST: VoterVotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var voterVotes = await _context.VoterVotes.FindAsync(id);
            _context.VoterVotes.Remove(voterVotes);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VoterVotesExists(string id)
        {
            return _context.VoterVotes.Any(e => e.Id == id);
        }
    }
}
