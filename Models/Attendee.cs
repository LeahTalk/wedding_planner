using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace WeddingPlanner.Models
{
    public class Attendee
    {
        // auto-implemented properties need to match the columns in your table
        // the [Key] attribute is used to mark the Model property being used for your table's Primary Key
        [Key]
        public int AttendeeId { get; set; }
        // MySQL VARCHAR and TEXT types can be represeted by a string
        public int WeddingId { get; set; }

        public int UserId { get; set; }

        public Wedding Wedding { get; set; }

        public User User { get; set; }

    }
}