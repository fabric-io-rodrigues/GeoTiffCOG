using BitMiracle.LibTiff.Classic;
using GeoTiffCOG.Struture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoTiffCOG
{
    public class RasterServices
    {
        private static void TagExtender(Tiff tiff)
        {
            TiffFieldInfo[] tiffFieldInfo =
            {
                new TiffFieldInfo(TiffTag.GEOTIFF_MODELTIEPOINTTAG, 6, 6, TiffType.DOUBLE, FieldBit.Custom, false, true, "MODELTILEPOINTTAG"),
                new TiffFieldInfo(TiffTag.GEOTIFF_MODELPIXELSCALETAG, 3, 3, TiffType.DOUBLE, FieldBit.Custom, false, true, "MODELPIXELSCALETAG")
            };

            tiff.MergeFieldInfo(tiffFieldInfo, tiffFieldInfo.Length);

        }

        public static void SaveRaster(MetaDataRaster metaDataRaster, string fileOutput, RasterOutputType outputType)
        {
            switch (outputType)
            {
                case RasterOutputType.XYZ:
                    SaveRasterXYZ(metaDataRaster, fileOutput);
                    break;
                case RasterOutputType.XYZGZIP:
                    SaveRasterXYZGZip(metaDataRaster, fileOutput);
                    break;
                case RasterOutputType.GEOTIFF:
                    SaveRasterGeoTiff(metaDataRaster, fileOutput);
                    break;
                default:
                    break;
            }
        }

        private static void SaveRasterXYZ(MetaDataRaster metaDataRaster, string fileOutput)
        {
            using (FileStream outFile = File.Create(fileOutput))
            using (StreamWriter writer = new StreamWriter(outFile))
            {
                foreach (var item in metaDataRaster.Coordinates)
                    writer.WriteLine(string.Format("{0},{1},{2}", item.Longitude, item.Latitude, item.Elevation));
            }
        }

        private static void SaveRasterXYZGZip(MetaDataRaster metaDataRaster, string fileOutput)
        {
            using (FileStream outFileGz = File.Create(fileOutput))
            using (System.IO.Compression.GZipStream compress = new System.IO.Compression.GZipStream(outFileGz, System.IO.Compression.CompressionMode.Compress))
            using (StreamWriter writer = new StreamWriter(compress))
            {
                foreach (var item in metaDataRaster.Coordinates)
                    writer.WriteLine(string.Format("{0},{1},{2}", item.Longitude, item.Latitude, item.Elevation));
            }
        }

        private static void SaveRasterGeoTiff(MetaDataRaster metaDataRaster, string fileOutput)
        {
            if (metaDataRaster.Buffer == null)
                PopulateRasterBuffer(metaDataRaster);

            Tiff.SetTagExtender(TagExtender);
            using (Tiff output = Tiff.Open(fileOutput, "w"))
            {
                output.SetField(TiffTag.IMAGEWIDTH, metaDataRaster.Width);
                output.SetField(TiffTag.IMAGELENGTH, metaDataRaster.Height);
                output.SetField(TiffTag.SAMPLESPERPIXEL, metaDataRaster.SamplesPerPixel);
                output.SetField(TiffTag.BITSPERSAMPLE, metaDataRaster.BitsPerSample);
                output.SetField(TiffTag.ORIENTATION, Orientation.RIGHTBOT);

                output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));
                output.SetField(TiffTag.XRESOLUTION, metaDataRaster.Height);
                output.SetField(TiffTag.YRESOLUTION, metaDataRaster.Width);

                output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                output.SetField(TiffTag.SAMPLEFORMAT, SampleFormat.IEEEFP);

                double[] tiePoints = new double[] { 0, 0, 0, metaDataRaster.TiePointLon, metaDataRaster.TiePointLat, 0 };
                double[] pixelScale = new double[] { metaDataRaster.PixelScaleX, metaDataRaster.PixelScaleY, 0 };

                output.SetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG, 6, (object)tiePoints);
                output.SetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG, 3, (object)pixelScale);

                for (int i = 0; i < metaDataRaster.Height; ++i)
                    output.WriteScanline(metaDataRaster.Buffer[i], i);
            }
        }

        public static MetaDataRaster GetMetaDataRaster(HeightMap heightMap, GeoTiff geoTiffMain)
        {
            MetaDataRaster metaDataRaster = new MetaDataRaster
            {
                Width = heightMap.Width,
                Height = heightMap.Height,
                SamplesPerPixel = 1,
                BitsPerSample = geoTiffMain.metadata.BitsPerSample,
                TiePointLon = heightMap.BoundingBox.xMin - (geoTiffMain.metadata.PixelScaleX / 2),
                TiePointLat = heightMap.BoundingBox.yMax + (geoTiffMain.metadata.PixelScaleY / 2),
                PixelScaleX = geoTiffMain.metadata.PixelScaleX,
                PixelScaleY = geoTiffMain.metadata.PixelScaleY,
                Coordinates = heightMap.Coordinates
            };

            return metaDataRaster;
        }

        public static MetaDataRaster GetMetaDataRaster(IEnumerable<GeoPoint> coordinates)
        {
            List<double> listLat = coordinates.OrderByDescending(c => c.Latitude).Select(c => c.Latitude).Distinct().ToList();
            List<double> listLon = coordinates.OrderByDescending(c => c.Longitude).Select(c => c.Longitude).Distinct().ToList();
            double _minLat = listLat.Min();
            double _maxLat = listLat.Max();
            double _minLon = listLon.Min();
            double _maxLon = listLon.Max();
            int _height = listLat.Count(); //countLat
            int _width = listLon.Count(); //countLon
            double _pixelScaleY = listLat[0] - listLat[1]; //diffLat
            double _pixelScaleX = listLon[0] - listLon[1]; //diffLon
            double _tiePointLon = _minLon - (_pixelScaleX / 2);
            double _tiePointLat = _maxLat + (_pixelScaleY / 2);

            MetaDataRaster metaDataRaster = new MetaDataRaster
            {
                Width = _width,
                Height = _height,
                SamplesPerPixel = 1,
                BitsPerSample = 32,
                TiePointLon = _tiePointLon,
                TiePointLat = _tiePointLat,
                PixelScaleX = _pixelScaleX,
                PixelScaleY = _pixelScaleY,
                Coordinates = coordinates
            };

            return metaDataRaster;
        }

        private static void PopulateRasterBuffer(MetaDataRaster metaDataRaster)
        {
            byte[][] buffer;
            Single[] elevations = metaDataRaster.Coordinates.OrderByDescending(c => c.Latitude).ThenBy(c => c.Longitude).Select(c => Convert.ToSingle(c.Elevation)).ToArray();

            int iC = 0;
            int scanlineSize = metaDataRaster.Width * 4;
            buffer = new byte[metaDataRaster.Height][];
            for (int iH = 0; iH < metaDataRaster.Height; iH++)
            {
                buffer[iH] = new byte[scanlineSize];
                for (int iW = 0; iW < metaDataRaster.Width; iW++)
                {
                    Single valueElevation = elevations[iC++];
                    Buffer.BlockCopy(BitConverter.GetBytes(valueElevation), 0, buffer[iH], iW * 4, 4);
                }
            }
            metaDataRaster.Buffer = buffer;
        }
    }
}
