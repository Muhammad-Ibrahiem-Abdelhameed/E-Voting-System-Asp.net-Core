using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVotingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
            // total 
            ViewData["StatesCount"] = _context.States.Count();
            ViewData["VotesCount"] = _context.Votes.Count();
            ViewData["UsersCount"] = _context.Users.Count();
            ViewData["VotersCount"] = _context.Voters.Count();

            ViewData["Votes"] = new SelectList(_context.Votes, "Id", "Title");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("VoteId")] string voteId)
        {

            // total 
            ViewData["StatesCount"] = _context.States.Count();
            ViewData["VotesCount"] = _context.Votes.Count();
            ViewData["UsersCount"] = _context.Users.Count();
            ViewData["VotersCount"] = _context.Voters.Count();

            ViewData["Votes"] = new SelectList(_context.Votes, "Id", "Title");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Overview([Bind("VoteId")] string voteId)
        {
            var cands = _context.Candidates
                    .Where(c => c.Vote.Id == voteId);

            var states = new Dictionary<string, int>();
            states = _context.States.ToDictionary(s => s.Name, s => 0);

            ViewData["CandidateNames"] = JsonConvert.SerializeObject(
                cands.Select(c => c.Name).ToList());

            ViewData["CurrentVoted"] = JsonConvert.SerializeObject(
                cands.Select(c => c.CurrentVoted).ToList());

            var presntVoters = 0;
            var absentVoters = 0;
            await _context.VoterVotes.Include(vv => vv.Voter).Include(vv => vv.Voter.State)
                .Where(vv => vv.VoteId == voteId).ForEachAsync(vv =>
                {
                    if (vv.IsVoted == true)
                    {
                        states[vv.Voter.State.Name] += 1;
                        presntVoters += 1;
                    }
                    else
                    {
                        states[vv.Voter.State.Name] += 1;
                        absentVoters += 1;
                    }
                });            

            ViewData["PreVoters"] = presntVoters;
            ViewData["AbsVoters"] = absentVoters;            

            ViewData["StatesNames"] = JsonConvert.SerializeObject(states.Keys);

            ViewData["StatesVoted"] = JsonConvert.SerializeObject(states.Values);

            return PartialView("Overview");
        }

        /*foreach (var vote in _context.Votes.ToList())
            {
                
        }*/
    }
}