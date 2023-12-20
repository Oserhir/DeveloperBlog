using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TheBlogProject.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string? BlogUserId { get; set; }

        [Required] // These are annotations
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string Description { get; set; }

        [DataType(DataType.Date)] // Doesn't display time if UI has date picker
        [Display(Name = "Created Date")] // Label will say created date, instead of just created
        public DateTime? Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set; } // ? means this property can be nullable, since initially a blog won't have been updated

        [Display(Name = "Blog Image")]
        public byte[]? ImageData { get; set; }

        [Display(Name = "Image Type")]
        public string? ImageType { get; set; }


        [NotMapped] // Not going to be saved to Db
        public IFormFile? Image { get; set; }

        // Navigation properties

        [Display(Name = "Author")]
        public virtual BTUser? BlogUser { get; set; }  // Blog is child to Author (renamed BlogUser)
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();  // Blog is parent to a collection of Posts 
    }
}
