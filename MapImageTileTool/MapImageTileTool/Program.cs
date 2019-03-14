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
    class MapInfo
    {
        public enum ResizeType { None, Stretch, FillTopLeft, FillBottomLeft, FillTopRight, FillBottomRight, FillTop, FillBottom, FillLeft, FillRight };
        public string MapName { get; set; }
        public int Scale { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int DistanceX { get; set; }
        public int DistanceY { get; set; }
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


            // initialize enum variable to save how map is to be resized
            MapInfo.ResizeType resizeType = MapInfo.ResizeType.None;

            MapInfo mapInfo = new MapInfo()
            {
                MapName = fileName,
                Scale = scale,
                DistanceX = distanceX,
                DistanceY = distanceY
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
                    Console.Write("1: Fill\n2: Resize\n3: Cancel\n:");
                    string gfxSelection = Console.ReadLine();

                    switch (gfxSelection)
                    {
                        case "1":
                            Console.Write("\nMap will be adjusted for aspect ratio and will contain blank space.\nWhere should the new pixels to be rendered?\n");
                            if (fillPixelsX > 0 && fillPixelsY > 0)
                            {
                                Console.Write("1: Top Left\n2: Bottom Left\n3: Top Right\n4: Bottom Right\nOther Input: Cancel\n:");
                                switch (Console.ReadLine())
                                {
                                    case "1":
                                        resizeType = MapInfo.ResizeType.FillTopLeft;
                                        break;
                                    case "2":
                                        resizeType = MapInfo.ResizeType.FillBottomLeft;
                                        break;
                                    case "3":
                                        resizeType = MapInfo.ResizeType.FillTopRight;
                                        break;
                                    case "4":
                                        resizeType = MapInfo.ResizeType.FillBottomRight;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else if (fillPixelsX > 0)
                            {
                                Console.Write("1: Left\n2: Right\n:");
                                switch (Console.ReadLine())
                                {
                                    case "1":
                                        resizeType = MapInfo.ResizeType.FillLeft;
                                        break;
                                    case "2":
                                        resizeType = MapInfo.ResizeType.FillRight;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else if (fillPixelsY > 0)
                            {
                                Console.Write("1: Top\n2: Bottom\n:");
                                switch (Console.ReadLine())
                                {
                                    case "1":
                                        resizeType = MapInfo.ResizeType.FillTop;
                                        break;
                                    case "2":
                                        resizeType = MapInfo.ResizeType.FillBottom;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            break;

                        case "2":
                            resizeType = MapInfo.ResizeType.Stretch;
                            Console.WriteLine("Scaling image to " + destWidth + "x" + destHeight + ".");
                            break;

                        default:
                            break;
                    }
                    
                }

                Bitmap resizedImg = new Bitmap(destWidth, destHeight);
                gfx = Graphics.FromImage(resizedImg);

                switch (resizeType)
                {
                    case MapInfo.ResizeType.FillLeft:
                        gfx.DrawImage(firstResize, fillPixelsX, 0, resizedImg.Width, resizedImg.Height);
                        break;
                    case MapInfo.ResizeType.FillRight:
                        gfx.DrawImage(firstResize, 0, 0, firstResize.Width, firstResize.Height);
                        break;
                    case MapInfo.ResizeType.FillTop:
                        gfx.DrawImage(firstResize, 0, fillPixelsY, firstResize.Width, firstResize.Height);
                        break;
                    case MapInfo.ResizeType.FillBottom:
                        gfx.DrawImage(firstResize, 0, 0, firstResize.Width, firstResize.Height);
                        break;
                    case MapInfo.ResizeType.FillTopLeft:
                        gfx.DrawImage(firstResize, fillPixelsX, fillPixelsY, firstResize.Width, firstResize.Height);
                        break;
                    case MapInfo.ResizeType.FillTopRight:
                        gfx.DrawImage(firstResize, 0, fillPixelsY, firstResize.Width, firstResize.Height);
                        break;
                    case MapInfo.ResizeType.FillBottomLeft:
                        gfx.DrawImage(firstResize, fillPixelsX, 0, firstResize.Width, firstResize.Height);
                        break;
                    case MapInfo.ResizeType.FillBottomRight:
                        gfx.DrawImage(firstResize, 0, 0, firstResize.Width, firstResize.Height);
                        break;
                    case MapInfo.ResizeType.None:
                        gfx.DrawImage(firstResize, 0, 0, firstResize.Width, firstResize.Height);
                        break;
                    default:
                        Console.WriteLine("Invalid resize type.");
                        break;
                }
                
                Directory.SetCurrentDirectory(fileName + " Maps");
                PixelFormat format = image.PixelFormat;
                for (int i = 0; i < mapAmtX; i++)
                {
                    for (int j = 0; j < mapAmtY; j++)
                    {
                        Rectangle cropArea = new Rectangle(i * (resizedImg.Width / mapAmtX), j * (resizedImg.Height / mapAmtY), (resizedImg.Width / mapAmtX), (resizedImg.Height / mapAmtY));
                        // Bitmap newImg = new Bitmap(resizedImg.Clone(cropArea, format), new Size(minXRes, minYRes));
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
