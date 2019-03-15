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
    public interface IMaps
    {
        string MapName { get; set; }
        int Scale { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        int OffsetX { get; set; }
        int OffsetY { get; set; }
        int DistanceX { get; set; }
        int DistanceY { get; set; }
    }
    class MapInfo : IMaps
    {
        public string MapName { get; set; }
        public int Scale { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int DistanceX { get; set; }
        public int DistanceY { get; set; }
        public Point RenderPoint { get; set; }
        public void SetFill(int X, int Y)
        {
            RenderPoint = new Point(X, Y);
        }
    }
    class Program
    {
        static Graphics gfx;
        static void Main(string[] args)
        {

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            
            JObject mapsInfo;
            JArray mapArray;
            string mapInfoPath;
            string depoPath;

            // select map depo
            Console.Write("Name of Map Folder: ");
            string depoName = Console.ReadLine();

            if (Directory.Exists(depoName))
            {
                Directory.SetCurrentDirectory(depoName);
            }

            // Map folder doesn't exist, create new one
            else
            {
                Console.WriteLine("No folder " + depoName + " found.");
                Console.WriteLine("Create new folder? (Y or N): ");

                if (Console.ReadLine().ToUpper() == "Y")
                {
                    Directory.CreateDirectory(depoName);
                    Directory.SetCurrentDirectory(depoName);
                    File.WriteAllText(@"MapInfo.JSON", "{\nMaps:[]\n}");
                }
                else
                {
                    Environment.Exit(1);
                }
            }

            // sets path to be used later as maps are added 
            depoPath = Directory.GetCurrentDirectory();
            mapInfoPath = Path.GetFullPath(@"MapInfo.JSON");

            // gets array for map info appending
            using (StreamReader reader = File.OpenText(@"MapInfo.JSON"))
            {
                mapsInfo = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                mapArray = (JArray)mapsInfo["Maps"];
            }

            int menuSelection = 0;

            while (menuSelection != 3)
            {
                Directory.SetCurrentDirectory(depoPath);
                Console.WriteLine(" \n - Select map folder option - \n");
                Console.WriteLine("1: Add Map");
                Console.WriteLine("2: Generate JSON String");
                Console.WriteLine("3: Exit");

                menuSelection = Convert.ToInt32(Console.ReadLine());
                if (menuSelection < 1 || menuSelection > 3)
                {
                    Console.WriteLine("Enter a valid menu option.");
                }
                else
                {
                    switch (menuSelection)
                    {
                        case 1:
                            Console.WriteLine("Enter image name: ");
                            string fileName = Console.ReadLine();

                            // make sure Map Folder can be used, and write the selected image to tiles
                            if (GetMapFolder(fileName))
                            {
                                WriteMapTiles(fileName, mapInfoPath, mapArray);
                            }

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
            double ratio = 1.5;

            int aspectX = 1800;
            int aspectY = 1200;

            int minXRes = 3600;
            int minYRes = 2400;

            int basePPI = 100;

            int fillPixelsX = 0;
            int fillPixelsY = 0;

            Console.Write("Scale amount: ");
            int scale = Convert.ToInt32(Console.ReadLine());


            // get map distance in squares and get measurements to resize and tile the picture
            Console.Write("Distance (X): ");
            int distanceX = Convert.ToInt32(Console.ReadLine());

            Console.Write("Distance (Y): ");
            int distanceY = Convert.ToInt32(Console.ReadLine());
            MapInfo mapInfo = new MapInfo()
            {
                MapName = fileName,
                Scale = scale,
                DistanceX = distanceX,
                DistanceY = distanceY,

            };

            // makes object into string and adds it to suppied array
            // needs to overwrite map data if the map tiles are being overwritten
            string mapInfoJSON = JsonConvert.SerializeObject(mapInfo, Formatting.Indented);
            mapArray.Add(mapInfoJSON);

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
                int mapAmtX = (int)Math.Ceiling((double)distanceX / (aspectX * scale / basePPI));
                int mapAmtY = (int)Math.Ceiling((double)distanceY / (aspectY * scale / basePPI));

                // minimum resolution must be found for grid to fully display, and then add fill pixels to resize map to aspect ratio
                int newResX = basePPI * (distanceX / scale);
                int newResY = basePPI * (distanceY / scale);

                int mapPxX = newResX / mapAmtX;
                int mapPxY = newResY / mapAmtY;

                // scales image to hold correct number of maps within aspect ratio
                Bitmap firstResize = new Bitmap(image, new Size(newResX, newResY));

                // initializes variables to set desired width and height by adding blank space
                int destWidth = firstResize.Width;
                int destHeight = firstResize.Height;

                // gets the current resolution scale by the floored ratio of the current resolution (increased by one to increase PPI)
                int currRes = (destWidth / destHeight) + 1;

                // gets the next available resolution that stays within the correct aspect ratio
                if ((double)destWidth / destHeight != ratio || destWidth < minXRes || destHeight < minYRes)
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
                    while ((double)destWidth / destHeight != ratio)
                    {
                        currRes++;
                        destWidth = currRes * aspectX;
                        destHeight = currRes * aspectY;
                    }

                    fillPixelsX = destWidth - image.Width;
                    fillPixelsY = destHeight - image.Height;

                    Console.WriteLine("\nImage of " + firstResize.Width + "x" + firstResize.Height + " will be resized to " + destWidth + "x" + destHeight + ".");
                    Console.WriteLine("Scale original image or fill with blank space?");
                    Console.Write("1: Fill\n2: Resize\n3: Cancel\n:");
                    string gfxSelection = Console.ReadLine();

                    switch (gfxSelection)
                    {
                        case "1":
                            Console.Write("\nSelect location on map to render blank space");
                            if (fillPixelsX > 0 && fillPixelsY > 0)
                            {
                                Console.WriteLine("\n{0, -12} {0, 12}", "1: Top Left", "2: Top Right");
                                Console.WriteLine("\n{0, -12} {0, 12}", "3: Top Right", "4: Bottom Right");
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
                                        mapInfo.SetFill(0, fillPixelsY);
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
                                Console.Write("{0, -12} {0, 12}","1: Left", "2: Right\n\n Select: ");
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
                            break;

                        case "2":
                            break;

                        default:
                            break;
                    }
                    
                }

                Bitmap resizedImg = new Bitmap(destWidth, destHeight);
                gfx = Graphics.FromImage(resizedImg);
                gfx.DrawImage(firstResize, mapInfo.RenderPoint);

                
                Directory.SetCurrentDirectory(fileName + " Maps");
                PixelFormat format = image.PixelFormat;


                for (int i = 0; i < mapAmtX; i++)
                {
                    for (int j = 0; j < mapAmtY; j++)
                    {
                        Rectangle cropArea = new Rectangle(i * (resizedImg.Width / mapAmtX), j * (resizedImg.Height / mapAmtY), (resizedImg.Width / mapAmtX), (resizedImg.Height / mapAmtY));
                        resizedImg.Clone(cropArea, format).Save(fileName + "_" + i + "_" + j + ".png");
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
