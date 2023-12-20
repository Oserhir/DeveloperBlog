using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace TheBlogProject.Models
{
    public class Blog
    {
        public int Id { get; set; } // Primary Key
        public string AuthorID { get; set; }

        [Required]
        [StringLength(100,ErrorMessage = "The {0} must be at least {2} and at most {1} characters" , MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string Description { get; set; }

        [DataType(DataType.Date)] // treat Created as date not dateTime
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)] // treat Created as date not dateTime
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set; } // Allow null

        // Image

        [Display(Name = "Blog Image")]
        public byte[] ImageData { get; set; }

        [Display(Name = "Image Type")]
        public string contentType { get; set; } // .png .jpeg

        [NotMapped] // not store in database
        public IFormFile Image { get; set; }

        // Navigation Properties
       public virtual IdentityUser Author { get; set; }
       public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();


    }
}
