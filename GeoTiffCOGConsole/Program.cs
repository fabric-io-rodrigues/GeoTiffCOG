using GeoTiffCOG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTiffCOGConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            string directoryApp = System.IO.Path.GetDirectoryName(asm.Location);
            Console.WriteLine("GeoTiff COG - version: 0.1");
            Console.WriteLine("--------------------------");

            if (args.Length >= 3)
            {
                string url = args[0];
                double lat = Convert.ToDouble(args[1]);
                double lon = Convert.ToDouble(args[2]);

                string directoryTemp = Path.Combine(directoryApp, "CacheChk");
                GeoTiff geoTiff = new GeoTiff(new Uri(url), directoryTemp);

                double value = geoTiff.GetElevationAtLatLon(lat, lon);

                Console.WriteLine(" Latitude {0};\n Longitude{1};\n Value: {2}", lat, lon, value);
            }
            else
            {

                Console.WriteLine("Usage: {0} <url_geotiff_cog> <lat> <lon>", asm.GetName().Name);

            }
            Console.WriteLine();

        }

     /*
         * 
         * https://globalwindatlas.info/api/gis/country/SLV/wind-speed/50 
         *
         * Latitude::  13.88427 
         * Longitude:  -89.42231
         * Value:    3.6284714
         *
         * Latitude:   13.59475;
         * Longitude: -88.84668;
         * Value:    8.6823205947876
         * 
     */

    }
}
