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
            reader = new MapReader();
            Console.WriteLine("Program has concluded, press any key to exit.");
            Console.ReadKey();
            Environment.Exit(1);
        }
    }
}
