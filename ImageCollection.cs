using PdfSharpCore.Pdf.IO;
using Spire.Pdf;
using Spire.Pdf.Conversion;
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

        public static Dictionary<string, List<List<Image>>> GetImages()
        {
            Dictionary<string, List<List<Image>>> pdfs = new();

            List<string> filePaths = new(Directory.GetFiles(Consts.PDF_OUTPUT_FOLDER, "*.pdf", SearchOption.TopDirectoryOnly));

            foreach (string filePath in filePaths)
            {
                pdfs.Add(filePath.Replace(Consts.PDF_OUTPUT_FOLDER,""), GetImagesFromPdf(filePath));
            }

            return pdfs;
        }

        public static List<List<Image>> GetImagesFromPdf(string fileName)
        {
            Console.WriteLine("Importing PDF images...");
            PdfDocument pdf = new();
            pdf.LoadFromFile(fileName);

            List<List<Image>> pages = new();

            int i = 1;

            foreach (PdfPageBase page in pdf.Pages)
            {
                var images = new List<Image>();
                images.AddRange(page.ExtractImages());
                pages.Add(images);
                i++;
            }

            return pages;
        }

        public static List<Image> GetImagesFromFolder()
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

        public static void SplitPdfs()
        {
            List<string> filePaths = new(Directory.GetFiles(Consts.PDF_FOLDER, "*.pdf", SearchOption.TopDirectoryOnly));

            foreach(string filePath in filePaths)
            {
                var pdfName = filePath.Replace(Consts.PDF_FOLDER, "");
                PdfSharpCore.Pdf.PdfDocument pdf = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);

                if (pdf.PageCount > 10)
                {
                    var pageNo = 0;
                    for (int i = 0; i < (pdf.PageCount / 10) + 1; i++)
                    {
                        var newPdf = new PdfSharpCore.Pdf.PdfDocument();
                        for (int j = 0; j < 10; j++)
                        {
                            newPdf.Pages.Add(pdf.Pages[pageNo]);
                            pageNo++;
                            if (pageNo >= pdf.PageCount)
                                break;
                        }

                        var saveLoc = Consts.PDF_OUTPUT_FOLDER + "/" + pdfName.Replace(".pdf", i + ".pdf");

                        newPdf.Save(saveLoc);
                    }
                }
                else
                {
                    pdf.Save(Consts.PDF_OUTPUT_FOLDER + "/" + pdfName);
                }
            }
        }
    }
}
