using System;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace TakeTrainingPhotos
{
    //Major points from: https://docs.microsoft.com/en-us/azure/cognitive-services/custom-vision-service/getting-started-improving-your-classifier

    //Provide images with different angles, backgrounds, object size, groups, and other variations.
    //Maintain at least a 1:2 ratio between the label with the fewest images and the label with the most images
    //Provide images of your object in front of different backgrounds
    //Provide images with varied lighting
    //Provide images in which the objects vary in size
    //Provide images taken with different camera angles.
    //Hover over an image to see the tags that were predicted by the model.
    //Images are sorted so that the ones which can bring the most improvements to the model are listed the top.
    //Go to the Training Images tab, select your previous training iteration in the Iteration drop-down menu, and check one or more tags under the Tags section.
    //The view should now display a red box around each of the images for which the model failed to correctly predict the given tag.
    //Sometimes a visual inspection can identify patterns that you can then correct by adding more training data or modifying existing training data

    class Program
    {
        static void Main()
        {
            using (var jpeg = new JpegCompression())
            using (var capture = new VideoCapture(0))
            {
                capture.Open(0);
                if (!capture.IsOpened()) return;
                while (true)
                {
                    var frame = new Mat();
                    capture.Read(frame);
                    using (var image = frame.ToBitmap())
                        jpeg.Save(image, $@"C:\$WIP\Gestures\Bird\New\{DateTime.Now.Ticks}.jpg");
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
    }
}