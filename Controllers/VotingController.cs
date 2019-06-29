using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVotingSystem.Data;
using EVotingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace EVotingSystem.Controllers
{
    [Authorize]
    public class VotingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        private static byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public VotingController(ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {            
            var userId = _userManager.GetUserId(HttpContext.User);            
            var voter = await _context.Voters.FindAsync(userId);
            
            if (voter == null || voter.AllowPhone == false)
            {
                return NotFound();               
            }
            var voteIds = _context.VoterVotes
                    .Where(vv => vv.VoterId == voter.Id && !vv.IsVoted)
                    .Select(vv => vv.VoteId)
                    .ToList();
            var votes = _context.Votes
                .Where(v => voteIds.Contains(v.Id))
                .Where(v => v.Status == "Running");

            // temp Data
            TempData["userId"] = userId;
            

            ViewData["Vote"] = new SelectList(votes, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromForm(Name = "Id")] string voteId)
        {
            if(voteId != null)
            {
                TempData["voteId"] = voteId;
                return RedirectToAction(nameof(Vote), new { voteId });
            }
            var votes = _context.Voters
                    .Include(v => v.ApplicationUser)
                    .Where(vv => vv.ApplicationUser.Id == _userManager.GetUserId(HttpContext.User));
                    
            // Temp Data
            ViewData["Votes"] = new SelectList(await votes.ToListAsync(), "Id", "Title", voteId);
            return RedirectToAction(nameof(Vote), "Id");

        }

        [HttpGet]
        public async Task<IActionResult> Vote(string voteId)
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            var voter = await _context.Voters.FindAsync(userId);
            if (voter == null || voter.AllowPhone == false)
            {
                return NotFound();
            }
            var candidates = _context.Candidates
                .Include(c => c.Vote)
                .Where(c => c.VoteId == voteId);

            var timeExpire = DateTime.Now.AddMinutes(10).ToString();

            /*var outputKey = ProtectedData.Protect( Encoding.Unicode.GetBytes(timeExpire), null, DataProtectionScope.CurrentUser);
            var outstring = Convert.ToBase64String(outputKey);
            ViewData["Expire"] = outstring;
            ViewData["Ex"] = Encoding.Unicode.GetString(
                                ProtectedData.Unprotect(outputKey, null,
                                DataProtectionScope.CurrentUser) );*/

            var encrypt = new Encrypt();
            var outstring = encrypt.EncryptString(timeExpire, "P");
            
            //ViewData["Expire"] = outstring;
            //ViewData["Ex"] = encrypt.DecryptString(outstring, "P");
            return View(await candidates.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote()
        {
            var candId = Request.Form["Id"];
            
            if (candId != "")
            {                
                try
                {
                    var isVoted = _context.VoterVotes
                        .Where(vv => vv.VoteId == TempData["voteId"].ToString() &&
                            vv.VoterId == TempData["userId"].ToString())
                        .FirstOrDefault().IsVoted;
                    if(isVoted == false )
                    {
                        var result = _context.Candidates.Find(candId).CurrentVoted + 1;
                        _context.Candidates.Find(candId).CurrentVoted = result;

                        _context.VoterVotes
                            .Where(vv => 
                                vv.VoteId == TempData["voteId"].ToString() &&
                                vv.VoterId == TempData["userId"].ToString())
                            .FirstOrDefault().IsVoted = true;

                        await _context.SaveChangesAsync();
                    }
                    
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CandidateExists(candId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Home" );
            }
            return View();
        }

        private bool CandidateExists(string id)
        {
            return _context.Candidates.Any(e => e.Id == id);
        }

        /*private bool IsVoted(string id)
        {
            return _context.V.Any(e => e.Id == id);
        }*/

        public class Encrypt
        {
            // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
            // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
            private const string initVector = "pemgail9uzpgzl88";
            // This constant is used to determine the keysize of the encryption algorithm
            private const int keysize = 256;
            //Encrypt
            public string EncryptString(string plainText, string passPhrase)
            {
                byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
                byte[] keyBytes = password.GetBytes(keysize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                byte[] cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                return Convert.ToBase64String(cipherTextBytes);
            }
            //Decrypt
            public string DecryptString(string cipherText, string passPhrase)
            {
                byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
                byte[] keyBytes = password.GetBytes(keysize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
        }
    }
}