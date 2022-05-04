using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTiffCOG.Struture
{
    public class GEOTiffProperties
    {
        public ModelType ModelType { get; set; }
        public RasterType RasterType { get; set; }
        public ushort GTCitationGeo { get; set; }
        public ushort GeogCitation { get; set; }
        public ushort GeodeticDatum { get; set; }
        public ushort PrimeMeridian { get; set; }
        public AngularUnits AngularUnit { get; set; }
        public ushort Ellipsoid { get; set; }
        public double SemiMajorAxis { get; set; }
        public double SemiMinorAxis { get; set; }
        public double InvFlattening { get; set; }
        public ushort PrimeMeridianLong { get; set; }
        public LinearUnits LinearUnit { get; set; }
        public ushort CoordinateSystemId { get; set; }
        public string GdalMetadata { get; set; }
        public string GeotiffAsciiParams { get; set; }


    }
}
