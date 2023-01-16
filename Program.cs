﻿using PdfImageProcessing;
using SixLabors.ImageSharp;
using System.Xml.Linq;

internal class Program
{
    private static void Main(string[] _)
    {
        var fileName = "realOne.pdf";

        ImageUtils utils = new();

        var collectTimeStamp = DateTime.Now;

        //var images = ImageCollection.GetImages();
        var images = ImageCollection.GetImagesFromPdf(fileName);

        Directory.CreateDirectory(Consts.IMAGE_OUTPUT_FOLDER);
        utils.SaveModifiedPageImages(images);

        var collectDuration = DateTime.Now - collectTimeStamp;

        var compareTimeStamp = DateTime.Now;

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

        var textToFile = "Comparison Results from input file " + fileName + "\n";
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
    }
}