# GeoTiffCOG
Open Cloud Optimized GeoTiffs in C# Console Library

After researching some solutions, mostly in Python or using GDAL (QGis/ArcGis) I worked to build this first working version.

The idea is to access GeoTiff (local or COG) and return the corresponding value given a latitude/longitude.


```c#
      GeoTiff geoTiff = new GeoTiff(new Uri(url), directoryTemp);

      double value = geoTiff.GetElevationAtLatLon(latitude, longitude);
```

As an example, url_COG + expected result. This same Tiff is in the Sample directory, it can be accessed locally.

```
 https://globalwindatlas.info/api/gis/country/SLV/wind-speed/50 
 
 Latitude::  13.88427 
 Longitude:  -89.42231
 Value:    3.6284714
 
 Latitude:   13.59475;
 Longitude: -88.84668;
 Value:    8.6823205947876
```
![Map of El Salvador - Wind Data 50m](https://raw.githubusercontent.com/fabric-io-rodrigues/GeoTiffCOG/master/SampleData/SLV-Wind50m.png)


Some project details: I used some best practices from the DEM.Net lib and as a base (used as reference) the BitMiracle/libtiff.net lib.

Feel free to download, modify and suggest changes (such as pull request).


## License

Copyright © 2021 [Fabricio Rodrigues](https://github.com/fabric-io-rodrigues)
Released under the MIT license.

***