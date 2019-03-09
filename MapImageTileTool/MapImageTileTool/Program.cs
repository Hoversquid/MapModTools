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
        public string MapName { get; set; }
        public decimal Scale { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            JObject mapsInfo;
            JArray mapArray;
            string mapInfoPath;

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
            Console.Write("Width: ");
            int width = Convert.ToInt32(Console.ReadLine());

            Console.Write("Height: ");
            int height = Convert.ToInt32(Console.ReadLine());

            Console.Write("Scale amount: ");
            decimal scale = Convert.ToInt32(Console.ReadLine());

            Console.Write("Selected Map Offset (X): ");
            int mapX = Convert.ToInt32(Console.ReadLine());

            Console.Write("Selected Map Offset (Y): ");
            int mapY = Convert.ToInt32(Console.ReadLine());
            
            MapInfo mapInfo = new MapInfo()
            {
                MapName = fileName,
                Width = width,
                Height = height,
                Scale = scale,
                OffsetX = mapX,
                OffsetY = mapY
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

                // testing resolution setting
                int destWidth = width;
                int destHeight = height;

                // fit map by adding blank pixels, must be at least 3600 by 2400 px
                // will need changes with custom aspect ratio, this is strict to 1800 by 1200 ratios
                if (destWidth < 3600)
                {
                    destWidth = 3600;
                }
                if (destHeight < 2400)
                {
                    destHeight = 2400;
                }
                while (destWidth % 1800 != 0 || destHeight % 1200 != 0 || destWidth <= destHeight)
                {
                    if ((destWidth % 1800) != 0)
                    {

                    }
                }

                Directory.SetCurrentDirectory(fileName + " Maps");
                PixelFormat format = image.PixelFormat;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        Rectangle cropArea = new Rectangle(i * (image.Width / width), j * (image.Height / height), (image.Width / width), (image.Height / height));
                        Bitmap bitmap = image.Clone(cropArea, format);
                        bitmap.Save(fileName + "_" + i + "_" + j + ".png");
                    }
                }
            }

            using (StreamWriter file = new StreamWriter(mapInfoPath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                // write full mapArray to file
                // needs to not have formatting characters and perserve original file layout
                Console.WriteLine(mapArray.ToString());
            }
        }
    }
}
