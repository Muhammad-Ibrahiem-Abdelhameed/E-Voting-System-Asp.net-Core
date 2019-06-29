using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EVotingSystem.Data;
using EVotingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EVotingSystem.ViewModels;

namespace EVotingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileAppController : Controller
    {        
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;


        public MobileAppController(ApplicationDbContext context, 
                                UserManager<ApplicationUser> userManager,
                                IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }        
        
        [Route("login")]
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
                    Audience =  _configuration["Jwt:Site"],
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

                await _context.SaveChangesAsync();

                return Ok(
                  new
                  {
                      Token = validToken,
                      //expiration = token.ValidTo
                  });
            }
            return Unauthorized();
        }

        [Route("login2")]
        [HttpPost]
        public async Task<JsonResult> Login2([FromBody] LoginViewModel model)
        {
            AuthenticationTicket ticket = null;
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var claims = new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName ),
                };
                
                var identity = new ClaimsIdentity(claims);
                var principal = new ClaimsPrincipal(identity);
                ticket = new AuthenticationTicket(principal, "BasicAuthentication");
                var ff = new AuthenticationToken();

                //return new JsonResult(ff);
                

                
            }
            return new JsonResult(AuthenticateResult.Success(ticket).Ticket);
        }

        /*[Route("getvotes")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]        
        public async Task<ActionResult> GetVoterVotes()
        {
            /*var token = Request.Headers["Authorization"][0].Split(" ")[1];
            var user = await _context.UserJwtTokens
                        .Where(t => t.JwtToken == token)
                        .FirstOrDefaultAsync();        
            
            var result = new List<Dictionary<string, VoteResult>>();
            var votesId = await _context.UserVotes
                .Where(vv => vv.UserId == User.Identity.Name)                
                .Where(vote => !vote.IsVoted)
                .Select(v => v.VoteId).ToListAsync();

            foreach (var item in votesId)
            {
                var oneVote = new Dictionary<string, VoteResult>();
                var voteTitle = _context.Votes.Find(item).Title;
                var cands = _context.Candidates
                .Where(c => c.VoteId == item)
                .Select(c => new CandidateResult
                {
                    ListNumber = c.ListNumber.ToString(),
                    Name = c.Name,
                }).ToListAsync();
                oneVote.Add(item, new VoteResult { Title = voteTitle, Candidates = await cands });
                result.Add(oneVote);
            }

            return new JsonResult(result);
        }*/

        /*[Route("postvote")]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> PostVote([FromBody] BallotResult ballot)
        {
            var vote = await _context.Votes.FindAsync(ballot.VoteId);
            if(vote.VoteType == "Election")
            {
                // use index in database 
                // use composite key 
                //var result = _context.Candidates.Sele
                var candidate = _context.Candidates
                                .Where(c => c.Name == ballot.Candidates[0].Name)
                                .FirstOrDefault();

                candidate.Result += 1;
                try
                {
                    _context.Update(candidate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                


            }
            else if (vote.VoteType == "Vote")
            {

            }

            return Ok("Success");
        }*/

        /*[Route("setvote")]
        [Authorize]
        [HttpPut]
        public async Task<ActionResult> PutVote([FromBody] Candidate model)
        {
            var nationalId = model.NationalId;
            var result = new Dictionary<string, VoteResult>();
            var votesId = await _context.UserVotes
                .Where(vv => vv.ApplicationUser.NationalId == nationalId)
                .Where(vote => !vote.IsVoted)
                .Select(v => v.VoteId).ToListAsync();

            foreach (var item in votesId)
            {
                var voteTitle = _context.Votes.Find(item).Title;
                var cands = _context.Candidates
                .Where(c => c.VoteId == item)
                .Select(c => new CandidateResult
                {
                    ListNumber = c.ListNumber.ToString(),
                    Name = c.Name,
                }).ToListAsync();
                result.Add(item, new VoteResult { Title = voteTitle, Candidates = await cands });
            }

            return Ok(new { Response = "Success" });
        }*/



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        public ActionResult Test()
        {
            
            return new JsonResult( User.Identity.Name );
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("test2")]
        public ActionResult Test2()
        {

            return new JsonResult("Ok");
        }

        public class CandidateResult
        {
            public string Id { get; set; }
            public string ListNumber { get; set; }
            public string Name { get; set; }
            //public byte[] Picture { get; set; }            
        }

        public class VoteResult
        {
            public string Title { get; set; }
            public CandidateResult Candidate { get; set; }
        }

        public class BallotResult
        {
            public string VoteId { get; set; }
            public List<CandidateResult> Candidates { get; set; }
        }

        public class VoteR
        {
            public string NationalId { get; set; }
        }
    }
}