using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FlightManagerASP.Controllers
{
    public class dbUsersController : Controller
    {
        private readonly FmDbContext _context;
        private readonly UserManager<dbUser> _userManager;

        public dbUsersController(FmDbContext context, UserManager<dbUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }



        // GET: dbUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: dbUsers/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dbUser == null)
            {
                return NotFound();
            }

            return View(dbUser);
        }

        // GET: dbUsers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: dbUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //Create a new user from dbUser and using the hashed password
        public async Task<IActionResult> Create([Bind("UserName,FirstName,LastName,EGN,Address,Id,Email,PasswordHash,PhoneNumber")] dbUser User)
        {
            string password = User.PasswordHash;
            dbUser user = new dbUser
            {
                UserName = User.Email,
                Email = User.Email,
                Address = User.Address,
                EGN = User.EGN,
                FirstName = User.FirstName,
                LastName = User.LastName,
                PhoneNumber = User.PhoneNumber,
            };
            if (ModelState.IsValid)
            {
                //Creating CreateAsync for the new user with the hashed password
                var _user = await _userManager.FindByNameAsync(user.UserName);
                if (_user == null)
                {
                    IdentityResult checkUser = await _userManager.CreateAsync(user, password);
                    if (checkUser.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Employee");
                    }
                }

                /*
                _context.Add(User);
                await _context.SaveChangesAsync();*/
                
                return RedirectToAction(nameof(Index));
                
            }
            return View(User);
        }

        // GET: dbUsers/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbUser = await _context.Users.FindAsync(id);
            if (dbUser == null)
            {
                return NotFound();
            }
            return View(dbUser);
        }

        // POST: dbUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserName,FirstName,LastName,EGN,Address,Id,Email,PasswordHash,PhoneNumber")] dbUser dbUser)
        {
            if (id != dbUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var editted = await _context.Users.FindAsync(id);
                    editted.FirstName = dbUser.FirstName;
                    editted.LastName = dbUser.LastName;
                    editted.EGN = dbUser.EGN;
                    editted.Address = dbUser.Address;
                    editted.PhoneNumber = dbUser.PhoneNumber;
                    editted.Email = dbUser.Email;
                    editted.UserName = dbUser.Email;
                    _context.Update(editted);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!dbUserExists(dbUser.Id))
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
            return View(dbUser);
        }

        // GET: dbUsers/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dbUser = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dbUser == null)
            {
                return NotFound();
            }

            return View(dbUser);
        }

        // POST: dbUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var dbUser = await _context.Users.FindAsync(id);
            _context.Users.Remove(dbUser);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool dbUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
