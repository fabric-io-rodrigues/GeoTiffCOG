using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeoTiffCOG.Struture;

namespace GeoTiffCOG
{
    static class Utils
    {
        public static BoundingBox GetBoundingBox(this IEnumerable<GeoPoint> points)
        {
            double xmin = double.MaxValue,
                ymin = double.MaxValue,
                zmin = double.MaxValue,
                xmax = double.MinValue,
                ymax = double.MinValue,
                zmax = double.MinValue;

            foreach (var pt in points)
            {
                xmin = Math.Min(xmin, pt.Longitude);
                xmax = Math.Max(xmax, pt.Longitude);

                ymin = Math.Min(ymin, pt.Latitude);
                ymax = Math.Max(ymax, pt.Latitude);

                zmin = Math.Min(zmin, pt.Elevation ?? double.MaxValue);
                zmax = Math.Max(zmax, pt.Elevation ?? double.MinValue);
            }
            return new BoundingBox(xmin, xmax, ymin, ymax, zmin, zmax);
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
