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
    [Authorize(Roles = "Admin,Supervisor")]
    public class ResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Results
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Results.Include(r => r.Candidate).Include(r => r.Vote);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Results/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.Results
                .Include(r => r.Candidate)
                .Include(r => r.Vote)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        // GET: Results/Create
        public IActionResult Create()
        {
            ViewData["Winner"] = new SelectList(_context.Candidates, "Id", "Name");
            ViewData["Id"] = new SelectList(_context.Votes, "Id", "Title");
            return View();
        }

        // POST: Results/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TotalVoters,CurrentVoted,Winner,Description")] Result result)
        {
            if (ModelState.IsValid)
            {
                _context.Add(result);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Winner"] = new SelectList(_context.Candidates, "Id", "Name", result.Winner);
            ViewData["Id"] = new SelectList(_context.Votes, "Id", "Title", result.Id);
            return View(result);
        }

        // GET: Results/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.Results.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            ViewData["Winner"] = new SelectList(_context.Candidates, "Id", "Name", result.Winner);
            ViewData["Id"] = new SelectList(_context.Votes, "Id", "Title", result.Id);
            return View(result);
        }

        // POST: Results/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Description")] Result result)
        {
            //"Id,TotalVoters,CurrentVoted,Winner,Description"
            if (id != result.Id)
            {
                return NotFound();
            }

            var voters = await _context.VoterVotes.Where(vv => vv.VoteId == id).ToListAsync();
            var totalVoters = voters.Count;
            var currentVoters = voters.Where(vv => vv.IsVoted).LongCount();
            var cands = await _context.Candidates
                .Include(c => c.Vote)
                .Where(c => c.VoteId == id)
                .ToListAsync();
            var maxCand = cands.Max(c => c.CurrentVoted);
            var winners = cands.Where(c => c.CurrentVoted == maxCand).ToList();

            result.TotalVoters = totalVoters;
            result.CurrentVoted = currentVoters;

            if(winners.Count == 1)
            {
                result.Winner = winners.FirstOrDefault().Id;
            }


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(result);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResultExists(result.Id))
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
            ViewData["Winner"] = new SelectList(_context.Candidates, "Id", "Name", result.Winner);
            ViewData["Id"] = new SelectList(_context.Votes, "Id", "Title", result.Id);
            return View(result);
        }

        // GET: Results/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var result = await _context.Results
                .Include(r => r.Candidate)
                .Include(r => r.Vote)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        // POST: Results/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _context.Results.FindAsync(id);
            _context.Results.Remove(result);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ResultExists(string id)
        {
            return _context.Results.Any(e => e.Id == id);
        }
    }
}
