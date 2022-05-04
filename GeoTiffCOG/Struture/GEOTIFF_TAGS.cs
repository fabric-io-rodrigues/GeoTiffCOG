using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTiffCOG.Struture
{
    enum GEOTIFF_TAGS
    {
        /// <summary>
        /// This tag is defining exact affine transformations between raster and model space. Used in interchangeable GeoTIFF files.
        /// </summary>
        GEOTIFF_MODELPIXELSCALETAG = 33550,

        /// <summary>
        /// This tag stores raster->model tiepoint pairs. Used in interchangeable GeoTIFF files.
        /// </summary>
        GEOTIFF_MODELTIEPOINTTAG = 33922,

        /// <summary>
        /// This tag is optionally provided for defining exact affine transformations between raster and model space. Used in interchangeable GeoTIFF files.
        /// </summary>
        GEOTIFF_MODELTRANSFORMATIONTAG = 34264,

        /// <summary>
        /// This tag may be used to store the GeoKey Directory, which defines and references the "GeoKeys". Used in interchangeable GeoTIFF files.
        /// </summary>
        GEOTIFF_GEOKEYDIRECTORYTAG = 34735,

        /// <summary>
        /// This tag is used to store all of the DOUBLE valued GeoKeys, referenced by the GeoKeyDirectoryTag. Used in interchangeable GeoTIFF files.
        /// </summary>
        GEOTIFF_GEODOUBLEPARAMSTAG = 34736,

        /// <summary>
        /// This tag is used to store all of the ASCII valued GeoKeys, referenced by the GeoKeyDirectoryTag. Used in interchangeable GeoTIFF files.
        /// </summary>
        GEOTIFF_GEOASCIIPARAMSTAG = 34737,

        // TIFF Tag GDAL_METADATA 42112
        GEOTIFF_GDAL_METADATA = 42112,

        // TIFF Tag GDAL_NODATA 42113
        GEOTIFF_GDAL_NODATA = 42113
    }
}
