using PdfImageProcessing;
using SixLabors.ImageSharp;
using System.Xml.Linq;

internal class Program
{
    private static void Main(string[] _)
    {
        ImageUtils utils = new();

        if (!Directory.Exists(Consts.PDF_FOLDER))
            Directory.CreateDirectory(Consts.PDF_FOLDER);
        if (!Directory.Exists(Consts.PDF_OUTPUT_FOLDER))
            Directory.CreateDirectory(Consts.PDF_OUTPUT_FOLDER);
        if (!Directory.Exists(Consts.IMAGE_FOLDER))
            Directory.CreateDirectory(Consts.IMAGE_FOLDER);


        var fileCount = Directory.GetFiles(Consts.PDF_FOLDER, "*.pdf", SearchOption.TopDirectoryOnly).Length;

        if (fileCount < 1) 
        {
            Console.WriteLine("No input PDFs in input folder.");
            Console.WriteLine("(" + Consts.PDF_FOLDER + ")");
            Console.ReadKey();
            return;
        }

        var collectTimeStamp = DateTime.Now;

        ImageCollection.SplitPdfs();

        //var images = ImageCollection.GetImages();

        //var images = ImageCollection.GetImagesFromPdf(fileName);

        var images = ImageCollection.GetImages();

        Directory.CreateDirectory(Consts.IMAGE_OUTPUT_FOLDER);
        utils.SaveModifiedPageImages(images);

        var collectDuration = DateTime.Now - collectTimeStamp;

        utils.CleanUpPdfs();

        var compareTimeStamp = DateTime.Now;

        var imageCount = Directory.GetDirectories(Consts.IMAGE_OUTPUT_FOLDER).Length;

        var results = ImageComparison.CompareImages();

        utils.CleanUpImages();

        var compareDuration = DateTime.Now - compareTimeStamp;

        var collectMinutes = (int)collectDuration.TotalMinutes;
        var collectSeconds = collectMinutes == 0 ? (int)collectDuration.TotalSeconds : (int)collectDuration.TotalSeconds % (int)collectDuration.TotalMinutes;
        var compareMinutes = (int)compareDuration.TotalMinutes;
        var compareSeconds = compareMinutes == 0 ? (int)compareDuration.TotalSeconds : (int)compareDuration.TotalSeconds % (int)compareDuration.TotalMinutes;

        Console.WriteLine("---------------------------------------");
        Console.WriteLine("Collecting Images took " + collectMinutes + "m" + collectSeconds + "s");
        Console.WriteLine("---------------------------------------");

        Console.WriteLine("---------------------------------------");
        Console.WriteLine("Comparing Images took " + compareMinutes + "m" + compareSeconds + "s");
        Console.WriteLine("---------------------------------------");

        if (!Directory.Exists(Consts.RESULTS_FOLDER))
            Directory.CreateDirectory(Consts.RESULTS_FOLDER);

        var textToFile = "Comparison Results from " + imageCount + " images extracted from " + fileCount + " files\n";
        textToFile += "Found " + results.Count + " matches - showing top " + Consts.RESULTS_COUNT + "\n";

        if (results.Count > 0)
        {
            Console.WriteLine(results.Count + " results");
            var counter = 0;
            while (counter < Consts.RESULTS_COUNT && counter < results.Count)
            {
                textToFile += results[counter].Description + "\n";
                Console.WriteLine(results[counter].Description);
                counter++;
            }
        }
        else
        {
            textToFile += "No close matches";
            Console.WriteLine("No close matches");
        }

        textToFile += "\n";
        textToFile += "Collecting Images took " + collectMinutes + "m" + collectSeconds + "s";
        textToFile += "Comparing Images took " + compareMinutes + "m" + compareSeconds + "s";

        var textFile = File.CreateText(Consts.RESULTS_FOLDER + "results.txt");
        textFile.Write(textToFile);
        textFile.Close();
        

        Console.ReadKey();
    }
}