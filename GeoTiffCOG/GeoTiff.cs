using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using BitMiracle.LibTiff.Classic;
using GeoTiffCOG.Struture;
using System.Xml;

namespace GeoTiffCOG
{
    public class GeoTiff
    {
        BitMiracle.LibTiff.Classic.Tiff _tiff;
        string _tiffSource;
        TraceTiffErrorHandler _traceLogHandler = new TraceTiffErrorHandler();
        Dictionary<int, byte[]> tilesCache;
        public Metadata metadata { get; private set; }

        internal Tiff TiffFile
        {
            get { return _tiff; }
        }

        public string Source
        {
            get { return _tiffSource; }
        }
        public GeoTiff(Uri urlCOG, string directoryCache)
        {
            Tiff.SetErrorHandler(_traceLogHandler);
            _tiff = Tiff.ClientOpen("tiff", "r", null, new TiffSteamCustom(urlCOG.ToString(), directoryCache));
            _tiffSource = urlCOG.ToString();

            metadata = this.ParseMetaData(new DEMFileDefinition());
        }

        public GeoTiff(Uri urlCOG, string directoryCache, System.Net.IWebProxy webProxy, System.Net.ICredentials credentials)
        {
            Tiff.SetErrorHandler(_traceLogHandler);
            _tiff = Tiff.ClientOpen("tiff", "r", null, new TiffSteamCustom(urlCOG.ToString(), directoryCache, webProxy, credentials));
            _tiffSource = urlCOG.ToString();

            metadata = this.ParseMetaData(new DEMFileDefinition());
        }

        public GeoTiff(string tiffPath)
        {
            Tiff.SetErrorHandler(_traceLogHandler);
            if (!File.Exists(tiffPath))
                throw new Exception($"File {tiffPath} does not exists !");

            _tiffSource = tiffPath;
            
            _tiff = Tiff.Open(tiffPath, "r");

            if (_tiff == null)
                throw new Exception($"File {tiffPath} cannot be opened !");

            metadata = this.ParseMetaData(new DEMFileDefinition());
        }

        #region Tile info

        int tileWidth = 0;

        internal int TileWidth
        {
            get
            {
                if (tileWidth == 0)
                    tileWidth = TiffFile.GetField(TiffTag.TILEWIDTH)[0].ToInt();
 
                return tileWidth;
            }
        }
        int tileHeight = 0;

        internal int TileHeight
        {
            get
            {
                if (tileHeight == 0)
                    tileHeight = TiffFile.GetField(TiffTag.TILELENGTH)[0].ToInt();

                return tileHeight;
            }
        }
        int tileSize = 0;

        internal int TileSize
        {
            get
            {
                if (tileSize == 0)
                    tileSize = TiffFile.TileSize();

                return tileSize;
            }
        }

        bool isTiledSet = false;
        bool isTiled;
        internal bool IsTiled
        {
            get
            {
                if (isTiledSet == false)
                {
                    isTiled = TiffFile.IsTiled();
                    isTiledSet = true;
                }

                return isTiled;
            }
        }
        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tiff?.Dispose();
                if (tilesCache != null)
                {
                    tilesCache.Clear();
                    tilesCache = null;
                }
            }
        }

        ~GeoTiff()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public float GetElevationAtLatLon(double latitude, double longitude)
        {
            Utils.GetXYFromLatLon(metadata, latitude, longitude, out int x, out int y);

            if (x < 0) throw new Exception($"Error longitude: {longitude} not valid to [{metadata.PhysicalStartLon}, {metadata.PhysicalEndLon}] offsetX: {x}");
            if (y < 0) throw new Exception($"Error latitude: {latitude} not valid to [{metadata.PhysicalStartLat}, {metadata.PhysicalEndLat}] offsetY: {y}");

            return GetElevationAtPoint(x, y);
        }

        public float GetElevationAtPoint(int x, int y)
        {
            float heightValue = 0;
            try
            {
                if (this.IsTiled)
                {
                    int tileWidth = this.TileWidth;
                    int tileHeight = this.TileHeight;
                    int tileSize = this.TileSize;
                    byte[] buffer;

                    var tileX = (x / tileWidth) * tileWidth;
                    var tileY = (y / tileHeight) * tileHeight;

                    if (tilesCache == null) tilesCache = new Dictionary<int, byte[]>();
                    var tileKey = (x / tileWidth) + (y / tileHeight) * (metadata.Width / tileWidth + 1);
                    if (!tilesCache.TryGetValue(tileKey, out buffer))
                    {
                        buffer = new byte[tileSize];
                        TiffFile.ReadTile(buffer, 0, tileX, tileY, 0, 0);
                        tilesCache.Add(tileKey, buffer);
                    }
                    var offset = x - tileX + (y - tileY) * tileHeight;
                    heightValue = GetElevationAtPoint(offset, buffer);
                }
                else
                {
                    int bytesPerSample = metadata.BitsPerSample / 8;
                    byte[] byteScanline = new byte[metadata.ScanlineSize];

                    //it is necessary to iterate until you reach the desired line
                    int _minHeigh = Math.Min(y, metadata.Height);
                    for (int iLine = 0; iLine < _minHeigh; ++iLine)
                        TiffFile.ReadScanline(byteScanline, iLine);

                    TiffFile.ReadScanline(byteScanline, y);

                    heightValue = GetElevationAtPoint(x, byteScanline);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error in ParseGeoDataAtPoint: {e.Message}");
            }
            return heightValue;
        }

        protected float GetElevationAtPoint(int offset, byte[] buffer)
        {
            float heightValue = 0;
            try
            {
                switch (metadata.SampleFormat)
                {
                    case RasterSampleFormat.FLOATING_POINT:
                        heightValue = BitConverter.ToSingle(buffer, offset * metadata.BitsPerSample / 8);
                        break;
                    case RasterSampleFormat.INTEGER:
                        if (metadata.BitsPerSample == 32)
                            heightValue = BitConverter.ToInt32(buffer, offset * metadata.BitsPerSample / 8);
                        else
                            heightValue = BitConverter.ToInt16(buffer, offset * metadata.BitsPerSample / 8);
                        heightValue = heightValue * metadata.Scale + metadata.Offset;
                        break;
                    case RasterSampleFormat.UNSIGNED_INTEGER:
                        if (metadata.BitsPerSample == 32)
                            heightValue = BitConverter.ToUInt32(buffer, offset * metadata.BitsPerSample / 8);
                        else if (metadata.BitsPerSample == 16)
                            heightValue = BitConverter.ToUInt16(buffer, offset * metadata.BitsPerSample / 8);
                        else if (metadata.BitsPerSample == 8)
                            heightValue = buffer[offset];
                        heightValue = heightValue * metadata.Scale + metadata.Offset;
                        break;
                    default:
                        throw new Exception("Sample format unsupported.");
                }
                if ((heightValue > 32768.0f) || (heightValue <= -32768.0f))
                {
                    heightValue = metadata.NoDataValueFloat;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error in ParseGeoDataAtPoint: {e.Message}");
            }

            return heightValue;
        }
 
        public class GDALMetaData
        {
            public float Offset { get; set; }
            public float Scale { get; set; }

        }

        protected GDALMetaData ParseXml(string xml)
        {
            GDALMetaData gdal = new GDALMetaData()
            {
                Offset = 0,
                Scale = 1
            };

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNodeList nodeList = doc.GetElementsByTagName("GDALMetadata");

            foreach (XmlNode node in nodeList)
            {

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    string item = childNode.InnerText;

                    if (childNode.Attributes?["name"]?.Value == "OFFSET")
                    {
                        gdal.Offset = Convert.ToSingle(item);
                    }
                    if (childNode.Attributes?["name"]?.Value == "SCALE")
                    {
                        gdal.Scale = Convert.ToSingle(item);
                    }
                }
            }

            return gdal;
        }

        protected Metadata ParseMetaData(DEMFileDefinition format)
        {
            Metadata _metadata = new Metadata(Source, format);

            _metadata.Height = TiffFile.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            _metadata.Width = TiffFile.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();

            FieldValue[] modelPixelScaleTag = TiffFile.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
            FieldValue[] modelTiepointTag = TiffFile.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);

            byte[] modelPixelScale = modelPixelScaleTag[1].GetBytes();
            double pixelSizeX = BitConverter.ToDouble(modelPixelScale, 0);
            double pixelSizeY = BitConverter.ToDouble(modelPixelScale, 8) * -1;
            _metadata.pixelSizeX = pixelSizeX;
            _metadata.pixelSizeY = pixelSizeY;
            _metadata.PixelScaleX = BitConverter.ToDouble(modelPixelScale, 0);
            _metadata.PixelScaleY = BitConverter.ToDouble(modelPixelScale, 8);

            byte[] modelTransformation = modelTiepointTag[1].GetBytes();
            _metadata.DataStartLon = BitConverter.ToDouble(modelTransformation, 24);
            _metadata.DataStartLat = BitConverter.ToDouble(modelTransformation, 32);
            _metadata.DataEndLon = _metadata.DataStartLon + _metadata.Width * pixelSizeX;
            _metadata.DataEndLat = _metadata.DataStartLat + _metadata.Height * pixelSizeY;

            if (_metadata.DataStartLon > _metadata.DataEndLon)
            {
                double temp = _metadata.DataStartLon;
                _metadata.DataStartLon = _metadata.DataEndLon;
                _metadata.DataEndLon = temp;
            }
            if (_metadata.DataStartLat > _metadata.DataEndLat)
            {
                double temp = _metadata.DataStartLat;
                _metadata.DataStartLat = _metadata.DataEndLat;
                _metadata.DataEndLat = temp;
            }

            if (format.Registration == DEMFileRegistrationMode.Grid)
            {
                _metadata.PhysicalStartLat = _metadata.DataStartLat;
                _metadata.PhysicalStartLon = _metadata.DataStartLon;
                _metadata.PhysicalEndLat = _metadata.DataEndLat;
                _metadata.PhysicalEndLon = _metadata.DataEndLon;
                _metadata.DataStartLat = Math.Round(_metadata.DataStartLat + (_metadata.PixelScaleY / 2.0), 10);
                _metadata.DataStartLon = Math.Round(_metadata.DataStartLon + (_metadata.PixelScaleX / 2.0), 10);
                _metadata.DataEndLat = Math.Round(_metadata.DataEndLat - (_metadata.PixelScaleY / 2.0), 10);
                _metadata.DataEndLon = Math.Round(_metadata.DataEndLon - (_metadata.PixelScaleX / 2.0), 10);
            }
            else
            {
                _metadata.PhysicalStartLat = _metadata.DataStartLat;
                _metadata.PhysicalStartLon = _metadata.DataStartLon;
                _metadata.PhysicalEndLat = _metadata.DataEndLat;
                _metadata.PhysicalEndLon = _metadata.DataEndLon;
            }
            var scanline = new byte[TiffFile.ScanlineSize()];
            _metadata.ScanlineSize = TiffFile.ScanlineSize();

            _metadata.BitsPerSample = TiffFile.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt();
            var sampleFormat = TiffFile.GetField(TiffTag.SAMPLEFORMAT);
            _metadata.SampleFormat = sampleFormat[0].Value.ToString();

            LoadGEOTiffTags(_metadata);

            return _metadata;
        }

        protected void LoadGEOTiffTags(Metadata _metadata)
        {
            //defaults
            _metadata.Properties.CoordinateSystemId = 4326; // WGS 84
            _metadata.Properties.ModelType = ModelType.Geographic;
            _metadata.Properties.RasterType = RasterType.RasterPixelIsArea;
            _metadata.Properties.AngularUnit = AngularUnits.Degree;

            var geoKeys = TiffFile.GetField((TiffTag)GEOTIFF_TAGS.GEOTIFF_GEOKEYDIRECTORYTAG);
            if (geoKeys != null)
            {
                var geoDoubleParams = TiffFile.GetField((TiffTag)GEOTIFF_TAGS.GEOTIFF_GEODOUBLEPARAMSTAG);
                double[] doubleParams = null;
                if (geoDoubleParams != null)
                {
                    doubleParams = geoDoubleParams[1].ToDoubleArray();
                }
                var geoAsciiParams = TiffFile.GetField((TiffTag)GEOTIFF_TAGS.GEOTIFF_GEOASCIIPARAMSTAG);
                if (geoAsciiParams != null) _metadata.Properties.GeotiffAsciiParams = geoAsciiParams[1].ToString().Trim('\0');

                // Array of GeoTIFF GeoKeys values
                var keys = geoKeys[1].ToUShortArray();
                if (keys.Length > 4)
                {
                    // Header={KeyDirectoryVersion, KeyRevision, MinorRevision, NumberOfKeys}
                    var keyDirectoryVersion = keys[0];
                    var keyRevision = keys[1];
                    var minorRevision = keys[2];
                    var numberOfKeys = keys[3];
                    for (var keyIndex = 4; keyIndex < keys.Length;)
                    {
                        switch (keys[keyIndex])
                        {
                            case (ushort)GeoTiffKey.GTModelTypeGeoKey:
                                {
                                    _metadata.Properties.ModelType = (ModelType)keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GTRasterTypeGeoKey:
                                {
                                    _metadata.Properties.RasterType = (RasterType)keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GTCitationGeoKey:
                                {
                                    _metadata.Properties.GTCitationGeo = keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeographicTypeGeoKey:
                                {
                                    _metadata.Properties.CoordinateSystemId = keys[keyIndex + 3]; //geographicType
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogCitationGeoKey:
                                {
                                    _metadata.Properties.GeogCitation = keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogGeodeticDatumGeoKey:
                                {
                                    // 6.3.2.2 Geodetic Datum Codes
                                    _metadata.Properties.GeodeticDatum = keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogPrimeMeridianGeoKey:
                                {
                                    // 6.3.2.4 Prime Meridian Codes
                                    _metadata.Properties.PrimeMeridian = keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogAngularUnitsGeoKey:
                                {
                                    _metadata.Properties.AngularUnit = (AngularUnits)keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogAngularUnitSizeGeoKey:
                                {
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogEllipsoidGeoKey:
                                {
                                    // 6.3.2.3 Ellipsoid Codes
                                    _metadata.Properties.Ellipsoid = keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogSemiMajorAxisGeoKey:
                                {
                                    if (doubleParams != null)
                                        _metadata.Properties.SemiMajorAxis = doubleParams[keys[keyIndex + 3]];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogSemiMinorAxisGeoKey:
                                {
                                    if (doubleParams != null)
                                        _metadata.Properties.SemiMinorAxis = doubleParams[keys[keyIndex + 3]];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogInvFlatteningGeoKey:
                                {
                                    if (doubleParams != null)
                                        _metadata.Properties.InvFlattening = doubleParams[keys[keyIndex + 3]];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogAzimuthUnitsGeoKey:
                                {
                                    _metadata.Properties.AngularUnit = (AngularUnits)keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.GeogPrimeMeridianLongGeoKey:
                                {
                                    _metadata.Properties.PrimeMeridianLong = keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.ProjectedCSTypeGeoKey:
                                {
                                    _metadata.Properties.CoordinateSystemId = keys[keyIndex + 3]; //projectedCSType
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.PCSCitationGeoKey:
                                {
                                    keyIndex += 4;
                                    break;
                                }
                            case (ushort)GeoTiffKey.ProjLinearUnitsGeoKey:
                                {
                                    _metadata.Properties.LinearUnit = (LinearUnits)keys[keyIndex + 3];
                                    keyIndex += 4;
                                    break;
                                }
                            default:
                                {
                                    // Just skipping all unprocessed keys
                                    keyIndex += 4;
                                    break;
                                }
                        }
                    }

                }

            }

            // TIFF Tag GDAL_METADATA 42112
            _metadata.Scale = 1;
            var tag42112 = TiffFile.GetField((TiffTag)GEOTIFF_TAGS.GEOTIFF_GDAL_METADATA);
            if (tag42112 != null && tag42112.Length >= 1)
            {
                _metadata.Properties.GdalMetadata = tag42112[1].ToString().Trim('\0');
                var gd = ParseXml(_metadata.Properties.GdalMetadata);
                _metadata.Offset = gd.Offset;
                _metadata.Scale = gd.Scale;
            }

            // TIFF Tag GDAL_NODATA 42113
            _metadata.NoDataValue = "-10000";
            var tag42113 = TiffFile.GetField((TiffTag)GEOTIFF_TAGS.GEOTIFF_GDAL_NODATA);
            if (tag42113 != null && tag42113.Length >= 1)
            {
                _metadata.NoDataValue = tag42113[1].ToString().Trim('\0');
            }
        }

        public HeightMap GetHeightMapInBBox(BoundingBox bbox, float noDataValue = 0)
        {
            double rasterEndLat = metadata.DataEndLat;
            double rasterStartLon = metadata.PhysicalStartLon;

            int yStart = (int)Math.Floor((bbox.yMax - rasterEndLat) / metadata.pixelSizeY);
            int yEnd = (int)Math.Ceiling((bbox.yMin - rasterEndLat) / metadata.pixelSizeY);
            int xStart = (int)Math.Floor((bbox.xMin - rasterStartLon) / metadata.pixelSizeX);
            int xEnd = (int)Math.Ceiling((bbox.xMax - rasterStartLon) / metadata.pixelSizeX);

            // Tiled geotiffs like raster have overlapping 1px borders
            int overlappingPixel = this.IsTiled ? 1 : 0;

            xStart = Math.Max(0, xStart);
            xEnd = Math.Min(metadata.Width - 1, xEnd) - overlappingPixel;
            yStart = Math.Max(0, yStart);
            yEnd = Math.Min(metadata.Height - 1, yEnd) - overlappingPixel;

            HeightMap heightMap = new HeightMap(xEnd - xStart + 1, yEnd - yStart + 1);
            heightMap.Count = heightMap.Width * heightMap.Height;
            var coords = new List<GeoPoint>(heightMap.Count);
            heightMap.BoundingBox = new BoundingBox(0, 0, 0, 0);

            if (this.IsTiled)
            {
                int tileWidth = this.TileWidth;
                int tileHeight = this.TileHeight;
                int tileSize = this.TileSize;
                byte[] buffer;

                for (int y = yStart; y <= yEnd; y++)
                {
                    double latitude = rasterEndLat + (metadata.pixelSizeY * y);

                    if (y == yStart)
                    {
                        heightMap.BoundingBox.yMax = latitude;
                        heightMap.BoundingBox.xMin = rasterStartLon + (metadata.pixelSizeX * xStart);
                        heightMap.BoundingBox.xMax = rasterStartLon + (metadata.pixelSizeX * xEnd);
                    }
                    if (y == yEnd)
                    {
                        heightMap.BoundingBox.yMin = latitude;
                    }

                    for (int x = xStart; x <= xEnd; x++)
                    {
                        double longitude = rasterStartLon + (metadata.pixelSizeX * x);
                        var tileX = (x / tileWidth) * tileWidth;
                        var tileY = (y / tileHeight) * tileHeight;

                        if (tilesCache == null) tilesCache = new Dictionary<int, byte[]>();
                        var tileKey = (x / tileWidth) + (y / tileHeight) * (metadata.Width / tileWidth + 1);
                        if (!tilesCache.TryGetValue(tileKey, out buffer))
                        {
                            buffer = new byte[tileSize];
                            TiffFile.ReadTile(buffer, 0, tileX, tileY, 0, 0);
                            tilesCache.Add(tileKey, buffer);
                        }
                        var offset = x - tileX + (y - tileY) * tileHeight;
                        float heightValue = GetElevationAtPoint(offset, buffer);
                        if (heightValue <= 0)
                        {
                            heightMap.Minimum = Math.Min(heightMap.Minimum, heightValue);
                            heightMap.Maximum = Math.Max(heightMap.Maximum, heightValue);
                        }
                        else if (heightValue < 32768)
                        {
                            heightMap.Minimum = Math.Min(heightMap.Minimum, heightValue);
                            heightMap.Maximum = Math.Max(heightMap.Maximum, heightValue);
                        }

                        else
                        {
                            heightValue = (float)noDataValue;
                        }
                        coords.Add(new GeoPoint(latitude, longitude, heightValue));

                    }
                }
            }
            else
            {
                int bytesPerSample = metadata.BitsPerSample / 8;
                byte[] byteScanline = new byte[metadata.ScanlineSize];
                double endLat = rasterEndLat + metadata.pixelSizeY / 2d;
                double startLon = rasterStartLon + metadata.pixelSizeX / 2d;

                for (int y = yStart; y <= yEnd; y++)
                {

                    TiffFile.ReadScanline(byteScanline, y);
                    double latitude = endLat + (metadata.pixelSizeY * y);

                    if (y == yStart)
                    {
                        heightMap.BoundingBox.yMax = latitude;
                        heightMap.BoundingBox.xMin = startLon + (metadata.pixelSizeX * xStart);
                        heightMap.BoundingBox.xMax = startLon + (metadata.pixelSizeX * xEnd);
                    }
                    else if (y == yEnd)
                    {
                        heightMap.BoundingBox.yMin = latitude;
                    }

                    for (int x = xStart; x <= xEnd; x++)
                    {
                        double longitude = startLon + (metadata.pixelSizeX * x);

                        float heightValue = 0;
                        switch (metadata.SampleFormat)
                        {
                            case RasterSampleFormat.FLOATING_POINT:
                                heightValue = BitConverter.ToSingle(byteScanline, x * bytesPerSample);
                                break;
                            case RasterSampleFormat.INTEGER:
                                if (metadata.BitsPerSample == 32)
                                    heightValue = BitConverter.ToInt32(byteScanline, x * bytesPerSample);
                                else
                                    heightValue = BitConverter.ToInt16(byteScanline, x * bytesPerSample);
                                break;
                            case RasterSampleFormat.UNSIGNED_INTEGER:
                                if (metadata.BitsPerSample == 32)
                                    heightValue = BitConverter.ToUInt32(byteScanline, x * bytesPerSample);
                                else if (metadata.BitsPerSample == 16)
                                    heightValue = BitConverter.ToUInt16(byteScanline, x * bytesPerSample);
                                else if (metadata.BitsPerSample == 8)
                                    heightValue = byteScanline[x * bytesPerSample];
                                break;
                            default:
                                throw new Exception("Sample format unsupported.");
                        }
                        if (heightValue <= 0)
                        {
                            heightMap.Minimum = Math.Min(heightMap.Minimum, heightValue);
                            heightMap.Maximum = Math.Max(heightMap.Maximum, heightValue);
                        }
                        else if (heightValue < 32768)
                        {
                            heightMap.Minimum = Math.Min(heightMap.Minimum, heightValue);
                            heightMap.Maximum = Math.Max(heightMap.Maximum, heightValue);
                        }
                        else
                        {
                            heightValue = (float)noDataValue;
                        }
                        coords.Add(new GeoPoint(latitude, longitude, heightValue));
                    }
                }

            }
            heightMap.BoundingBox.zMin = heightMap.Minimum;
            heightMap.BoundingBox.zMax = heightMap.Maximum;

            heightMap.Coordinates = coords;
            return heightMap;
        }

    }
}
