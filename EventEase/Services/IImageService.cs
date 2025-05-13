using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EventEase.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile imageFile, string containerName);
        Task DeleteImageAsync(string imageUrl); // Add this method declaration
        // Potentially other image service methods
    }
}