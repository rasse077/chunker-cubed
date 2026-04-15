using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        Console.Title = "Cubed Installer";

        Console.WriteLine("Cubed Build Installer");

        Console.Write("game install folder location: ");
        string path = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("No path given, exiting...");
            return;
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string api = "https://builds.rebootfn.org/6.31.rar";
        string output = Path.Combine(path, "6.31.rar");

        Console.WriteLine("\nStarting download...\n");

        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(api, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            long total = response.Content.Headers.ContentLength ?? -1;
            long current = 0;

            using var stream = await response.Content.ReadAsStreamAsync();
            using var file = new FileStream(output, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[8192];
            int read;

            var start = DateTime.Now;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await file.WriteAsync(buffer, 0, read);
                current += read;

                double elapsed = (DateTime.Now - start).TotalSeconds;
                double speed = elapsed > 0 ? current / elapsed : 0;
                double eta = (speed > 0 && total > 0) ? (total - current) / speed : 0;

                if (total > 0)
                {
                    int percent = (int)(current * 100 / total);

                    Console.Write($"\rDownloading... {percent}%      ");
                    Console.Write($"\nETA: {TimeSpan.FromSeconds(eta):mm\\:ss}      ");
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
                else
                {
                    Console.Write($"\rDownloaded {current / 1024} KB");
                }
            }

            Console.WriteLine("\n\nDownload finished.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\nError: " + ex.Message);
        }

        Console.WriteLine("\nDone. Press enter to exit.");
        Console.ReadLine();
    }
}
