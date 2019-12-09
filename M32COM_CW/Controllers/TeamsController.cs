using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M32COM_CW.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace M32COM_CW.Controllers
{
    [Route("[controller]/[action]")]
    public class TeamsController : Controller
    {
        private readonly M32COM_CWContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public TeamsController(M32COM_CWContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            List<Team> teams;

            if (User.IsInRole("Admin") || !(User.IsInRole("TeamManager")))
            {
                teams = await _context.Team.ToListAsync();

                foreach (Team team in teams)
                {
                    team.Members = await _context.Member.Where(m => m.TeamID == team.ID).ToListAsync();
                }
            }
            else
            {
                Member member = GetMemberFromUser();
                teams = await _context.Team.Where(m => m.ID == member.TeamID).ToListAsync();

                foreach (Team team in teams)
                {
                    team.Members = await _context.Member.Where(m => m.TeamID == team.ID).ToListAsync();
                }
            }
            return View(teams);
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Team
                .SingleOrDefaultAsync(m => m.ID == id);
            if (team == null)
            {
                return NotFound();
            }
            team.Members = await _context.Member.Where(m => m.TeamID == id).ToListAsync();

            ViewBag.teamLeader = _context.Member.Find(team.TeamLeaderId).FullName;
            return View(team);
        }

        [Authorize(Roles = "Admin,Member")]
        // GET: Teams/Create
        public IActionResult Create()
        {
            var member = GetMemberFromUser();
            var team = UpdateTeamLeaderId(member);

            return View(team);
        }


        // POST: Teams/Create
        // Create a team, update member team id, and change user role from Member to TeamMember
        [Authorize(Roles = "Admin,Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Name,TeamLeaderId")] Team team)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(User.Identity.Name);
                var member = GetMemberFromUser();
                if (member == null)
                {
                    return NotFound();
                }

                // Changing user role from member to TeamManager and TeamMember
                string[] rolesToAdd = { "TeamManager", "TeamMember" };
                await _userManager.AddToRolesAsync(user, rolesToAdd);
                await _userManager.RemoveFromRoleAsync(user, "Member");

                // setting TeamLeaderId to member id
                team.TeamLeaderId = member.Id;
                _context.Add(team);
                await _context.SaveChangesAsync();

                // setting member team.id to team id
                member.TeamID = team.ID;
                _context.Update(member);
                await _context.SaveChangesAsync();

                // Creating boat for team
                Boat boat = new Boat(team, team.ID);
                _context.Add(boat);
                await _context.SaveChangesAsync();

                await _signInManager.RefreshSignInAsync(user);

                return RedirectToAction(nameof(Index));
            }
            return View(team);
        }


        // GET: Teams/Edit/5
        [Authorize(Roles = "Admin,TeamManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Team team;
            if (User.IsInRole("Admin"))
            { 
                team = await _context.Team.SingleOrDefaultAsync(m => m.ID == id);
            }
            else
            {
                team = await _context.Team.SingleOrDefaultAsync(m => m.ID == id && m.TeamLeaderId == GetMemberFromUser().Id);
            }
            if (team == null)
            {
                return NotFound();
            }
            return View(team);
        }

        // POST: Teams/Edit/5
        [Authorize(Roles = "Admin,TeamManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Team updatedTeam)
        {
            if (id != updatedTeam.ID)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var team = _context.Team.Find(updatedTeam.ID);
                    team.Name = updatedTeam.Name;
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(updatedTeam.ID))
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
            return View(updatedTeam);
        }

        // GET: Teams/Delete/5
        [Authorize(Roles = "Admin,TeamManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Team team;
            if (User.IsInRole("Admin"))
            {
                team = await _context.Team
                    .SingleOrDefaultAsync(m => m.ID == id);
            }
            else
            {
                team = await _context.Team
                    .SingleOrDefaultAsync(m => m.ID == id && m.TeamLeaderId == GetMemberFromUser().Id);
            }
            if (team == null)
            {
                return NotFound();
            }
            ViewBag.teamLeader = _context.Member.Find(team.TeamLeaderId).FullName;
            return View(team);
        }

        // POST: Teams/Delete/5
        [Authorize(Roles = "Admin,TeamManager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Team
                .SingleOrDefaultAsync(m => m.ID == id);
            // Get a list of members in the team
            var members = _context.Member.Where(m => m.TeamID == id).Select(m => m).ToList();
            foreach (var member in members)
            {
                // Find any members who are Application Users
                if (member.ApplicationUserId != null)
                {
                    // Change the users role
                    var user = await _userManager.FindByIdAsync(member.ApplicationUserId);
                    string[] rolesToRemove = { "TeamManager", "TeamMember" };
                    await _userManager.AddToRoleAsync(user, "Member");
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    await _signInManager.RefreshSignInAsync(user);
                } else
                {
                    // Remove any members who aren't Application Users
                    _context.Member.Remove(member);
                }
            }
            // Remove the team
            _context.Team.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Team.Any(e => e.ID == id);
        }

        private Team UpdateTeamLeaderId(Member member)
        {
            Team team = new Team();
            team.TeamLeaderId = member.Id;

            return team;
        }

        private Member GetMemberFromUser()
        {
            var user = _userManager.GetUserId(User);
            var member = _context.Member
                .Where(m => m.ApplicationUser.Id == user)
                .SingleOrDefault();

            return member;
        }
    }
}
