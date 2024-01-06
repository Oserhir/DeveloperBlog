namespace TheBlogProject.Services.Interfaces
{
    public interface ISlugService
    {
        string UrlFriendly(string title);
        bool IsUnique(string slug);
        bool is_unique_category_slug(string slug);

    }
}
