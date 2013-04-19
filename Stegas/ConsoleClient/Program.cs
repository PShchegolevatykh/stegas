using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Core;
using System.Drawing.Imaging;

namespace ConsoleClient
{
    class Program
    {
        static void Main(string[] args)
        {
           // DoPaletteSortingMethod();
            DoLeastSignificantBitMethod();
        }

        private static void DoLeastSignificantBitMethod()
        {
            Image image = Bitmap.FromFile("lenna.bmp");
            SteganographyMethod leastSignificantBit = new LeastSignificantBitMethod();
            string textToHide = "You spin me right round!";
            Bitmap container = leastSignificantBit.Hide((Bitmap)image, textToHide);
            container.Save("lenna_lsb.bmp", ImageFormat.Bmp);
            string extractedText = leastSignificantBit.Extract(container);
            Console.WriteLine(textToHide.Equals(extractedText));
        }

        private static void DoPaletteSortingMethod()
        {
            Image image = Bitmap.FromFile("lenna.gif");
            if (image.Palette.Entries.Length == 0)
            {
                image.Dispose();
                image = null;
                Console.WriteLine("Cannot open that image file: No palette found.");
                return;
            }
            SteganographyMethod paletteSorting = new PaletteSortingMethod();
            string textToHide = "You spin me right round!";
            decimal countBytes = ((image.Palette.Entries.Length - 1) / 8) - 1;
            int capacity = (int)Math.Floor(countBytes);
            if (textToHide.Length > capacity)
            {
                Console.WriteLine("Cannot encode that message: Message is too long for this image.");
                return;
            }
            Bitmap container = paletteSorting.Hide((Bitmap)image, textToHide);
            container.Save("lenna_ps.gif", ImageFormat.Gif);
            string extractedText = paletteSorting.Extract(container);
            Console.WriteLine(textToHide.Equals(extractedText));
        }
    }
}
