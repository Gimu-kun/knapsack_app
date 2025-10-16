using System;
using System.IO;
using System.Threading.Tasks;

public static class FileHelper
{
    public static async Task<string> ConvertToBase64Async(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
    }
}
