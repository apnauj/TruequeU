using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TruequeU.Enums;

namespace TruequeU.Models.DTOs
{
    public class ListingCreateDTO
    {
        [Required, MaxLength(100) ]
        public string title { get; set;}

        [Required, MaxLength(2000)]
        public string description { get; set; } = string.Empty;

        [Required, Precision(18, 2), Range(0.0, double.MaxValue)]

        public decimal price { get; set; }


        [Required]

        public Category category { get; set; }

        [Required]

        public ItemCondition condition { get; set; }


        [Required, MaxLength(100)]
        public string campusLocation { get; set; } = string.Empty; 














    }
}
