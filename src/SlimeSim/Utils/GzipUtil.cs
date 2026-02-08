using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimeSim.Utils
{
    public static class GzipUtil
    {
        public static byte[] Compress(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Array.Empty<byte>();

            byte[] inputBytes = Encoding.UTF8.GetBytes(text);

            using var output = new MemoryStream();

            using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
            {
                gzip.Write(inputBytes, 0, inputBytes.Length);
            }

            return output.ToArray();
        }

        public static string Decompress(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
                return string.Empty;

            using var input = new MemoryStream(compressedData);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();

            gzip.CopyTo(output);

            byte[] resultBytes = output.ToArray();

            return Encoding.UTF8.GetString(resultBytes);
        }
    }
}
