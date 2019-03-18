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
    public class DisplayInfo
    {
        public int PixelDensity { get; set; }
        public int MapSqX { get; set; }
        public int MapSqY { get; set; }
        public int MinResX { get; set; }
        public int MinResY { get; set; }
        public DisplayInfo()
        {
            PixelDensity = 100;
            MapSqX = 18;
            MapSqY = 12;
            MinResX = 3600;
            MinResY = 2400;
        }
    }
    class MapReader
    {
        static Graphics gfx;
        public string StartDirectory;
        public string DepoDirectory;
        public DisplayInfo Display;
        public string MapInfoPath;
        public JArray MapArray;
        public string[] MainMenuOptions = { "Add New Map", "Create Map Tiles", "Export JSON String", "Change Display Settings", "Back" };

        public MapReader(string dir)
        {
            StartDirectory = Path.GetDirectoryName(dir);
            DepoDirectory = string.Empty;
            string[] depos = Directory.GetDirectories(StartDirectory);


            Console.WriteLine("\n Select Map Folder");
            for (int i = 0; i < depos.Length; i++)
            {
                Console.WriteLine("{0}: {1}", i + 1, depos[i]);
            }

            int depoSelect = 0;
            try
            {
                depoSelect = Convert.ToInt32(Console.ReadLine());
            }
            catch (FormatException)
            {
                Console.WriteLine("{0} is not a valid selection.");
            }

            if (depoSelect < 1 || depoSelect > depos.Length + 1)
            {
                Console.WriteLine("Invalid selection");
            }
            else
            {
                DepoDirectory = Path.GetDirectoryName(depos[depoSelect - 1]);
                Console.WriteLine("Selecting {0}.", DepoDirectory);
                SetDepoInfo();
                SetMapArray();
            }
        }

        public void OpenMainMenu()
        {
            bool exitMenu = false;
            while (!exitMenu)
            {
                Console.WriteLine("\n- Select Map Option -\n");
                for (int i = 0; i < MainMenuOptions.Length; i++)
                {
                    Console.WriteLine("{0}: {1}", i + 1, MainMenuOptions[i]);
                }
                switch (Console.ReadLine())
                {
                    case "1":

                        break;
                    case "2":
                        break;
                    case "3":
                        break;
                    case "4":
                        break;
                    case "5":

                        exitMenu = true;
                        break;
                }
            }
        }

        public void SetDepoInfo()
        {
            if (DepoDirectory == string.Empty)
            {
                Console.WriteLine("ERROR: Map Folder directory has not been set.");
            }
            else
            {
                string infoPath = Path.Combine(DepoDirectory, "DisplayInfo.JSON");
                if (File.Exists(infoPath))
                {
                    Display = JsonConvert.DeserializeObject<DisplayInfo>(File.ReadAllText(infoPath));
                }
                else
                {
                    Console.WriteLine("No Display Info file found, creating default DisplayInfo.JSON file.");
                    Display = new DisplayInfo();
                    File.WriteAllText(infoPath, JsonConvert.SerializeObject(Display));
                }

                // change this to create JArray Map Collection
                infoPath = Path.Combine(DepoDirectory, "MapInfo.JSON");
            }
        }

        public void SetMapArray()
        {
            string infoPath = Path.Combine(DepoDirectory, "MapInfo.JSON");
            if (!File.Exists(infoPath))
            {
                Console.WriteLine("No Map Info file found, creating default MapInfo.JSON file.");
                string JSONstring = "{\nMaps: []\n}";
                File.WriteAllText(infoPath, JSONstring);
                MapArray = JArray.Parse(JSONstring);
            }

            // read in "Maps" array
            using (StreamReader reader = File.OpenText(infoPath))
            {
                JObject mapsInfo = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                MapArray = (JArray)mapsInfo["Maps"];
            }
        }

        public void AddResizedMap()
        {
            Console.WriteLine("Select image name from the \"Source Images\" folder to resize:");
            string imageName = Console.ReadLine();
            string filePath = Path.Combine(DepoDirectory, @"Source Images", imageName);
            if (File.Exists(filePath))
            {
                ResizedMap newMap = new ResizedMap();
                ResizeMap(imageName, newMap);
                AddMapJSON(newMap);
                Console.WriteLine("{0} has been resized and saved to \"Resized Images\" folder.", imageName);
            }
        }

        public void AddMapJSON(IMaps map)
        {
            string json = JsonConvert.SerializeObject(map, Formatting.Indented);
            map.Token = JToken.FromObject(map);
            MapArray.Add(map.Token);
        }

        public void AddTiledMap(ResizedMap oldMap)
        {

            // makes a copy of a resized map image and tiles it for aspect ratio
            // scale must be changed to fit what the tiles are displaying
            TiledMap map = new TiledMap(oldMap);
            Console.WriteLine("Scale for tiled map: ");

            //int num;

            //if (map.CheckValidMapInput(Console.ReadLine(), out num, 1, true))
            //{
            //    map.Scale = num;
            //}
            //else
            //{
            //    return;
            //}

            map.TiledImageDirectory = Path.Combine(DepoDirectory, @"Tiled Maps", map.MapName);
            Directory.CreateDirectory(map.TiledImageDirectory);

            using (FileStream pngStream = new FileStream(map.FilePath, FileMode.Open, FileAccess.Read))
            using (var image = new Bitmap(pngStream))
            {
                PixelFormat format = image.PixelFormat;
                map.Width = (int)Math.Ceiling((double)map.DistanceX / Display.MapSqX);
                map.Height = (int)Math.Ceiling((double)map.DistanceY / Display.MapSqY);

                for (int i = 0; i < map.Width; i++)
                {
                    for (int j = 0; j < map.Height; j++)
                    {
                        Rectangle cropArea = new Rectangle(i * (image.Width / map.Width), j * (image.Height / map.Height), (image.Width / map.Width), (image.Height / map.Height));
                        image.Clone(cropArea, format).Save(Path.Combine(map.TiledImageDirectory, map.MapName + "_" + i + "_" + j + ".png"));
                    }
                }

                // adds new serialized object to MapArray
                AddMapJSON(map);
            }

        }

        public void ResizeMap(string fileName, ResizedMap map)
        {
            // eventually this can create an image and place cropped versions of that image to other map images that have overlapping distance renderings
            // will need to work with tiled maps too
            using (FileStream pngStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var image = new Bitmap(pngStream))
            {

                // minimum resolution must be found for grid to fully display, and then add fill pixels to resize map to aspect ratio
                int newResX = Display.PixelDensity * (map.DistanceX / map.Scale);
                int newResY = Display.PixelDensity * (map.DistanceY / map.Scale);

                // scales image to hold correct number of maps within aspect ratio
                Bitmap firstResize = new Bitmap(image, new Size(newResX, newResY));

                // initializes variables to set desired width and height by adding blank space
                int destWidth = firstResize.Width;
                int destHeight = firstResize.Height;

                // gets the current resolution scale by the floored ratio of the current resolution (increased by one to increase PPI)
                int currRes = (destWidth / destHeight) + 1;
                double aspectRatio = (double)Display.MapSqX / Display.MapSqY;

                // gets the next available resolution that stays within the correct aspect ratio
                if ((double)destWidth / destHeight != aspectRatio || destWidth < Display.MinResX || destHeight < Display.MinResY)
                {
                    // fit map by adding blank pixels
                    if (destWidth < Display.MinResX)
                    {
                        destWidth = Display.MinResX;
                    }
                    if (destHeight < Display.MinResY)
                    {
                        destHeight = Display.MinResY;
                    }
                    while ((double)destWidth / destHeight != aspectRatio)
                    {
                        currRes++;
                        destWidth = currRes * Display.MapSqX * Display.PixelDensity;
                        destHeight = currRes * Display.MapSqY * Display.PixelDensity;
                    }

                    int fillPixelsX = destWidth - image.Width;
                    int fillPixelsY = destHeight - image.Height;

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
                                map.SetFill(fillPixelsX, fillPixelsY);
                                break;
                            case "2":
                                map.SetFill(0, fillPixelsY);
                                break;
                            case "3":
                                map.SetFill(fillPixelsX, 0);
                                break;
                            case "4":
                                map.SetFill(0, 0);
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
                                map.SetFill(fillPixelsX, 0);
                                break;
                            case "2":
                                map.SetFill(0, 0);
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
                                map.SetFill(0, fillPixelsY);
                                break;
                            case "2":
                                map.SetFill(0, 0);
                                break;
                            default:
                                break;
                        }
                    }
                }

                Bitmap resizedImg = new Bitmap(destWidth, destHeight);
                gfx = Graphics.FromImage(resizedImg);
                gfx.DrawImage(firstResize, map.RenderPoint);
                string path = Path.Combine(DepoDirectory, @"Resized Images", map.MapName + "_" + destWidth + "x" + destHeight);
                resizedImg.Save(path);
                map.FilePath = path;
            }
        }
    }
}
