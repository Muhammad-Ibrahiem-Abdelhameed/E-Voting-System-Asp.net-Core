using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EVotingSystem.Data;
using EVotingSystem.Models;
using EVotingSystem.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace EVotingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RaspberryPiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public RaspberryPiController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
             IHostingEnvironment hostingEnvironment,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }
       
        [Route("Login")]
        [HttpPost]
        public async Task<ActionResult> Login([FromBody] LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var signinKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]));
                int expiryInMinutes = Convert.ToInt32(_configuration["Jwt:ExpiryInMinutes"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = _configuration["Jwt:Site"],
                    Audience = _configuration["Jwt:Site"],
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
                    SigningCredentials = new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var validToken = tokenHandler.WriteToken(token);

                //await _context.SaveChangesAsync();

                return Ok( new {
                    Token = validToken,                      
                  });
            }
            return Unauthorized();
        }        

        /*[Route("Register")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]        
        public async Task<ActionResult> RegisterFingerprint([FromForm] IFormFile fingerprint)
        {
            //var userId = User.Identity.Name;
            var voter = await _context.Voters.FindAsync(User.Identity.Name);
            var filePath = _hostingEnvironment.ContentRootPath + "\\AppData\\Fingerprints\\";

            //var file = Request.Form.Files[0];
            if (fingerprint.Length > 0)
            {
                filePath += voter.Id;
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {                    
                    await fingerprint.CopyToAsync(stream);
                }
                voter.Fingerprint = filePath;
                _context.Voters.Update(voter);
                await _context.SaveChangesAsync();
                return Ok(new { result = "Thanks" });
            }
            
            return Ok(new { Error = "Error" });
        }*/        

        [Route("RegisterFingerprint")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult> RegisterFingerprint([FromBody] FingerprintText fingerprint)
        {
            //var userId = User.Identity.Name;
            var voter = await _context.Voters.FindAsync(User.Identity.Name);
            var filePath = _hostingEnvironment.ContentRootPath + "\\AppData\\Fingerprints\\";

            //var file = Request.Form.Files[0];
            if (fingerprint.Fingerprint.Length > 0)
            {                      
                voter.Fingerprint = fingerprint.Fingerprint;
                _context.Voters.Update(voter);
                await _context.SaveChangesAsync();
                return Ok(new { result = "Thanks" });
            }

            return Ok(new { Error = "Error" });
        }

        [Route("GetFingerprint")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]        
        public async Task<ActionResult> GetFingerprint()
        {
            var filePath = _hostingEnvironment.ContentRootPath + "\\AppData\\Fingerprints\\";
            var voter = await _context.Voters.FindAsync(User.Identity.Name);
            var votervotes = await _context.VoterVotes
                .Include(vv => vv.Vote)
                .Include(vv => vv.Voter)
                .Where(vv => vv.VoterId == User.Identity.Name)
                .Where(vv => vv.Vote.Status == "Running")
                .Where(vv => !vv.IsVoted)
                .ToListAsync();

            if (votervotes.Count > 0)
            {
                //var file = File(filePath + User.Identity.Name, "image/bmp");
                var fingerprint = voter.Fingerprint;
                if (String.IsNullOrEmpty(fingerprint))
                {
                    return NotFound();
                }

                return Ok ( new {fingerprint});
            }
            return Ok(new { result = "You voted for all votes" });
        }

        [Route("GetVote")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public async Task<ActionResult> GetVote()
        {
            var filePath = _hostingEnvironment.ContentRootPath + "\\AppData\\Fingerprints\\";
            var voter = await _context.Voters.FindAsync(User.Identity.Name);
            var votervotes = await _context.VoterVotes
                .Include(vv => vv.Vote)
                .Include(vv => vv.Voter)
                .Where(vv => vv.VoterId == User.Identity.Name)
                .Where(vv => vv.Vote.Status == "Running")
                .Where(vv => !vv.IsVoted)
                .ToListAsync();

            if (votervotes.Count > 0)
            {
                var voteRes = new VoteResult()
                {
                    VoteId = votervotes.ElementAt(0).VoteId,
                    Title = votervotes.ElementAt(0).Vote.Title,
                    Candidates = await _context.Candidates
                        .Where(c => c.VoteId == votervotes.ElementAt(0).VoteId)
                        .Select(c => new CandidateResultRasp
                        {
                            Id = c.Id,
                            ListNumber = c.ListNumber,
                            Name = c.Name
                        })
                        .ToListAsync()
                };

                return Json(voteRes);
            }
            return Ok(new { result = "You voted for all votes" });
        }

        [Route("Vote")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public async Task<ActionResult> Vote([FromBody] VoteResult voteResult)
        {    
            var voter = await _context.Voters.FindAsync(User.Identity.Name);
            var votervotes = await _context.VoterVotes
                .Include(vv => vv.Vote)
                .Include(vv => vv.Voter)
                .Where(vv => vv.VoterId == User.Identity.Name && vv.Voter.Fingerprint != null)              
                .Where(vv => vv.Vote.Title == voteResult.Title)
                .Where(vv => vv.Vote.Status == "Running")
                .FirstOrDefaultAsync();

            if (votervotes != null)
            {
                try
                {
                    _context.Candidates.Find(voteResult.Candidates[0].Id).CurrentVoted += 1;
                    votervotes.IsVoted = true;
                    _context.VoterVotes.Update(votervotes);
                    _context.SaveChanges();
                    return Ok(new { result = "Thanks a lot" });
                }
                catch (Exception)
                {
                    return BadRequest(new { result = "Error in Voting process" });
                }
                
            }
            return Ok(new { result = "You voted in all votes" });
        }

        public class CandidateResultRasp
        {
            public string Id { get; set; }
            public int ListNumber { get; set; }
            public string Name { get; set; }            
        }        

        public class VoteResult
        {
            public string VoteId { get; set; }
            public string Title { get; set; }
            public List<CandidateResultRasp> Candidates { get; set; }
        }

        public class FingerprintText
        {
            public string Fingerprint { get; set; }            
        }
    }
}