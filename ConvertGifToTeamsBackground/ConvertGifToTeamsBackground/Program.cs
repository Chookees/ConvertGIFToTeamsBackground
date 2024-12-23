using System;
using System.IO;
using System.Threading.Tasks;

namespace ConvertGifToTeamsBackground
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            string user = Environment.UserName;
            string wholePath = Path.Combine("C:\\Users", user, "AppData\\Local\\Packages\\MSTeams_8wekyb3d8bbwe\\LocalCache\\Microsoft\\MSTeams\\Backgrounds\\Uploads");

            if (!Directory.Exists(wholePath))
            {
                Console.WriteLine($"The Teams path seems to not exist. Please make sure it exists: \n{wholePath}");
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("No file path provided.");
                return;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Given path {filePath} does not exist.");
                return;
            }

            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Extension != ".gif")
            {
                Console.WriteLine($"The file given is not a .gif, it is a {fileInfo.Extension}.");
                return;
            }

            if (fileInfo.Length > 20971520) // Equal to 20MB
            {
                Console.WriteLine("\nDue to the large size of the file, loading in Teams may take a little time.\n" +
                                  "Please be patient and wait a few seconds if you want to choose a background.");
                await Task.Delay(3000);
            }

            Guid guid = Guid.NewGuid();
            string newFileName = guid + ".jpg";
            string newFilePath = Path.Combine(wholePath, newFileName);
            string thumbFileName = guid + "_thumb.jpg";
            string thumbFilePath = Path.Combine(wholePath, thumbFileName);

            try
            {
                await CopyFileWithProgressAsync(fileInfo.FullName, newFilePath);
                await CopyFileWithProgressAsync(newFilePath, thumbFilePath);

                Console.WriteLine("Transforming your .gif to an animated Teams Background was successful.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something went wrong while copying the .gif to the right location. \nCopied to: {newFilePath}");
                Console.WriteLine($"Error: {e.Message}");
            }

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        private static async Task CopyFileWithProgressAsync(string sourcePath, string destinationPath)
        {
            byte[] buffer = new byte[81920]; // 80KB buffer
            using (FileStream sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
            using (FileStream destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
            {
                long totalBytes = sourceStream.Length;
                long totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await destinationStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                    DisplayProgress(totalBytesRead, totalBytes);
                }
            }
        }

        private static void DisplayProgress(long bytesRead, long totalBytes)
        {
            Console.Clear();
            double percentComplete = (double)bytesRead / totalBytes * 100;
            Console.WriteLine($"Progress: {percentComplete:F2}%");
        }
    }
}