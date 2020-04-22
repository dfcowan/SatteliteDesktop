using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SatteliteDesktop
{
    internal class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

        private static int Main(string[] args)
        {
            Directory.CreateDirectory("c:\\temp\\SatteliteDesktop");
            try
            {
                string imageFileName = $"c:\\temp\\SatteliteDesktop\\goes16_abi_conus_geocolor_5000x3000_{DateTime.Now:yyyyMMddHHmm}.jpg";

                byte[] original = GetImage().Result;

                Font font = SystemFonts.CreateFont("Consolas", 100);

                using (var img = Image.Load(original))
                {
                    img.Mutate(x => x.DrawText(DateTime.Now.ToString("yyyy-MM-dd HH:mm"), font, Color.White.WithAlpha(0.7f), new PointF(100, img.Height - 400)));
                    img.Save(imageFileName);
                }

                SetImage(imageFileName);
                DeleteOldFiles();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }

        private static async Task<byte[]> GetImage()
        {
            using (var client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync("https://cdn.star.nesdis.noaa.gov/GOES16/ABI/CONUS/GEOCOLOR/5000x3000.jpg"))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
        }

        private static void SetImage(string filename)
        {
            uint SPI_SETDESKWALLPAPER = 0x14;
            uint SPIF_UPDATEINIFILE = 0x1;
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, filename, SPIF_UPDATEINIFILE);
        }

        private static void DeleteOldFiles()
        {
            foreach (string fileName in Directory.EnumerateFileSystemEntries("c:\\temp\\SatteliteDesktop"))
            {
                DateTimeOffset createTime = File.GetCreationTimeUtc(fileName);
                if ((DateTimeOffset.Now - createTime).TotalMinutes > 10)
                {
                    File.Delete(fileName);
                }
            }
        }
    }
}
