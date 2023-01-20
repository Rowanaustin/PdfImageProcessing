using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace PdfImageProcessing
{
    internal class Consts
    {
        public static string PDF_FOLDER = "./pdf/";
        public static string PDF_OUTPUT_FOLDER = "./pdfOut/";
        public static string IMAGE_FOLDER = "./image/";
        public static string IMAGE_OUTPUT_FOLDER = "./imageOut/";
        public static string IMAGE_ROT_OUTPUT_FOLDER = "/rotations/";
        public static string RESULTS_FOLDER = "./results/";
        public static float ROTATION_DEGREES = 90f;
        public static float ROTATIONS = 360 / ROTATION_DEGREES;
        public static int RESULTS_COUNT = 30;

        public static float MIN_SIMILARITY = 0.99f;
        public static float MAX_DIFFERENCE = 0.8f;
    }
}
