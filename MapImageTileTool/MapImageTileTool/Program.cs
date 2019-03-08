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
            JObject mapSetInfo;
            //MapInfo currMapInfo;

            //select map depo
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
                    //MapInfo emptyMapInfo = new MapInfo();
                    File.WriteAllText(@"MapInfo.JSON", "{\nMaps:[]\n}");
                }
                else
                {
                    Environment.Exit(1);
                }
            }

            using (StreamReader reader = File.OpenText(@"MapInfo.JSON"))
            {
                mapSetInfo = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
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
                            Console.WriteLine("Enter image name: ");
                            string fileName = Console.ReadLine();

                            // make sure Map Folder can be used, and write the selected image to tiles
                            if (GetMapFolder(fileName))
                                WriteMapTiles(fileName, mapSetInfo);

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

            // select map depo
            // add new map
            // map gets tiled and named, put into folder

            // select map folder
            // generate JSON string with file names, width, height, depth and current displayed board
            // write new JSON file
        }

        // returns 'true' if image and its associated map folder exists, 'false' if it doesn't exist, and 'false' if the file is not allowed to be overwritten
        static bool GetMapFolder(string fileName)
        {
            Console.Write("Select image file in folder: ");

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
        static void WriteMapTiles(string fileName, JObject mapSetInfo)
        {
            Console.Write("Width: ");
            int width = Convert.ToInt32(Console.ReadLine());

            Console.Write("Height: ");
            int height = Convert.ToInt32(Console.ReadLine());

            Console.Write("Scale amount: ");
            decimal scale = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("\n - Map Offsets - \n");

            Console.Write("Selected Map X: ");
            int mapX = Convert.ToInt32(Console.ReadLine());

            Console.Write("Selected Map Y: ");
            int mapY = Convert.ToInt32(Console.ReadLine());


            MapInfo mapInfo = new MapInfo()
            {
                Width = width,
                Height = height,
                Scale = scale,
                OffsetX = mapX,
                OffsetY = mapY
            };

            JArray JSONmapArray = (JArray)mapSetInfo["Maps"];
            JSONmapArray.Add(JsonConvert.SerializeObject(mapInfo));

            /*

            using (FileStream pngStream = new FileStream(fileName + ".png", FileMode.Open, FileAccess.Read))
            using (var image = new Bitmap(pngStream))
            {
                Directory.SetCurrentDirectory(fileName + " Maps");
                System.Drawing.Imaging.PixelFormat format = image.PixelFormat;
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

            */
        }
    }
}
