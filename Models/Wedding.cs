using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
namespace WeddingPlanner.Models
{
    public class Wedding {
        // auto-implemented properties need to match the columns in your table
        // the [Key] attribute is used to mark the Model property being used for your table's Primary Key
        [Key]
        public int WeddingId { get; set; }
        // MySQL VARCHAR and TEXT types can be represeted by a string
        [Required(ErrorMessage = "Name is required!")]
        [MinLength(2, ErrorMessage="Name must be at least 2 characters!")]
        [Display(Name = "Wedder One:")] 
        public string NameOne { get; set; }
        [Required(ErrorMessage = "Name is required!")]
        [MinLength(2, ErrorMessage="Name must be at least 2 characters!")]
        [Display(Name = "Wedder Two:")] 
        public string NameTwo { get; set; }
        [Required(ErrorMessage = "Date is required!")]
        [Display(Name = "Date:")] 
        public string Date { get; set; }
        public int UserId { get; set; }
        //[DataType(DataType.Password)]
        [Display(Name = "Address:")] 
        [Required(ErrorMessage="Address must be provided!")]
        public string WeddingAddress { get; set; }
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;

        public List<Attendee> Attendees { get; set; }
    }
}