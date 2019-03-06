using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace MapImageTileTool
{
    class Program
    {
        static void Main(string[] args)
        {

            //select map depo
            Console.Write("Name of Map Folder: ");
            string depoName = Console.ReadLine();

            if (Directory.Exists(depoName))
            {
                Directory.SetCurrentDirectory(depoName);
            }
            else
            {
                Console.WriteLine("No folder " + depoName + " found.");
                Console.WriteLine("Create new folder? (Y or N): ");

                if (Console.ReadLine().ToUpper() == "Y")
                {
                    Directory.CreateDirectory(depoName);
                    Directory.SetCurrentDirectory(depoName);
                }
                else
                {
                    System.Environment.Exit(1);
                }
            }

            int menuSelection = 0;

            while (menuSelection != 3)
            {
                Console.WriteLine(" \n - Select map folder option - \n");
                Console.WriteLine("1: Add Map");
                Console.WriteLine("2: Generate JSON String");
                Console.WriteLine("3: Exit");

                menuSelection = Convert.ToInt32(Console.ReadLine());
                if (menuSelection < 0 || menuSelection > 3)
                {
                    Console.WriteLine("Enter a valid menu option.");
                }
                else
                {
                    switch (menuSelection)
                    {
                        case 1:
                            addMap();
                            break;
                        case 2:

                            break;
                        case 3:
                            break;
                        default:
                            break;
                    }
                }
            }

            

            Console.WriteLine("\n - Map Offsets - \n");

            Console.Write("Selected Map X: ");
            int mapX = Convert.ToInt32(Console.ReadLine());

            Console.Write("Selected Map Y: ");
            int mapY = Convert.ToInt32(Console.ReadLine());

            

            // select map depo
            // add new map
            // map gets tiled and named, put into folder

            // select map folder
            // generate JSON string with file names, width, height, depth and current displayed board
            // write new JSON file
            

            
            Console.ReadLine();
        }
        static void addMap()
        {
            Console.Write("Select image file in folder: ");
            string fileName = Console.ReadLine();

            Console.Write("Width: ");
            int width = Convert.ToInt32(Console.ReadLine());

            Console.Write("Depth: ");
            int depth = Convert.ToInt32(Console.ReadLine());

            Console.Write("Scale level (starting at tactical level, 0): ");
            int zLevel = Convert.ToInt32(Console.ReadLine());

            // check file name
            if (File.Exists(fileName + ".png"))
            {
                // add directory to place tiled map images if there isn't one
                if (!Directory.Exists(fileName + " Maps"))
                {
                    Directory.CreateDirectory(fileName + " Maps");
                    Console.WriteLine(fileName + " Maps created");
                }
            }

            using (FileStream pngStream = new FileStream(fileName + ".png", FileMode.Open, FileAccess.Read))
            using (var image = new Bitmap(pngStream))
            {
                Directory.SetCurrentDirectory(fileName + " Maps");
                System.Drawing.Imaging.PixelFormat format = image.PixelFormat;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < depth; j++)
                    {
                        Rectangle cropArea = new Rectangle(i * (image.Width / width), j * (image.Height / depth), (image.Width / width), (image.Height / depth));
                        Bitmap bitmap = image.Clone(cropArea, format);
                        bitmap.Save(fileName + "_" + i + "_" + j + ".png");
                    }
                }
            }
        }
    }

    class MapInfo
    {
        public string MapName;
        public int Scale;
        public int Width;
        public int Height;
        public int OffsetX;
        public int OffsetY;
    }

}
