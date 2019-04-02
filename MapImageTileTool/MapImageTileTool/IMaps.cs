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
        int TilesX { get; set; }
        int TilesY { get; set; }
        int OffsetX { get; set; }
        int OffsetY { get; set; }
        int DistanceX { get; set; }
        int DistanceY { get; set; }
        string FilePath { get; set; }
        bool PromptMapInfo();
        bool CheckValidMapInput(string input, out int num, int lowerBound, bool isBounded);
    }
    public class ResizedMap : IMaps
    {
        public string MapName { get; set; }
        public int Scale { get; set; }
        public int TilesX { get; set; }
        public int TilesY { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int DistanceX { get; set; }
        public int DistanceY { get; set; }
        public Point RenderPoint { get; set; }
        public string FilePath { get; set; }
        public void SetFill(int X, int Y)
        {
            RenderPoint = new Point(X, Y);
        }
        public ResizedMap()
        {
        }

        public bool CheckValidMapInput(string input, out int num, int lowerBound, bool isBounded)
        {
            num = 0;
            int convertedInt = 0;
            try
            {
                convertedInt = Convert.ToInt32(input);
            }
            catch (FormatException)
            {
                Console.WriteLine("ERROR: Invalid integer input.");
                return false;
            }

            if (convertedInt < lowerBound && isBounded)
            {
                Console.WriteLine("ERROR: Input lower than lower limit.");
                return false;
            }
            num = convertedInt;
            return true;
        }
        public bool PromptMapInfo()
        {
            Console.Write("Map Name: ");
            MapName = Console.ReadLine();
            Console.Write("Map Scale: ");
            int num;
            if (!CheckValidMapInput(Console.ReadLine(), out num, 1, true))
            {
                return false;
            }
            else
            {
                Scale = num;
            }

            Console.Write("Distance X: ");
            if (!CheckValidMapInput(Console.ReadLine(), out num, 1, true))
            {
                return false;
            }
            else
            {
                DistanceX = num;
            }

            Console.Write("Distance Y: ");
            if (!CheckValidMapInput(Console.ReadLine(), out num, 1, true))
            {
                return false;
            }
            else
            {
                DistanceY = num;
            }

            Console.Write("Offset X: ");
            if (!CheckValidMapInput(Console.ReadLine(), out num, 0, false))
            {
                return false;
            }
            else
            {
                OffsetX = num;
            }

            Console.Write("Offset Y: ");
            if (!CheckValidMapInput(Console.ReadLine(), out num, 0, false))
            {
                return false;
            }
            else
            {
                OffsetY = num;
            }
            TilesX = 1;
            TilesY = 1;
            return true;
        }
    }
    public class TiledMap : ResizedMap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string TiledImageDirectory { get; set; }
        public TiledMap() : base() { }
        public TiledMap(ResizedMap map, int displayX, int displayY)
        {
            MapName = map.MapName;
            Scale = map.Scale;
            OffsetX = map.OffsetX;
            OffsetY = map.OffsetY;
            DistanceX = map.DistanceX;
            DistanceY = map.DistanceY;
            RenderPoint = map.RenderPoint;
            TilesX = (int)Math.Ceiling((double)DistanceX / displayX * Scale);
            TilesY = (int)Math.Ceiling((double)DistanceY / displayY * Scale);
        }
    }
}
