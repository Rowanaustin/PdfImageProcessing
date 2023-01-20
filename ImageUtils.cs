using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using ImageProcessor;
using ImageProcessor.Imaging;
using SixLabors.ImageSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace PdfImageProcessing
{
    internal class ImageUtils
    {
        private readonly ImageFactory factory;

        private Blob[] blobs;
        public ImageUtils()
        {
            factory = new ImageFactory();
        }

        public void SaveModifiedPageImages(Dictionary<string,List<List<System.Drawing.Image>>> images)
        {
            Dictionary<string, System.Drawing.Image> imagesForProc = new();
            Console.WriteLine("Saving " + images.Count + " pdfs of images");

            foreach (string file in images.Keys)
            {
                var i = 0;
                foreach (List<System.Drawing.Image> page in images[file])
                {
                    var j = 0;
                    foreach (var image in page)
                    {
                        var name = file + "P" + i + "image" + j;
                        imagesForProc.Add(name, image);

                        AddSections(imagesForProc, name);

                        j++;
                    }
                    i++;
                }
            }

            SaveRotatedVersions(imagesForProc);
        }

        public void SaveModifiedPageImages(List<System.Drawing.Image> images)
        {
            Dictionary<string, System.Drawing.Image> imagesForProc = new();

            var i = 0;
            foreach (var image in images)
            {
                imagesForProc.Add("image" + i, image);

                AddSections(imagesForProc, "image" + i);

                i++;
            }

            SaveRotatedVersions(imagesForProc);
        }

        public void CleanUpPdfs ()
        {
            Directory.Delete(Consts.PDF_OUTPUT_FOLDER, true);
        }

        public void CleanUpImages()
        {
            Directory.Delete(Consts.IMAGE_OUTPUT_FOLDER, true);
        }


        private void AddSections(Dictionary<string, System.Drawing.Image> existing, string imageName)
        {
            var bitmap = ImageComparison.ConvertToBitmap(existing[imageName]);

            UnmanagedImage grayImage;

            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                grayImage = UnmanagedImage.FromManagedImage(bitmap);
            }
            else
            {
                grayImage = UnmanagedImage.Create(bitmap.Width, bitmap.Height,
                    PixelFormat.Format8bppIndexed);
                Grayscale.CommonAlgorithms.BT709.Apply(UnmanagedImage.FromManagedImage(bitmap), grayImage);
            }

            // create filter
            Invert filter = new();
            // apply the filter
            filter.ApplyInPlace(grayImage);

            // grayImage.ToManagedImage(true).Save(Consts.IMAGE_OUTPUT_FOLDER + name + "/" + "gs.bmp");

            // create an instance of blob counter algorithm
            BlobCounterBase bc = new BlobCounter
            {
                // set filtering options
                FilterBlobs = true,
                MinWidth = bitmap.Width / 4,
                MinHeight = bitmap.Height / 4,
                MaxWidth = bitmap.Width / 2,
                MaxHeight = bitmap.Height / 2,
                BackgroundThreshold = System.Drawing.Color.FromArgb(50, 50, 50),
                //BackgroundThreshold = System.Drawing.Color.Black,
                // set ordering options
                ObjectsOrder = ObjectsOrder.Size
            };

            bc.ProcessImage(grayImage);
            blobs = bc.GetObjectsInformation();

            if (blobs.Length < 2)
                return;

            var blobsRects = bc.GetObjectsRectangles();
            BitmapData data = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            // process each blob
            for (int i = 0; i < blobs.Length; i++)
            {
                //Console.WriteLine(imageName + " " + blobsRects[i].ToString());
                Drawing.Rectangle(data, blobsRects[i], System.Drawing.Color.OrangeRed);

                var another = bitmap.Clone(blobsRects[i], PixelFormat.DontCare);
                existing.Add(imageName + "blob" + i, another);

                if (i == blobs.Length - 1)
                {
                    bitmap.Save(Consts.IMAGE_OUTPUT_FOLDER + imageName + "_blobs.bmp");
                }
            }

            bitmap.UnlockBits(data);
        }

        private void SaveRotatedVersions(Dictionary<string, System.Drawing.Image> images) 
        {

            foreach (string name in images.Keys)
            {
                factory.Load(images[name]);

                SaveOriginalImage(name);

                for (int r = 0; r < Consts.ROTATIONS; r++)
                {
                    factory.Load(images[name]);
                    factory.Rotate(Consts.ROTATION_DEGREES * r);

                    SaveProcessedImage(name + "_r" + (r * Consts.ROTATION_DEGREES), name);

                    //Console.WriteLine("New image: " + factory.Image.Width + " x " + factory.Image.Height);
                    //Console.WriteLine(("\n"));
                }
            }
        }

        private void SaveOriginalImage(string name)
        {
            factory.Save(Consts.IMAGE_OUTPUT_FOLDER + name + "/image.png");
            Directory.CreateDirectory(Consts.IMAGE_OUTPUT_FOLDER + name + Consts.IMAGE_ROT_OUTPUT_FOLDER);
        }

        private void SaveProcessedImage(string name, string imageName)
        {
            //Console.WriteLine(Consts.IMAGE_OUTPUT_FOLDER + imageName + Consts.IMAGE_ROT_OUTPUT_FOLDER + name + ".png");
            factory.Save(Consts.IMAGE_OUTPUT_FOLDER + imageName + Consts.IMAGE_ROT_OUTPUT_FOLDER + name + ".png");
        }
    }
}
