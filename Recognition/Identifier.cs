using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RecognitionEngine
{
    public class Identifier : IDisposable
    {
        private readonly float _threshold;
        private readonly PredictionEngine<ImageInputData, ImageOutputData> _predictionEngine;

        public Identifier(string onnxPath, float threshold)
        {
            if (threshold <= 0 || threshold > 1)
            {
                throw new ArgumentException("Invalid threshold");
            }
            _threshold = threshold;

            var context = new MLContext();
            var data = context.Data.LoadFromEnumerable(new List<ImageInputData>());
            var pipeline = context.Transforms
                .ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: ImageSettings.InputColumnName,
                    imageWidth: ImageSettings.ImageWidth, imageHeight: ImageSettings.ImageHeight,
                    inputColumnName: nameof(ImageInputData.Image))
                .Append(context.Transforms.ExtractPixels(outputColumnName: ImageSettings.InputColumnName)
                .Append(context.Transforms.ApplyOnnxModel(modelFile: onnxPath, outputColumnName: ImageSettings.OutputColumnName, inputColumnName: ImageSettings.InputColumnName)));

            var model = pipeline.Fit(data);
            _predictionEngine = context.Model.CreatePredictionEngine<ImageInputData, ImageOutputData>(model);
        }

        public ImageType IdentifyImageType(string imagePath)
        {
            using (var image = Image.FromFile(imagePath))
            {
                return IdentifyImageType(image);
            }
        }

        public ImageType IdentifyImageType(Image image)
        {
            var input = new ImageInputData { Image = (Bitmap)image };
            var prediction = _predictionEngine.Predict(input);

            var parser = new OutputParser(ImageSettings.Labels);
            var boxes = parser.ParseOutputs(prediction.PredictedLabels, _threshold);

            if (boxes.Count > 0)
            {
                var maxConfidence = boxes.Max(b => b.Confidence);
                var topBoundingBox = boxes.First(b => b.Confidence.Equals(maxConfidence));
                return (ImageType)(int)Enum.Parse(typeof(ImageType), topBoundingBox.Label);
            }
            return ImageType.Unknown;
        }

        public void Dispose()
        {
            _predictionEngine?.Dispose();
        }
    }
}
//KEEP THIS!
//var pipeline = context.Transforms
//    .ResizeImages("image", ImageSettings.ImageWidth, ImageSettings.ImageHeight, nameof(ImageInputData.Image))
//    .Append(context.Transforms.ExtractPixels("data", "image"))
//    .Append(context.Transforms.ApplyOnnxModel(new[] { "classLabel", "loss" }, new[] { "data" },
//        onnxPath));
