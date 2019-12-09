using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using M32COM_CW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using M32COM_CW.Models.MemberViewModel;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;

namespace M32COM_CW.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class MembersController : Controller
    {
        private readonly M32COM_CWContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MembersController(M32COM_CWContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Members
        public async Task<IActionResult> Index()
        {
            List<Member> members;

            if (User.IsInRole("Admin") || !(User.IsInRole("TeamManager")))
            {
                members = await _context.Member.OrderBy(m => m.TeamID).ToListAsync();
                foreach (Member mem in members)
                {
                    if (mem.TeamID == null)
                    {
                        mem.Team = new Team();
                    }
                    else
                    {
                        mem.Team = await _context.Team.SingleOrDefaultAsync(m => m.ID == mem.TeamID);
                    }
                }
            }
            else
            {
                var member = GetMemberFromUser();
                if (member.TeamID == null)
                {
                    members = await _context.Member.Where(m => m.Id == member.Id).ToListAsync();
                }
                else
                {
                    members = await _context.Member.Where(m => m.TeamID == member.TeamID).ToListAsync();
                }

                // Set team object for every member in members
                foreach (Member mem in members)
                {
                    if (mem.TeamID == null)
                    {
                        mem.Team = new Team();
                    }
                    else
                    {
                        mem.Team = await _context.Team.SingleOrDefaultAsync(m => m.ID == mem.TeamID);
                    }
                }
            }
            return View(members);
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Member
                .SingleOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            member.Team = await _context.Team
                .SingleOrDefaultAsync(m => m.ID == member.TeamID);

            return View(member);
        }

        // GET: Members/Create
        [Authorize(Roles = "Admin,TeamManager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        [Authorize(Roles = "Admin,TeamManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] MemberViewModel membervm)
        {
            if (ModelState.IsValid)
            {
                var currentMember = GetMemberFromUser();
                var member = new Member
                {
                    Forename = membervm.Forename,
                    Surname = membervm.Surname,
                    Role = membervm.Role,
                    TeamID = currentMember.TeamID,
                    Team = currentMember.Team,
                    ProfilePicture = ImageToByteArray(membervm.ProfilePicture).Result

            };
                // Get the image file from the view and convert into a byte array

                member.TeamID = currentMember.TeamID;
                member.Team = currentMember.Team;
                _context.Add(member);
                await _context.SaveChangesAsync();

                // Updating team references
                var team = await _context.Team
                    .SingleOrDefaultAsync(m => m.ID == member.TeamID);

                team.Members.Add(member);
                _context.Update(team);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(membervm);
        }

        // GET: Members/Edit/5
        [Authorize(Roles = "Admin,TeamManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Member member;
            if (User.IsInRole("Admin"))
            {
                member = await _context.Member.SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                member = await _context.Member.SingleOrDefaultAsync(m => m.Id == id && m.TeamID == GetMemberFromUser().TeamID);
            }
            if (member == null)
            {
                return NotFound();
            }

            var membervm = new MemberViewModel
            {
                Forename = member.Forename,
                Surname = member.Surname,
                Role = member.Role,
                TeamID = member.TeamID,
            };

            return View(membervm);
        }

        // POST: Members/Edit/5
        [Authorize(Roles = "Admin,TeamManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] MemberViewModel membervm)
        {
            var member = _context.Member.Find(id);

            if (ModelState.IsValid)
            {
                try
                {
                    member.Forename = membervm.Forename;
                    member.Surname = membervm.Surname;
                    member.Role = membervm.Role;
                    member.TeamID = membervm.TeamID;
                    if (membervm.ProfilePicture != null)
                    {
                        member.ProfilePicture = ImageToByteArray(membervm.ProfilePicture).Result;
                    }
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
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
            return View(membervm);
        }

        // GET: Members/Delete/5
        [Authorize(Roles = "Admin,TeamManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Member member;
            if (User.IsInRole("Admin"))
            {
                member = await _context.Member.SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                member = await _context.Member
                    .SingleOrDefaultAsync(m => m.Id == id && m.TeamID == GetMemberFromUser().TeamID && m.ApplicationUserId == null);
            }
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [Authorize(Roles = "Admin,TeamManager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Member member;
            if (User.IsInRole("Admin"))
            {
                member = await _context.Member.SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                member = await _context.Member.SingleOrDefaultAsync(m => m.Id == id && m.TeamID == GetMemberFromUser().TeamID && m.ApplicationUserId == null);
            }
            _context.Member.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Member.Any(e => e.Id == id);
        }

        // Returns a member based off of the current logged in user
        private Member GetMemberFromUser()
        {
            var user = _userManager.GetUserId(User);
            var member = _context.Member
                .Where(m => m.ApplicationUser.Id == user)
                .SingleOrDefault();

            return member;
        }
        public ActionResult GetImage(int id)
        {
            var member = _context.Member.Find(id);
            if (member == null)
            {
                return NotFound();
            }
            var imageData = member.ProfilePicture;
            if (imageData == null)
            {
                return NotFound();
            }
            return File(imageData, "image/jpg");
        }

        // Get the image file from the view and convert into a byte array
        private async Task<Byte[]> ImageToByteArray(IFormFile image)
        {
            if (image != null)
            {
                using (var memorystream = new MemoryStream())
                {
                    await image.CopyToAsync(memorystream);
                    return memorystream.ToArray();
                }
            } else
            {
                return null;
            }
        }
    }
}
