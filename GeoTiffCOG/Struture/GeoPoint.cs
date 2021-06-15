using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTiffCOG.Struture
{
    public class GeoPoint : IEquatable<GeoPoint>
    {
        public int? Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Elevation { get; set; }

        /// <summary>
        /// When this point is part of a List and ComputePointsDistances is called, this field
        /// stores the distance from this point to origin point in meters.
        /// </summary>
        public double? DistanceFromOriginMeters { get; set; }

        public GeoPoint(int? id, double latitude, double longitude, double? altitude)
            : this(latitude, longitude, altitude)
        {
            this.Id = id;
        }
        public GeoPoint(double latitude, double longitude, double? altitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Elevation = (altitude.HasValue && double.IsNaN(altitude.Value)) ? null : altitude;
            this.DistanceFromOriginMeters = null;
        }

        public GeoPoint(double latitude, double longitude) : this(latitude, longitude, null) { }
        public GeoPoint(int? id, double latitude, double longitude) : this(id, latitude, longitude, null) { }
        public GeoPoint() : this(0, 0) { }

        public GeoPoint Clone(double? newHeight = null)
        {
            return new GeoPoint
            {
                Id = this.Id
                ,
                DistanceFromOriginMeters = this.DistanceFromOriginMeters
                ,
                Elevation = newHeight ?? this.Elevation
                ,
                Latitude = this.Latitude
                ,
                Longitude = this.Longitude
            };

        }

        public double DistanceSquaredTo(GeoPoint pt)
        {
            return (pt.Longitude - Longitude) * (pt.Longitude - Longitude)
                    + (pt.Latitude - Latitude) * (pt.Latitude - Latitude);
        }


        public static GeoPoint Zero => new GeoPoint(0d, 0d, 0d);
        public static GeoPoint UnitX => new GeoPoint(1d, 0d, 0d);
        public static GeoPoint UnitY => new GeoPoint(0d, 1d, 0d);
        public static GeoPoint UnitZ => new GeoPoint(0d, 0d, 1d);

        public static GeoPoint operator *(GeoPoint left, float right)
        {
            return new GeoPoint(left.Latitude * right, left.Longitude * right, left.Elevation.GetValueOrDefault(0) * right);
        }


        public override string ToString()
        {
            return
                (Id.HasValue ? $"Id: {Id.Value}, " : "")
                + $"Lat/Lon: {Latitude} / {Longitude} "
                + (Elevation.HasValue ? $", Elevation: {Elevation.Value:F2}" : "")
                + ((DistanceFromOriginMeters ?? 0) > 0 ? $", DistanceFromOrigin: {DistanceFromOriginMeters:F2}" : "");
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
