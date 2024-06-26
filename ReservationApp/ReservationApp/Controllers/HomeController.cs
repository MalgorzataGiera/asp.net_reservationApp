﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservationApp.Models;
using System.Security.Claims;

namespace ReservationApp.Controllers
{
    /// <summary>
    /// Controller for handling user actions related to reservations.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IReservationService _reservations;
        private readonly IRoomService _rooms;
        private readonly ReservationValidationService _reservationValidationService;
        private readonly AppDbContext _context;

        public HomeController(IReservationService reservations, ReservationValidationService reservationValidationService, IRoomService rooms, AppDbContext context)
        {
            _reservations = reservations;
            _reservationValidationService = reservationValidationService;
            _rooms = rooms;
            _context = context;
        }

        /// <summary>
        /// GET action for displaying the home page for user.
        /// </summary>
        /// <returns>The home page view.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET action for displaying the create reservation view.
        /// </summary>
        /// <returns>The create reservation view with a list of available rooms in the ViewBag.</returns>
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST action for creating a new reservation.
        /// </summary>
        /// <param name="model">The reservation model containing reservation data.</param>
        /// <returns>Returns a redirection to the Index action if the reservation is successfully created, otherwise returns the view with validation errors.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Reservation model)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

                model.ReceivedById = userIdClaim.Value;

                var selectedRoom = _rooms.FindRoomByAvailableSlots(model.NumberOfPeople);
                if (selectedRoom is null)
                {
                    ModelState.AddModelError("reservation.NumberOfPeople", "No rooms to accommodate this number of people. Please split your reservation.");
                    return View(model);
                }
                model.Room = selectedRoom;
                model.Price = selectedRoom.Price;

                if (!await _reservationValidationService.IsReservationDateValid(model))
                {
                    ModelState.AddModelError("reservation.Date", "No rooms are available for the selected period.");
                    return View(model);
                }

                model = await _reservations.CreateAsync(model);

                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("UserReservations");
            }
            return View(model);
        }

        /// <summary>
        /// Displays user's reservations.
        /// </summary>
        /// <returns>The view with user's reservations.</returns>
        [HttpGet]
        public IActionResult UserReservations()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var result = _reservations.FindReservationsByUserId(userIdClaim);
            return View(result);
        }

        /// <summary>
        /// Displays the form to update a reservation.
        /// </summary>
        /// <param name="id">The id of the reservation to be updated.</param>
        /// <returns>The view with the form to update the reservation.</returns>
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            return View(_reservations.FindById(id));
        }

        /// <summary>
        /// Updates the reservation.
        /// </summary>
        /// <param name="model">The reservation to be updated.</param>
        /// <returns>Redirects to user's reservations view after updating the reservation.</returns>
        [HttpPost]
        public async Task<IActionResult> Update(Reservation model, int id)
        {
            if (ModelState.IsValid)
            {
                var updatedReservation = _reservations.FindById(id);

                if (updatedReservation == null)
                {
                    return NotFound();
                }
                _context.Entry(updatedReservation).State = EntityState.Detached;

                updatedReservation.Date = model.Date;
                updatedReservation.City = model.City;
                updatedReservation.Address = model.Address;
                updatedReservation.NumberOfPeople = model.NumberOfPeople;
                updatedReservation.Owner = model.Owner;
                updatedReservation.NumberOfNights = model.NumberOfNights;

                var selectedRoom = _rooms.FindRoomByAvailableSlots(model.NumberOfPeople);
                if (selectedRoom is null)
                {
                    ModelState.AddModelError("reservation.NumberOfPeople", "No rooms to accommodate this number of people. Please split your reservation.");
                    return View(model);
                }
                model.Room = selectedRoom;
                model.Price = selectedRoom.Price;

                if (!await _reservationValidationService.IsReservationDateValid(model))
                {
                    ModelState.AddModelError("reservation.Date", "No rooms are available for the selected period.");
                    return View(model);
                }

                model = await _reservations.UpdateAsync(updatedReservation);

                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("UserReservations");
            }
            return View(model);
        }

        /// <summary>
        /// Displays the form to cancel a reservation.
        /// </summary>
        /// <param name="id">The id of the reservation to be canceled.</param>
        /// <returns>The view with the form to cancel the reservation.</returns>
        [HttpGet]
        public IActionResult Cancel(int id)
        {
            return View(_reservations.FindById(id));
        }

        /// <summary>
        /// Cancels the reservation.
        /// </summary>
        /// <param name="id">The id of the reservation to be canceled.</param>
        /// <returns>Redirects to user's reservations view after canceling the reservation.</returns>
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult>CancelConfirmed(int id)
        {
            var reservationId = _reservations.FindById(id);

            await _reservations.DeleteAsync(reservationId);

            return RedirectToAction("UserReservations");
        }

    }
}
