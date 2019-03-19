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
                Console.WriteLine("Creating ReaderOptions.txt and setting to current directory.");
                readerOptions = Directory.GetCurrentDirectory();
                File.WriteAllText("ReaderOptions.txt", readerOptions);
                reader = new MapReader(readerOptions);
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
            Console.WriteLine("Program has concluded, press any key to exit.");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }
}
