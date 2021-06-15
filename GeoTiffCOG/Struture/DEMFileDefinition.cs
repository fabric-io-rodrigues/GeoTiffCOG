using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTiffCOG.Struture
{
    public class DEMFileDefinition
    {
        public DEMFileDefinition(string name, DEMFileType format, string extension, DEMFileRegistrationMode registration)
        {
            this.Name = name;
            this.Type = format;
            this.FileExtension = extension;
            this.Registration = registration;
        }
        public DEMFileDefinition(DEMFileType format, DEMFileRegistrationMode registration)
        {
            this.Name = null;
            this.Type = format;
            this.FileExtension = null;
            this.Registration = registration;
        }

        public DEMFileDefinition()
        {
        }
        /// <summary>
        /// Common name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Physical raster files extension (format: ".ext")
        /// </summary>
		public string FileExtension { get; set; }

        /// <summary>
        /// Physical file format enumeration
        /// </summary>
        public DEMFileType Type { get; set; }

        /// <summary>
        /// Grid/node-registered: cells are centered on lines of latitude and longitude (usually there is one pixel overlap for each tile).
        /// Cell/pixel-registered: cell edges are along lines of latitude and longitude.
        /// Good explanation here : https://www.ngdc.noaa.gov/mgg/global/gridregistration.html
        /// </summary>
        public DEMFileRegistrationMode Registration { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
