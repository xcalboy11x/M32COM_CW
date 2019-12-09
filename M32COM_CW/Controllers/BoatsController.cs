using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using M32COM_CW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace M32COM_CW.Controllers
{
    [Route("[controller]/[action]")]
    public class BoatsController : Controller
    {
        private readonly M32COM_CWContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BoatsController(M32COM_CWContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Boats
        public async Task<IActionResult> Index()
        {
            List<Boat> boats;
            
            if (User.IsInRole("Admin") || !(User.IsInRole("TeamManager")))
            {
                boats = await _context.Boat.ToListAsync();
                
                foreach (Boat boat in boats)
                {
                    boat.Team = await _context.Team.SingleOrDefaultAsync(m => m.ID == boat.TeamId);
                }
            }
            else
            {
                Member member = GetMemberFromUser();
                boats = await _context.Boat.Where(m => m.TeamId == member.TeamID).ToListAsync();
                foreach (Boat boat in boats)
                {
                    boat.Team = await _context.Team.SingleOrDefaultAsync(m => m.ID == boat.TeamId);
                }
            }
            
            return View(boats);
        }

        // GET: Boats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boat = await _context.Boat
                .Include(b => b.Team)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (boat == null)
            {
                return NotFound();
            }

            return View(boat);
        }

        // GET: Boats/Create
        [Authorize(Roles = "Admin,Member")]
        public IActionResult Create()
        {
            ViewData["TeamId"] = new SelectList(_context.Team, "ID", "ID");
            return View();
        }

        // POST: Boats/Create
        [Authorize(Roles = "Admin,Member")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] Boat boat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(boat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TeamId"] = new SelectList(_context.Team, "ID", "ID", boat.TeamId);
            return View(boat);
        }

        [Authorize(Roles = "Admin,TeamManager")]
        // GET: Boats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Boat boat;
            if (User.IsInRole("Admin"))
            {
                boat = await _context.Boat.SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                boat = await _context.Boat.SingleOrDefaultAsync(m => m.Id == id && m.TeamId == GetMemberFromUser().TeamID);
            }
            if (boat == null)
            {
                return NotFound();
            }
            ViewData["TeamId"] = new SelectList(_context.Team, "ID", "ID", boat.TeamId);
            return View(boat);
        }

        // POST: Boats/Edit/5
        [Authorize(Roles = "Admin,TeamManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Boat boat)
        {
            if (id != boat.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(boat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoatExists(boat.Id))
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
            ViewData["TeamId"] = new SelectList(_context.Team, "ID", "ID", boat.TeamId);
            return View(boat);
        }

        // GET: Boats/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Boat boat;
            if (User.IsInRole("Admin"))
            {
                boat = await _context.Boat.SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                boat = await _context.Boat
                .Include(b => b.Team)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == GetMemberFromUser().TeamID);
            }
            if (boat == null)
            {
                return NotFound();
            }

            return View(boat);
        }

        // POST: Boats/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Boat boat;
            if (User.IsInRole("Admin"))
            {
                boat = await _context.Boat.SingleOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                boat = await _context.Boat
                .Include(b => b.Team)
                .SingleOrDefaultAsync(m => m.Id == id && m.TeamId == GetMemberFromUser().TeamID);
            }
            _context.Boat.Remove(boat);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BoatExists(int id)
        {
            return _context.Boat.Any(e => e.Id == id);
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
    }
}
