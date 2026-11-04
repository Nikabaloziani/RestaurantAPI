using System;
using System.ComponentModel.DataAnnotations;

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
            ErrorMessage = "Use lowercase letters, numbers and hyphens.")]
        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;

        // Required: an unassigned restaurant would bypass SaaS location quotas.
        [Required]
        public Guid? OrganizationId { get; set; }
    }
}
