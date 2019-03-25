using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
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
        //public string MapInfoPath;
        public JArray MapArray;

        public MapReader()
        {

            StartDirectory = Path.Combine(Directory.GetCurrentDirectory(), @"Map Depos");
            if (!Directory.Exists(StartDirectory))
            {
                Directory.CreateDirectory(StartDirectory);
            }
            DepoDirectory = string.Empty;
            string[] depos = Directory.GetDirectories(StartDirectory);

            if (depos.Length < 1)
            {
                Console.Write("No map depo folders found in base directory.\nCreate new one? (Y or N): ");
                switch (Console.ReadLine().ToUpper())
                {
                    case "Y":
                        CreateDepo();
                        break;

                    default:
                        return;
                }
            }
            else
            {
                Console.WriteLine("\n Select Map Folder");
                int i;
                for (i = 0; i < depos.Length; i++)
                {
                    Console.WriteLine("{0}: {1}", i + 1, Path.GetFileName(depos[i]));
                }
                Console.WriteLine("{0}: {1}", i + 1, "Create new depo");

                int depoSelect = 0;
                try
                {
                    depoSelect = Convert.ToInt32(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.WriteLine("{0} is not a valid selection.");
                    return;
                }

                if (depoSelect < 1 || depoSelect > depos.Length + 2)
                {
                    Console.WriteLine("Invalid selection");
                    return;
                }
                else if (depoSelect == i + 1)
                {
                    CreateDepo();
                }
                else
                {
                    DepoDirectory = depos[depoSelect - 1];

                }
            }

            Console.WriteLine("Selecting {0}.", Path.GetFileName(DepoDirectory));
            if (SetDepoInfo())
            {
                OpenMainMenu();
            }
        }

        public void CreateDepo()
        {
            Console.Write("New map depo name: ");
            string mapDepoName = Console.ReadLine();


            string newDepoPath = Path.Combine(StartDirectory, mapDepoName);
            DepoDirectory = newDepoPath;
            Directory.CreateDirectory(newDepoPath);
            Directory.CreateDirectory(Path.Combine(DepoDirectory, @"Source Images"));
            Directory.CreateDirectory(Path.Combine(DepoDirectory, @"Resized Maps"));
            Directory.CreateDirectory(Path.Combine(DepoDirectory, @"Tiled Maps"));
        }

        public void OpenMainMenu()
        {
            string[] MainMenuOptions = { "Add New Map", "Create Map Tiles", "Export JSON String", "Change Display Settings", "Back" };

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
                        AddResizedMap();
                        break;
                    case "2":
                        AddTiledMap();
                        break;

                    case "3":

                        Console.WriteLine("Coming soon...");
                        break;
                    case "4":

                        Console.WriteLine("Coming soon...");
                        break;
                    case "5":

                        exitMenu = true;
                        break;
                }
            }
        }

        public bool SetDepoInfo()
        {
            if (DepoDirectory == string.Empty)
            {
                Console.WriteLine("ERROR: Map Folder directory has not been set.");
                return false;
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

                infoPath = Path.Combine(DepoDirectory, "MapInfo.JSON");

                if (!File.Exists(infoPath))
                {
                    Console.WriteLine("No Map Info file found, creating default MapInfo.JSON file.");
                    string JSONstring = "{\nMaps: []\n}";
                    File.WriteAllText(infoPath, JSONstring);
                }
                using (StreamReader reader = File.OpenText(infoPath))
                {
                    MapArray = (JArray)JToken.ReadFrom(new JsonTextReader(reader))["Maps"];
                }

                return true;
            }
        }

        public void AddMapJSON(IMaps map)
        {
            string json = JsonConvert.SerializeObject(map, Formatting.Indented);
            JToken newObj = JToken.Parse(json);
            MapArray.Add(newObj);
            string fileText = "{\nMaps: " + MapArray.ToString() + "\n}";
            File.WriteAllText(Path.Combine(DepoDirectory, @"MapInfo.JSON"), fileText);
        }

        public string ReadFilesInDirectory(string path)
        {
            List<string> files = new List<string>();
            foreach (string file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file) == ".png")
                {
                    files.Add(file);
                }
            }
            for (int i = 0; i < files.Count; i++)
            {
                Console.WriteLine("{0}: {1}", i + 1, Path.GetFileName(files[i]));
            }
            Console.Write("Select file: ");

            int input;
            try
            {
               input = Convert.ToInt32(Console.ReadLine());
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid selection");
                return "";
            }
            if (input > 0 && input < files.Count + 1)
            {
                return files[input - 1];
            }
            else
            {
                Console.WriteLine("Invalid Selection");
                return "";
            }
        }

        public void AddResizedMap()
        {
            Console.WriteLine("Select image name from the \"Source Images\" folder to resize:");
            string filePath = ReadFilesInDirectory(Path.Combine(DepoDirectory, @"Source Images"));
            if (filePath != "" && File.Exists(filePath))
            {
                ResizedMap map = new ResizedMap();
                if (map.PromptMapInfo())
                {
                    ResizeMap(filePath, map);
                    AddMapJSON(map);
                }

                Console.WriteLine("{0} has been resized and saved to \"Resized Maps\" folder.", Path.GetFileName(filePath));
            }
            else
            {
                Console.WriteLine("File {0} not found.", Path.GetFileName(filePath));
            }
        }

        //defines function that calculates the gcd with parameters a and b
        static int gcd(int a, int b)
        {
            //find the gcd using the Euclid’s algorithm
            while (a != b)
                if (a < b) b = b - a;
                else a = a - b;
            //since at this point a=b, the gcd can be either of them
            //it is necessary to pass the gcd to the main function
            return (a);
        }

        //defines function that reduces the fraction
        static int division(int a, int b)
        {
            int remainder = a, quotient = 0;
            while (remainder >= b)
            {
                remainder = remainder - b;
                quotient++;
            }
            return (quotient);
        }

        public void ResizeMap(string fileName, ResizedMap map)
        {
            string mapPath = Path.Combine(DepoDirectory, @"Source Images", fileName);
            // eventually this can create an image and place cropped versions of that image to other map images that have overlapping distance renderings
            // will need to work with tiled maps too
            using (FileStream pngStream = new FileStream(mapPath, FileMode.Open, FileAccess.Read))
            using (var image = new Bitmap(pngStream))
            {
                int aspectX = Display.MapSqX, aspectY = Display.MapSqY;
                int divisor = gcd(Display.MapSqX, Display.MapSqY);
                if (divisor != 1)
                {
                    aspectX = division(Display.MapSqX, divisor);
                    aspectY = division(Display.MapSqY, divisor);
                }

                aspectX *= Display.PixelDensity;
                aspectY *= Display.PixelDensity;

                int primaryAspect = (aspectX > aspectY) ? aspectX : aspectY;

                int scaledMapRatio = (int)Math.Ceiling((double)image.Width / primaryAspect);

                // amount of squares contained by each dimension
                double squaresX = (double)map.DistanceX / map.Scale;
                double squaresY = (double)map.DistanceY / map.Scale;

                // amount of pixels per square when scaled
                int sqPxlX = (int)Math.Ceiling(image.Width / squaresX);
                int sqPxlY = (int)Math.Ceiling(image.Width / squaresY);

                // total size of grid
                int scaledSizeX = (int)(Display.PixelDensity * squaresX);
                int scaledSizeY = (int)(Display.PixelDensity * squaresY);

                Size newSize = new Size(aspectX * scaledMapRatio, aspectY * scaledMapRatio);
                
                // scales image to hold correct number of maps within aspect ratio
                Bitmap firstResize = new Bitmap(image, newSize);

                //Bitmap resizedImg = new Bitmap(scaledSizeX, scaledSizeY);
                //gfx = Graphics.FromImage(firstResize);
                //gfx.DrawImage(firstResize, map.RenderPoint);
                string path = Path.Combine(DepoDirectory, @"Resized Maps", map.MapName + ".png");
                firstResize.Save(path);
                map.FilePath = path;
            }
        }

        void SetMapFill(int initWidth, int initHeight, ResizedMap map, Image image)
        {
            int currRes = initWidth / initHeight;
            double aspectRatio = (double)Display.MapSqX / Display.MapSqY;

            // gets the next available resolution that stays within the correct aspect ratio
            if ((double)initWidth / initHeight != aspectRatio || initWidth < Display.MinResX || initHeight < Display.MinResY)
            {
                // fit map by adding blank pixels
                if (initWidth < Display.MinResX)
                {
                    initWidth = Display.MinResX;
                }
                if (initHeight < Display.MinResY)
                {
                    initHeight = Display.MinResY;
                }
                while ((double)initWidth / initHeight != aspectRatio)
                {
                    currRes++;
                    initWidth = currRes * Display.MapSqX * Display.PixelDensity;
                    initHeight = currRes * Display.MapSqY * Display.PixelDensity;
                }

                int fillPixelsX = initWidth - image.Width;
                int fillPixelsY = initHeight - image.Height;

                Console.WriteLine("\nImage of " + image.Width + "x" + image.Height + " will be resized to " + initWidth + "x" + initHeight + ".");
                Console.WriteLine("Scale original image or fill with blank space?");
                Console.Write("\nSelect location on map to render blank space");
                if (fillPixelsX > 0 && fillPixelsY > 0)
                {
                    Console.WriteLine("\n{0, -12} {1, 12}", "1: Top Left", "2: Top Right");
                    Console.WriteLine("\n{0, -12} {1, 12}", "3: Bottom Left", "4: Bottom Right");
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

        }

        public void AddTiledMap()
        {
            Console.WriteLine("Select image to tile from the \"Resized Maps\" folder.");
            string imageSelect = Console.ReadLine();
            string filePath = Path.Combine(DepoDirectory, @"Resized Maps", imageSelect + ".png");
            if (File.Exists(filePath))
            {
                List<JObject> mapNames = new List<JObject>();
                JToken mapName;
                foreach (JObject obj in MapArray)
                {
                    mapName = obj.GetValue("FilePath");
                    if ((string)mapName == filePath)
                    {
                        mapNames.Add(obj);
                    }
                }
                if (mapNames.Count > 1)
                {
                    Console.WriteLine("This image is used by other maps, select which map info to use: ");
                    int i;
                    for (i = 0; i < mapNames.Count; i++)
                    {
                        Console.WriteLine("{0}: {1}", i + 1, mapNames[i]["MapName"]);
                    }
                    Console.WriteLine("{0}: {1}", i + 1, "Create new map info");
                    try
                    {
                        int select = Convert.ToInt32(Console.ReadLine());
                        if (select > 0 && select < mapNames.Count + 1)
                        {
                            ResizedMap resizedMap = mapNames[select - 1].ToObject<ResizedMap>();
                            TileMap(resizedMap);
                        }
                        else if (select == mapNames.Count + 1)
                        {
                            ResizedMap resizedMap = new ResizedMap();
                            if (resizedMap.PromptMapInfo())
                            {
                                TileMap(resizedMap);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid selection.");
                            return;
                        }
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("ERROR: Invalid int.");
                        return;
                    }
                }
                else if (mapNames.Count == 1)
                {
                    ResizedMap resizedMap = mapNames[0].ToObject<ResizedMap>();
                    TileMap(resizedMap);
                }
                else
                {
                    Console.WriteLine("No map info found for this image. Please enter it now.");
                    ResizedMap resizedMap = new ResizedMap();
                    if (resizedMap.PromptMapInfo())
                    {
                        TileMap(resizedMap);
                    }
                }
            }
        }

        public void TileMap(ResizedMap oldMap)
        {
            TiledMap map = new TiledMap(oldMap, Display.MapSqX, Display.MapSqY);
            Console.WriteLine("Scale for tiled maps: ");

            int num;
            string input = Console.ReadLine();
            if (map.CheckValidMapInput(input, out num, 1, true) && map.Scale > num)
            {
                map.Scale = num;
            }
            else
            {
                Console.WriteLine("ERROR: Invalid scale.");
                return;
            }

            map.TiledImageDirectory = Path.Combine(DepoDirectory, @"Tiled Maps", map.MapName);
            Directory.CreateDirectory(map.TiledImageDirectory);

            using (FileStream pngStream = new FileStream(oldMap.FilePath, FileMode.Open, FileAccess.Read))
            using (var image = new Bitmap(pngStream))
            {
                PixelFormat format = image.PixelFormat;

                for (int i = 0; i < map.TilesX; i++)
                {
                    for (int j = 0; j < map.TilesY; j++)
                    {
                        Rectangle cropArea = new Rectangle(i * (image.Width / map.TilesX), j * (image.Height / map.TilesY), (image.Width / map.TilesX), (image.Height / map.TilesY));
                        image.Clone(cropArea, format).Save(Path.Combine(map.TiledImageDirectory, map.MapName + "_" + i + "_" + j + ".png"));
                    }
                }

                // adds new serialized object to MapArray
                AddMapJSON(map);
            }
        }
    }


}
