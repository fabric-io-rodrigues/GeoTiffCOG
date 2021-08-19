using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTiffCOG.Struture
{
    public class MetaDataRaster
    {
        public int Width;
        public int Height;
        public int SamplesPerPixel;
        public int BitsPerSample;
        public double TiePointLon;
        public double TiePointLat;
        public double PixelScaleX;
        public double PixelScaleY;
        public IEnumerable<GeoPoint> Coordinates;
        public byte[][] Buffer;
    }
}
