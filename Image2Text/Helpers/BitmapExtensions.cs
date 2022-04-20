using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Image2Text.Helpers
{
    public static class BitmapExtensions
    {
        public static void SaveJPG100(this Bitmap bitmap, string fileName)
        {
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
            bitmap.Save(fileName, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        public static void SaveJPG100(this Bitmap bitmap, Stream stream)
        {
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
            bitmap.Save(stream, GetEncoder(ImageFormat.Jpeg), encoderParameters);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();

            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }

            return null;
        }
    }
}
