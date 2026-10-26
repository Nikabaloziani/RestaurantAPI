using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RestaurantSERVICE.DTO.Restaurant
{
    public class CreateRestaurantDTO
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^[a-z0-9]+(?:-[a-z0-9]+)*$",
            ErrorMessage = "Use lowercase letters, numbers and hyphens."
        )]
        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;
    }
}
