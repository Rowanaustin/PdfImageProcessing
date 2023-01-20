using AForge;
using AForge.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Text.RegularExpressions;
using ImageProcessor.Imaging;
using System.Xml.Linq;
using SimpleImageComparisonClassLibrary;
using SimpleImageComparisonClassLibrary.ExtensionMethods;

namespace PdfImageProcessing
{
    internal static class ImageComparison
    {
        public static List<ComparisonResult> CompareImages()
        {
            string[] filePaths = Directory.GetDirectories(Consts.IMAGE_OUTPUT_FOLDER);

            Console.WriteLine("Comparing " + filePaths.Length + " PDF images...");

            List<ComparisonResult> results = new();

            for (int i = 0; i < filePaths.Length; i++)
            {
                var imageLoc = filePaths[i] + Consts.IMAGE_ROT_OUTPUT_FOLDER;

                var imageRotZero = Directory.GetFiles(imageLoc, "*_r0.png", SearchOption.TopDirectoryOnly)[0];
                //var imageRotZero = Directory.GetFiles(imageLoc, "*.png", SearchOption.TopDirectoryOnly)[0];

                var imageName = imageRotZero.Replace(filePaths[i] + Consts.IMAGE_ROT_OUTPUT_FOLDER, "");
                imageName.Replace(".png", "").Replace(".pdf","");
                var image = AForge.Imaging.Image.FromFile(imageRotZero);
                image = ConvertToBitmap(image);
                List<string> others = new(filePaths.Skip(i + 1).ToArray());

                if (others.Count > 0)
                {
                    var message = "Comparing " + imageName + "\n";
                    Console.Write(message);
                }

                var firstStartTime = DateTime.Now;

                foreach (string other in others)
                {
                    var otherName = other.Replace(Consts.IMAGE_OUTPUT_FOLDER, "").Replace(".png","").Replace(".bmp","");
                    string[] rotations = Directory.GetFiles(other + Consts.IMAGE_ROT_OUTPUT_FOLDER, "*.png", SearchOption.TopDirectoryOnly);
                    //Console.WriteLine("Searching for " + other);
                    foreach (string rotation in rotations)
                    {
                        var rotationName = rotation.Split("_r")[1];
                        rotationName = rotationName.Split('.')[0];
                        var otherImage = AForge.Imaging.Image.FromFile(rotation);
                        otherImage = ConvertToBitmap(otherImage);
                        //Console.WriteLine("image is " + image.Width + "x" + image.Height + ", other is " + otherImage.Width + "x" + otherImage.Height);

                        //use this method to find the difference between two images (returns a float between 0 and 1)
                        var difference = ImageTool.GetPercentageDifference(imageRotZero, rotation, 3);

                            
                        if (difference < Consts.MAX_DIFFERENCE)
                        {
                            //Console.WriteLine("Difference is " + difference);
                            //Console.WriteLine(imageName + " has " + match.Similarity + " similarity with " + otherName + "(rotated " + rotationName + " degrees)");
                            var differenceImage = ImageMethods.GetDifferenceImage(image, otherImage);
                            Directory.CreateDirectory(Consts.RESULTS_FOLDER + imageName + otherName);
                            differenceImage.Save(Consts.RESULTS_FOLDER + imageName + otherName + "/" + (int)(difference*100) + ".bmp");
                            image.Save(Consts.RESULTS_FOLDER + imageName + otherName + "/" + imageName + "_rot" + rotationName + ".bmp");
                            otherImage.Save(Consts.RESULTS_FOLDER + imageName + otherName + "/" + otherName + ".bmp");

                            var matches = GetMatchesUsingCrop(image, otherImage);

                            foreach (BlockMatch match in matches)
                            {
                                // check similarity
                                if (match.Similarity > Consts.MIN_SIMILARITY)
                                {
                                    var comparisonText = imageName + " and " + otherName + "(rotated " + rotationName + ")";
                                    results.Add(new ComparisonResult(match.Similarity, difference, comparisonText));
                                    break;
                                }
                            }
                        }
                    }
                    Console.Write("|");
                }
                Console.Write("\n");

                if (i==0)
                {
                    var firstDuration = DateTime.Now - firstStartTime;
                    var timePerImage = firstDuration / others.Count;
                    var imagesLeft = others.Count - 1;
                    var totalComparisons = (imagesLeft * (imagesLeft + 1)) / 2;
                    var estDuration = (timePerImage * totalComparisons);


                    //HACK FOR NOW!!!!!!!
                    estDuration = estDuration.Multiply(1.7);

                    var estMinutes = (int)estDuration.TotalMinutes;
                    var estSeconds = estMinutes == 0 ? (int)estDuration.TotalSeconds : (int)estDuration.TotalSeconds % (int)estDuration.TotalMinutes;
                    Console.WriteLine("Estimated time to complete is " + estMinutes + "m" + estSeconds + "s");
                }
            }

            return results.OrderDescending().ToList();
        }

        private static List<BlockMatch> GetMatchesUsingCrop(Bitmap image, Bitmap otherImage)
        {
            if (image.Width != otherImage.Width || image.Height != otherImage.Height)
            {
                var width = image.Width > otherImage.Width ? otherImage.Width : image.Width;
                var height = image.Height > otherImage.Height? otherImage.Height : image.Height;

                //var cropCoordLeft = (factory.Image.Width - endSquareSize) / 2;
                //var cropCoordRight = endSquareSize;
                //var cropCoordTop = (factory.Image.Height - endSquareSize) / 2;
                //var cropCoordBottom = endSquareSize;

                //if (Consts.ROTATION_DEGREES * r == 90 || Consts.ROTATION_DEGREES * r == 270)
                //    cropCoordTop = (factory.Image.Width - endSquareSize) / 2;

                //Console.WriteLine("CropWidth: " + cropCoordLeft + ", CropHeight: " + cropCoordRight);

                var rect = new Rectangle(0, 0, width, height);
                return GetMatches(image.Clone(rect, PixelFormat.DontCare), otherImage.Clone(rect, PixelFormat.DontCare));
            }
            else
                 return GetMatches(image, otherImage);
        }

        private static List<BlockMatch> GetMatches(Bitmap image, Bitmap otherImage)
        {
            if (image.Width != otherImage.Width || image.Height != otherImage.Height)
            {
                Console.WriteLine("Images do not match in size");
                return new List<BlockMatch>();
            }    
            AForge.Imaging.SusanCornersDetector scd = new(30, 18);
            List<IntPoint> points = scd.ProcessImage(image);

            AForge.Imaging.ExhaustiveBlockMatching bm = new(8, 12);

            return bm.ProcessImage(image, points, otherImage);
        }
    
        public static Bitmap ConvertToBitmap(System.Drawing.Image image)
        {
            Bitmap copy = new(image.Width, image.Height, PixelFormat.Format24bppRgb);
            using (Graphics gr = Graphics.FromImage(copy))
            {
                gr.DrawImage(image, new Rectangle(0, 0, copy.Width, copy.Height));
            }
            return copy;
        }
    }


    public readonly struct ComparisonResult : IEquatable<ComparisonResult>, IComparable<ComparisonResult>
    {
        public readonly float Similarity;
        public readonly float HistogramDifference;
        public readonly string Description;

        public ComparisonResult(float similarity, float histoDiff, string description) 
        {
            Similarity = similarity;
            HistogramDifference = histoDiff;
            Description = description + " - CompSimilarity " + similarity*100 + "%, HistoSimilarity " + (100-histoDiff) + "%";
        }

        private int CompareTo(ComparisonResult right)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode() => throw new NotImplementedException();

        int IComparable<ComparisonResult>.CompareTo(ComparisonResult other)
        {
            return Similarity.CompareTo(other.Similarity);
        }

        public static bool operator ==(ComparisonResult left, ComparisonResult right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ComparisonResult left, ComparisonResult right)
        {
            return !(left == right);
        }

        public static bool operator <(ComparisonResult left, ComparisonResult right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(ComparisonResult left, ComparisonResult right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(ComparisonResult left, ComparisonResult right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(ComparisonResult left, ComparisonResult right)
        {
            return left.CompareTo(right) >= 0;
        }

        public bool Equals(ComparisonResult other)
        {
            return other.Similarity == Similarity;
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
