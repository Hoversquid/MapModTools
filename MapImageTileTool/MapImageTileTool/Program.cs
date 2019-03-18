using System;
using System.IO;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MapImageTileTool
{


    class Program
    {
        static MapReader reader;

        static void Main(string[] args)
        {
            string readerOptions;
            if (!File.Exists("ReaderOptions.txt"))
            {
                Console.WriteLine("ERROR: No ReaderOptions.txt file located.");
                Console.WriteLine("Add this file in the program's directory and include the path of the main map folder in it.\nPress any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            else
            {
                readerOptions = File.ReadAllText("ReaderOptions.txt");
                if (!Directory.Exists(readerOptions))
                {
                    Console.WriteLine("ERROR: Path provided in ReaderOptions.txt does not exist.\nPress any key to exit.");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                else
                {
                    reader = new MapReader(readerOptions);
                }
            }
        }
    }

    // returns 'true' if image and its associated map folder exists, 'false' if it doesn't exist, and 'false' if the file is not allowed to be overwritten
    static bool GetMapFolder(string fileName)
    {
        // check file name
        if (File.Exists(fileName + ".png"))
        {
            // add directory to place tiled map images if there isn't one
            if (!Directory.Exists(fileName + " Maps"))
            {
                Directory.CreateDirectory(fileName + " Maps");
                Console.WriteLine(fileName + " Maps created");
                return true;
            }

            // if directory exists, ask to overwrite
            else
            {
                Console.WriteLine("/" + fileName + " Maps exists. Overwrite maps? (Y or N): ");
                if (Console.ReadLine().ToUpper() == "Y")
                {
                    return true;
                }
                else
                    return false;
            }
        }
        else
        {
            Console.WriteLine(fileName + ".png" + " not found.");
            return false;
        }
    }

    // writeMapTiles creates the image tiles and writes the MapInfo to the JSON file
    static void WriteMapTiles(string fileName, string mapInfoPath, JArray mapArray)
    {
        // these hard-coded values will be replaced by whatever's in the mapset info file
        int displaySqX = 18;
        int displaySqY = 12;

        int minXRes = 3600;
        int minYRes = 2400;

        int pxlDensity = 100;

        int fillPixelsX = 0;
        int fillPixelsY = 0;

        Console.Write("Scale amount: ");
        int scale = Convert.ToInt32(Console.ReadLine());

        // get map distance in squares and get measurements to resize and tile the picture
        Console.Write("Distance (X): ");
        int distanceX = Convert.ToInt32(Console.ReadLine()) / scale;

        Console.Write("Distance (Y): ");
        int distanceY = Convert.ToInt32(Console.ReadLine()) / scale;

        MapInfo mapInfo = new MapInfo()
        {
            MapName = fileName,
            Scale = scale,
            DistanceX = distanceX,
            DistanceY = distanceY
        };

        // makes object into string and adds it to suppied array
        // needs to overwrite map data if the map tiles are being overwritten

        //string mapInfoJSON = JsonConvert.SerializeObject(mapInfo, Formatting.Indented);
        //mapArray.Add(mapInfoJSON);

        // deletes all files before making new ones in directory
        // this is probably dangerous, needs backup procedure instead
        DirectoryInfo dir = new DirectoryInfo(fileName + " Maps");
        foreach (FileInfo file in dir.GetFiles())
        {
            file.Delete();
        }

        // tiles map, needs to have resizing options to maintain aspect ratio
        // probably needs PPI adjusting options too
        using (FileStream pngStream = new FileStream(fileName + ".png", FileMode.Open, FileAccess.Read))
        using (var image = new Bitmap(pngStream))
        {
            int mapAmtX = (int)Math.Ceiling((double)distanceX / displaySqX);
            int mapAmtY = (int)Math.Ceiling((double)distanceY / displaySqY);

            // minimum resolution must be found for grid to fully display, and then add fill pixels to resize map to aspect ratio
            int newResX = pxlDensity * distanceX;
            int newResY = pxlDensity * distanceY;

            // scales image to hold correct number of maps within aspect ratio
            Bitmap firstResize = new Bitmap(image, new Size(newResX, newResY));

            // initializes variables to set desired width and height by adding blank space
            int destWidth = firstResize.Width;
            int destHeight = firstResize.Height;

            // gets the current resolution scale by the floored ratio of the current resolution (increased by one to increase PPI)
            int currRes = (destWidth / destHeight) + 1;
            double aspectRatio = (double)displaySqX / displaySqY;
            // gets the next available resolution that stays within the correct aspect ratio
            if ((double)destWidth / destHeight != aspectRatio || destWidth < minXRes || destHeight < minYRes)
            {
                // fit map by adding blank pixels, must be at least 3600 by 2400 px
                if (destWidth < minXRes)
                {
                    destWidth = 3600;
                }
                if (destHeight < minYRes)
                {
                    destHeight = 2400;
                }
                while ((double)destWidth / destHeight != aspectRatio)
                {
                    currRes++;
                    destWidth = currRes * displaySqX * pxlDensity;
                    destHeight = currRes * displaySqY * pxlDensity;
                }

                fillPixelsX = destWidth - image.Width;
                fillPixelsY = destHeight - image.Height;

                Console.WriteLine("\nImage of " + firstResize.Width + "x" + firstResize.Height + " will be resized to " + destWidth + "x" + destHeight + ".");
                Console.WriteLine("Scale original image or fill with blank space?");
                Console.Write("\nSelect location on map to render blank space");
                if (fillPixelsX > 0 && fillPixelsY > 0)
                {
                    Console.WriteLine("\n{0, -12} {0, 12}", "1: Top Left", "2: Top Right");
                    Console.WriteLine("\n{0, -12} {0, 12}", "3: Bottom Left", "4: Bottom Right");
                    Console.Write("Other Input: Cancel\n\n Select: ");

                    switch (Console.ReadLine())
                    {
                        case "1":
                            mapInfo.SetFill(fillPixelsX, fillPixelsY);
                            break;
                        case "2":
                            mapInfo.SetFill(0, fillPixelsY);
                            break;
                        case "3":
                            mapInfo.SetFill(fillPixelsX, 0);
                            break;
                        case "4":
                            mapInfo.SetFill(0, 0);
                            break;
                        default:
                            break;
                    }
                }
                else if (fillPixelsX > 0)
                {
                    Console.Write("{0, -12} {0, 12}", "1: Left", "2: Right\n\n Select: ");
                    switch (Console.ReadLine())
                    {
                        case "1":
                            mapInfo.SetFill(fillPixelsX, 0);
                            break;
                        case "2":
                            mapInfo.SetFill(0, 0);
                            break;
                        default:
                            break;
                    }
                }
                else if (fillPixelsY > 0)
                {
                    Console.Write("1: Top\n2: Bottom\n\nSelect: ");
                    switch (Console.ReadLine())
                    {
                        case "1":
                            mapInfo.SetFill(0, fillPixelsY);
                            break;
                        case "2":
                            mapInfo.SetFill(0, 0);
                            break;
                        default:
                            break;
                    }
                }
            }

            
        }
    }
    /*
        using (StreamWriter file = new StreamWriter(mapInfoPath))
        using (JsonTextWriter writer = new JsonTextWriter(file))
        {
            // write full mapArray to file
            // needs to not have formatting characters and perserve original file layout
            Console.WriteLine(mapArray.ToString());
        }
        */
}
}
