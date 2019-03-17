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
        int OffsetX { get; set; }
        int OffsetY { get; set; }
        int DistanceX { get; set; }
        int DistanceY { get; set; }
        string JSONString { get; set; }
        string FilePath { get; set; }
    }
    public class ResizedMap : IMaps
    {
        public string MapName { get; set; }
        public int Scale { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int DistanceX { get; set; }
        public int DistanceY { get; set; }
        public Point RenderPoint { get; set; }
        public string JSONString { get; set; }
        public void SetFill(int X, int Y)
        {
            RenderPoint = new Point(X, Y);
        }
        public string FilePath { get; set; }
    }
    class TiledMapInfo : IMaps
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
        public string JSONString { get; set; }
        public void SetFill(int X, int Y)
        {
            RenderPoint = new Point(X, Y);
        }
        public string FilePath { get; set; }

    }
}
