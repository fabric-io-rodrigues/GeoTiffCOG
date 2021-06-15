using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GeoTiffCOG
{
    internal class TiffSteamCustom: BitMiracle.LibTiff.Classic.TiffStream
    {

        long position;
        string urlCOGTiff;
        string directoryCache;
        public TiffSteamCustom(string urlCOG)
        {
            position = 0;
            urlCOGTiff = urlCOG;
            directoryCache = string.Empty;
        }
        public TiffSteamCustom(string urlCOG, string directoryCacheChk) : this(urlCOG)
        {
            directoryCache = Path.Combine(directoryCacheChk, Utils.CreateMD5(urlCOG.ToLower()));
            if (!Directory.Exists(directoryCache))
                Directory.CreateDirectory(directoryCache);
        }


        public override int Read(object clientData, byte[] buffer, int offset, int count)
        {
            Stream stream = GetStream(offset, count);

            return stream.Read(buffer, offset, count);
        }

        public override void Write(object clientData, byte[] buffer, int offset, int count)
        {
            throw new ArgumentException("Not suported.");
        }

        public override long Seek(object clientData, long offset, SeekOrigin origin)
        {
            // we use this as a special code, so avoid accepting it
            if (offset == -1)
                return -1; // was 0xFFFFFFFF

            if (origin == SeekOrigin.Begin)
            {
                position = offset;
            } 
            else
            {

            }
            return position;
        }

        public override void Close(object clientData)
        {

        }

        public override long Size(object clientData)
        {
            return position;
        }

        private Stream GetStream(int offset, int count)
        {
            MemoryStream ms = new MemoryStream();

            string fileCache = string.Format("{0}-{1}-{2}.chk", position, offset, count);
            
            //Restore cache
            if (!string.IsNullOrWhiteSpace(directoryCache) && File.Exists(Path.Combine(directoryCache, fileCache)))
            {
                using (FileStream file = new FileStream(Path.Combine(directoryCache, fileCache), FileMode.Open, FileAccess.Read))
                    file.CopyTo(ms);
                ms.Position = 0;
            }
            else
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(urlCOGTiff);
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webRequest.ServicePoint.ConnectionLimit = 500;
                webRequest.AddRange(position + offset, count - 1 + position);
                var bufferSize = (1024 * 8);
                var buffer = new byte[bufferSize];
                int length;
                using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    using (var webStream = webResponse.GetResponseStream())
                    {
                        while ((length = webStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            ms.Write(buffer, 0, length);
                        }
                    }
                }
                ms.Position = 0;

                //Save cache
                if (!string.IsNullOrWhiteSpace(directoryCache))
                {
                    using (FileStream file = new FileStream(Path.Combine(directoryCache, fileCache), FileMode.CreateNew, FileAccess.Write))
                        ms.CopyTo(file);
                    ms.Position = 0;
                }
            }

            position += offset + count;
            return ms;
        }

    }
}
