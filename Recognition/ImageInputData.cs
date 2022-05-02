using System.Drawing;
using Microsoft.ML.Transforms.Image;

namespace RecognitionEngine
{
    internal class ImageInputData
    {
        [ImageType(ImageSettings.ImageHeight, ImageSettings.ImageWidth)]
        public Bitmap Image { get; set; }
    }
}
