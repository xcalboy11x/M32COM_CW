
using M32COM_CW.Models;
using M32COM_CW.Models.EventViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace M32COM_CW.Controllers
{
    [Route("[controller]/[action]")]
    public class EventsController : Controller
    {
        
        private readonly M32COM_CWContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsController(M32COM_CWContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            List<Venue> venues = await _context.Venue.ToListAsync();
            venues = await GetVenueObjectAsync(venues);

            return View(venues);
        }

        // Takes a list of Venues and returns Events and entries associated with each venue in the Venue list
        private async Task<List<Venue>> GetVenueObjectAsync(List<Venue> venues)
        {
            foreach (Venue venue in venues)
            {
                venue.Events = await _context.Event.Where(m => m.VenueId == venue.Id && m.EventStartDateTime >= DateTime.UtcNow).ToListAsync();
                foreach (Event eventM in venue.Events)
                {
                    eventM.Entries = await _context.Entry.Where(m => m.EventID == eventM.Id).ToListAsync();
                    
                    foreach (Entry entry in eventM.Entries)
                    {
                        entry.Team = await _context.Team.Where(m => m.ID == entry.TeamID).SingleOrDefaultAsync();
                    }
                }
            }

            return venues;
        }

        // Takes an event id and returns an event with the venue it's located at and the entries
        // Will also set two viewbag variables to return to the view - ViewBag.HasTeam and ViewBag.CurrentMemberId
        private Event GetVenueObjectAsync(int? id)
        {
            // Getting event by id
            Event eventM = _context.Event.Where(m => m.Id == id).OrderBy(e => e.Name).SingleOrDefault();

            // Getting venue for event
            Venue venue = _context.Venue.Where(m => m.Id == eventM.VenueId).SingleOrDefault();

            var member = GetMemberFromUser();

            // Setting event.venue to venue pulled from database
            eventM.Venue = venue;

            // Setting event.Entries to venue pulled from database
            eventM.Entries =  _context.Entry.Where(m => m.EventID == eventM.Id).ToList();

            // Check if user has signed up for event and set SignUp to false if the user hasn't signed ups
            var userEntry = _context.Entry.Where(e => e.TeamID == member.TeamID && e.Event.EventStartDateTime >= DateTime.Now && e.EventID == id);
            if (userEntry.Count() == 0)
            {
                // If user doesn't have a team then set ViewBag.HasTeam and ViewBag.SignedUp to false
                // Else set ViewBag.HasTeam to true and ViewBag.SignedUp to false
                if (member.TeamID == null)
                {
                    ViewBag.HasTeam = false;
                }
                else
                {
                    ViewBag.HasTeam = true;
                }
                ViewBag.SignedUp = false;
            }
            else
            {
                ViewBag.SignedUp = true;
            }
           
            // setting the team object in entries
            foreach (Entry entry in eventM.Entries)
            {
                entry.Team = _context.Team.Where(m => m.ID == entry.TeamID).SingleOrDefault();
            }

            ViewBag.CurrentMemberId = member.Id;

            return eventM;
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var events = GetVenueObjectAsync(id);

            if (events == null)
            {
                return NotFound();
            }

            return View(events);
        }

        // GET: Events/Create
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Create()
        {
            var EventModel = new EventViewModel();

            UpdateSelectListVenues(EventModel);

            return View(EventModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] EventViewModel eventvm)
        {
            var thisEvent = new Event
            {
                Name = eventvm.Name,
                Type = eventvm.Type,
                Description = eventvm.Description,
                DurationMinutes = eventvm.DurationMinutes,
                EventStartDateTime = eventvm.EventStartDateTime,
                VenueId = eventvm.VenueId,
                PromoImage = ImageToByteArray(eventvm.PromoImage).Result
            };

            if (ModelState.IsValid)
            {
                _context.Add(thisEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            UpdateSelectListVenues(eventvm);
            return View(eventvm);
        }

        // GET: Events/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var events = await _context.Event.SingleOrDefaultAsync(m => m.Id == id);

            var eventvm = new EventViewModel
            {
                Name = events.Name,
                Type = events.Type,
                Description = events.Description,
                DurationMinutes = events.DurationMinutes,
                EventStartDateTime = events.EventStartDateTime,
                VenueId = events.VenueId,
                Venues = events.Venues
            };
            UpdateSelectListVenues(eventvm);

            if (events == null)
            {
                return NotFound();
            }
            return View(eventvm);
        }

        // POST: Events/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] EventViewModel eventvm)
        {
            var events = await _context.Event.SingleOrDefaultAsync(m => m.Id == id);
            if (id != events.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    events.Name = eventvm.Name;
                    events.Type = eventvm.Type;
                    events.Description = eventvm.Description;
                    events.DurationMinutes = eventvm.DurationMinutes;
                    events.EventStartDateTime = eventvm.EventStartDateTime;
                    events.VenueId = eventvm.VenueId;
                    events.Venues = eventvm.Venues;
                    if (eventvm.PromoImage != null)
                    {
                        events.PromoImage = ImageToByteArray(eventvm.PromoImage).Result;
                    }
                    _context.Update(events);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventsExists(events.Id))
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
            UpdateSelectListVenues(eventvm);
            return View(eventvm);
        }

        // GET: Events/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var events = await _context.Event
                .SingleOrDefaultAsync(m => m.Id == id);
            if (events == null)
            {
                return NotFound();
            }

            return View(events);
        }

        // POST: Events/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entries = await _context.Entry.Where(m => m.EventID == id).ToListAsync();
            foreach (Entry entry in entries)
            {
                _context.Entry.Remove(entry);
            }
            await _context.SaveChangesAsync();

            var events = await _context.Event.SingleOrDefaultAsync(m => m.Id == id);
            _context.Event.Remove(events);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Sign up for an event
        // Takes an event id
        [Authorize(Roles = "TeamManager")]
        public async Task<IActionResult> SignUpForEvent(int id)
        {
            var eventToSignUpFor = await _context.Event
                .SingleOrDefaultAsync(m => m.Id == id);
            if (eventToSignUpFor == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var member = GetMemberFromUser();

                    var team = await _context.Team
                        .SingleOrDefaultAsync(m => m.ID == member.TeamID);
                    if (team == null)
                    {
                        return NotFound();
                    }

                    var preExistingEntry = await _context.Entry
                        .SingleOrDefaultAsync(m => m.EventID == id && m.TeamID == team.ID);
                    if (preExistingEntry == null) {
                        Entry entry = new Entry(team, eventToSignUpFor, DateTime.UtcNow);
                        _context.Add(entry);
                        await _context.SaveChangesAsync();

                        eventToSignUpFor.Entries.Add(entry);
                        _context.Update(eventToSignUpFor);
                        await _context.SaveChangesAsync();

                        ViewBag.UserSignedUp = "Register";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ViewBag.UserSignedUp = "Withdraw";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventsExists(eventToSignUpFor.Id))
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
            return View(eventToSignUpFor);
        }

        // Takes an entry id
        [Authorize(Roles = "Admin,TeamManager")]
        public async Task<IActionResult> WithdrawFromEvent(int id)
        {
            // Getting event and entry
            Entry entryToWithdraw;
            if (User.IsInRole("Admin"))
            {
                entryToWithdraw = await _context.Entry.SingleOrDefaultAsync(m => m.ID == id);
            }
            else
            {
                // Getting the entry to remove based off 
                entryToWithdraw = _context.Entry
                    .SingleOrDefault(m => m.ID == id && m.TeamID == GetMemberFromUser().TeamID);
            }

            if (entryToWithdraw == null)
            {
                return NotFound();
            }

            var eventToWithdrawFrom = await _context.Event.SingleOrDefaultAsync(m => m.Id == entryToWithdraw.EventID);
            if (eventToWithdrawFrom == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Removing entry and updating event in the database
                    _context.Remove(entryToWithdraw);
                    _context.SaveChanges();

                    eventToWithdrawFrom.Entries.Remove(entryToWithdraw);
                    _context.Update(eventToWithdrawFrom);
                    _context.SaveChanges();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventsExists(eventToWithdrawFrom.Id))
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
            return View();
        }

        private bool EventsExists(int id)
        {
            return _context.Event.Any(e => e.Id == id);
        }

        // Takes an event and returns an event with its list of venues populated
        public EventViewModel UpdateSelectListVenues(EventViewModel ev)
        {
            var result = _context.Venue.ToList();

            ev.Venues = new List<SelectListItem>();

            foreach (var item in result)
            {
                ev.Venues.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
            }

            return ev;
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
            var thisEvent = _context.Event.Find(id);
            if (thisEvent == null)
            {
                return NotFound();
            }
            var imageData = thisEvent.PromoImage;
            if (imageData == null)
            {
                return NotFound();
            }
            return File(imageData, "image/jpg");
        }

        private async Task<Byte[]> ImageToByteArray(IFormFile image)
        {
            if (image != null)
            {
                using (var memorystream = new MemoryStream())
                {
                    await image.CopyToAsync(memorystream);
                    return memorystream.ToArray();
                }
            }
            else
            {
                return null;
            }
        }
    }
}
