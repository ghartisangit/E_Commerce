using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs
{
    public class CheckOutDto
    {
        [Required]
        [Display(Name = "Full Name")]
        public string ShippingName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string ShippingEmail { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string ShippingPhone { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string ShippingAddress { get; set; }

        [Required]
        [Display(Name = "City")]
        public string ShippingCity { get; set; }

        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }
    }
}
