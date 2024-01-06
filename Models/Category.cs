using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheBlogProject.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string? CategoryUserId { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength =3)]
        public string title { get; set; }

        public string? slug { get; set; }

        [Display(Name = "Category Image")]
        public byte[]? ImageData { get; set; }

        [Display(Name = "Image Type")]
        public string? ImageType { get; set; }

        // Navigation properties
        [NotMapped]  
        public IFormFile? Image { get; set; }

        [Display(Name = "Author")]
        public virtual BTUser? CategoryUser { get; set; }   
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();  

    }
}
