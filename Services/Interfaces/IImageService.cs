namespace TheBlogProject.Services.Interfaces
{
    public interface IImageService
    {
        Task<byte[]> EncodeImageAsync(IFormFile file); // This  is to encode images ""first time upload""
        Task<byte[]> EncodeImageAsync(string fileName); // This overload is to encode images ""already stored in project""
        string DecodeImage(byte[] data, string type); // Display image
        string ContentType(IFormFile file);
        int Size(IFormFile file);

    }
}
