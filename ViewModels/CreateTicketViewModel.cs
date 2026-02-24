using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechDesk.Models;

namespace TechDesk.ViewModels
{
    public class CreateTicketViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Priority")]
        public Priority Priority { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        // This holds the list of categories for the dropdown
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }
}
