using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace TakeTrainingPhotos
{
    internal class JpegCompression : IDisposable
    {
        private readonly ImageCodecInfo _jpegEncoder;
        private readonly EncoderParameters _encoderParameters;
        private readonly EncoderParameter _encoderParameter;

        public JpegCompression(int compression = 100)
        {
            if (compression < 1 || compression > 100)
            {
                throw new ArgumentException("Invalid compression level");
            }
            _jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            _encoderParameter = new EncoderParameter(Encoder.Quality, compression);
            _encoderParameters = new EncoderParameters(1);
            _encoderParameters.Param[0] = _encoderParameter;
        }

        public void Save(Image image, string path)
        {
            image.Save(path, _jpegEncoder, _encoderParameters);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        public void Dispose()
        {
            _encoderParameter?.Dispose();
            _encoderParameters?.Dispose();
        }
    }
}
