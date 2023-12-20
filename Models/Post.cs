using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheBlogProject.Enums;

namespace TheBlogProject.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string AuthorId { get; set; }

        [Required]
        [StringLength(75, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)] // {0} Title {2} 2  {1} 75
        public string Title { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)] 
        public string Abstract { get; set; } // Like Description

        [Required]
        public string Content { get; set; }

        [DataType(DataType.Date)] 
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime? Updated { get; set; }

        public ReadyStatus ReadyStatus { get; set; } // public bool IsReady { get; set; } // is Ready to Publish
        public string Slug { get; set; }

        [Display(Name = "Blog Image")]
        public byte[] ImageData { get; set; }

        [Display(Name = "Image Type")]
        public string contentType { get; set; } // .png .jpeg

        [NotMapped] // not store in database
        public IFormFile Image { get; set; }


        // 
        public virtual Blog Blog { get; set; }
        public virtual IdentityUser Author { get; set; }


        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

    }
}
