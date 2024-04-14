using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data;
using Data.Models;
using FluentEmail.Core;
using FluentEmail.Smtp;
using System.Net.Mail;
using System.Net.Sockets;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace FlightManagerASP.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly FmDbContext _context;

        public ReservationsController(FmDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var fmDbContext = _context.Reservations.Include(r => r.Flight);
            return View(await fmDbContext.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Flight)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "LocationFrom");
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,MiddleName,LastName,EGN,PhoneNumber,Nationality,TicketType,FlightId")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                var flight = await _context.Flights
                    .Include(f => f.Reservations)
                    .FirstOrDefaultAsync(f => f.Id == reservation.FlightId);

                if (flight == null)
                {
                    // Flight not found
                    ModelState.AddModelError(string.Empty, "Invalid flight selection.");
                    ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "LocationFrom", reservation.FlightId);
                    return View(reservation);
                }

                // Check flight capacity
                int bookedBusinessSeats = flight.Reservations.Where(r => r.TicketType == "Business").Sum(r => r.Id);
                int bookedOrdinarySeats = flight.Reservations.Where(r => r.TicketType == "Ordinary").Sum(r => r.Id);

                int availableBusinessSeats = flight.BusinessPassengerCapacity - bookedBusinessSeats;
                int availableOrdinarySeats = flight.PassangerCapacity - bookedOrdinarySeats;

                int requestedSeats = reservation.Id;
                int availableBSeats = reservation.TicketType == "Business" ? availableBusinessSeats : availableOrdinarySeats;
                int availableOSeats = reservation.TicketType == "Ordinary" ? availableBusinessSeats : availableOrdinarySeats;

                if (availableBSeats < requestedSeats || availableOSeats < requestedSeats)
                {
                    // Not enough available seats
                    ModelState.AddModelError(string.Empty, $"Not enough available seats for {reservation.TicketType} ticket.");
                    ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "LocationFrom", reservation.FlightId);
                    return View(reservation);
                }

                // Create the reservation
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }


            // Model state is not valid, return the view with validation errors
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "LocationFrom", reservation.FlightId);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "LocationFrom", reservation.FlightId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,MiddleName,LastName,EGN,PhoneNumber,Nationality,TicketType,FlightId")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
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
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "LocationFrom", reservation.FlightId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Flight)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }

    }
}
