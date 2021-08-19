using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTiffCOG.Struture
{
    public class GeoPoint : IEquatable<GeoPoint>
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Elevation { get; set; }

        public GeoPoint(double latitude, double longitude, double? altitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Elevation = (altitude.HasValue && double.IsNaN(altitude.Value)) ? null : altitude;
        }

        public GeoPoint(double latitude, double longitude) : this(latitude, longitude, null) { }

        public override string ToString()
        {
            return
                $"Lat/Lon: {Latitude} / {Longitude} "
                + (Elevation.HasValue ? $", Elevation: {Elevation.Value:F2}" : "");
        }

        public bool Equals(GeoPoint other)
        {
            if (this == null) return false;
            if (other == null) return false;

            return Math.Abs(this.Latitude - other.Latitude) < double.Epsilon
                  && Math.Abs(this.Longitude - other.Longitude) < double.Epsilon;
        }

        public override int GetHashCode()
        {
            int hashCode = -1416534245;
            hashCode = hashCode * -1521134295 + Latitude.GetHashCode();
            hashCode = hashCode * -1521134295 + Longitude.GetHashCode();
            return hashCode;
        }
    }
}
