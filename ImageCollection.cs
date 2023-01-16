using Spire.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfImageProcessing
{
    internal static class ImageCollection
    {
        public static bool FileExists(string fileName)
        {
            return File.Exists(Consts.PDF_FOLDER + fileName);
        }
        public static Dictionary<int, List<System.Drawing.Image>> GetImagesFromPdf(string fileName)
        {
            Console.WriteLine("Importing PDF images...");
            PdfDocument pdf = new();
            pdf.LoadFromFile(Consts.PDF_FOLDER + fileName);

            Dictionary<int, List<Image>> keyValuePairs = new();

            int i = 1;

            foreach (PdfPageBase page in pdf.Pages)
            {
                var images = new List<Image>();
                images.AddRange(page.ExtractImages());
                keyValuePairs.Add(i, images);
                i++;
            }

            return keyValuePairs;
        }

        public static List<Image> GetImages()
        {
            List<string> filePaths = new( Directory.GetFiles(Consts.IMAGE_FOLDER, "*.png", SearchOption.TopDirectoryOnly) );
            filePaths.AddRange(Directory.GetFiles(Consts.IMAGE_FOLDER, "*.jpg", SearchOption.TopDirectoryOnly));
            filePaths.AddRange(Directory.GetFiles(Consts.IMAGE_FOLDER, "*.bmp", SearchOption.TopDirectoryOnly));

            var images = new List<Image>();

            foreach(string filePath in filePaths) 
            {
                images.Add(Image.FromFile(filePath));
            }

            return images;
        }
    }
}
