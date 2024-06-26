﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReservationApp.Models
{
    /// <summary>
    /// Represents a room in the application.
    /// </summary>
    public class Room
	{
		/// <summary>
		/// Gets or sets the unique identifier for the room.
		/// </summary>
		[HiddenInput]
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets the room number.
		/// </summary>
		[Display(Name = "RoomNumber")]
		[Required(ErrorMessage = "You must provide the room number")]
		[Range(1, int.MaxValue)]
		public int RoomNumber { get; set; }

		/// <summary>
		/// Gets or sets the price for the room.
		/// </summary>
		[Display(Name = "Price")]
		[Required(ErrorMessage = "You must provide the room price")]
		[Column(TypeName = "decimal(8, 2)")]
		[Range(0.01, (double)decimal.MaxValue)]
		public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of people that can be accommodated in the room.
        /// </summary>
        [Required(ErrorMessage = "You must provide the maximum number of people for accommodation")]
		[Range(1,10, ErrorMessage = "Maximum number of people for accommodation in this room must be a value beetwen 1 and 10")]
        public int MaxPeopleNumber { get; set; }

		/// <summary>
		/// Gets or sets the reservations associated with the room.
		/// </summary>
		public List<Reservation>? Reservations { get; set; }
	}
}
