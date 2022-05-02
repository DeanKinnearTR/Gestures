using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RecognitionEngine
{
    //Based on: https://github.com/dotnet/machinelearning-samples/tree/main/samples/csharp/getting-started/DeepLearning_ObjectDetection_Onnx
    internal class OutputParser
    {
        //private const int ChannelCount = 125;
        private const int RowCount = 13;
        private const int ColCount = 13;
        private const int BoxesPerCell = 5;
        private const int BoxInfoFeatureCount = 5;
        private readonly int _classCount;
        private const float CellWidth = 32;
        private const float CellHeight = 32;
        private readonly int _channelStride = RowCount * ColCount;
        private readonly string[] _labels;

        public OutputParser(string[] labels)
        {
            _labels = labels;
            _classCount = labels.Length;
        }

        private readonly float[] _anchors =  {
            1.08F, 1.19F, 3.42F, 4.41F, 6.63F, 11.38F, 9.42F, 5.11F, 16.62F, 10.52F
        };

        private static readonly Color[] ClassColors = {
            Color.Khaki,
            Color.Fuchsia,
            Color.Silver,
            Color.RoyalBlue,
            Color.Green,
            Color.DarkOrange,
            Color.Purple,
            Color.Gold,
            Color.Red,
            Color.Aquamarine,
            Color.Lime,
            Color.AliceBlue,
            Color.Sienna,
            Color.Orchid,
            Color.Tan,
            Color.LightPink,
            Color.Yellow,
            Color.HotPink,
            Color.OliveDrab,
            Color.SandyBrown,
            Color.DarkTurquoise
        };

        private float Sigmoid(float value)
        {
            var k = (float)Math.Exp(value);
            return k / (1.0f + k);
        }

        private float[] Softmax(IReadOnlyList<float> values)
        {
            var maxVal = values.Max();
            var exp = values.Select(v => Math.Exp(v - maxVal)).ToList();
            var sumExp = exp.Sum();

            return exp.Select(v => (float)(v / sumExp)).ToArray();
        }

        private int GetOffset(int x, int y, int channel)
        {
            return channel * _channelStride + y * ColCount + x;
        }

        private BoundingBoxDimensions ExtractBoundingBoxDimensions(IReadOnlyList<float> modelOutput, int x, int y, int channel)
        {
            return new BoundingBoxDimensions
            {
                X = modelOutput[GetOffset(x, y, channel)],
                Y = modelOutput[GetOffset(x, y, channel + 1)],
                Width = modelOutput[GetOffset(x, y, channel + 2)],
                Height = modelOutput[GetOffset(x, y, channel + 3)]
            };
        }

        private float GetConfidence(IReadOnlyList<float> modelOutput, int x, int y, int channel)
        {
            return Sigmoid(modelOutput[GetOffset(x, y, channel + 4)]);
        }

        private BoundingBoxDimensions MapBoundingBoxToCell(int x, int y, int box, BoundingBoxDimensions boxDimensions)
        {
            return new BoundingBoxDimensions
            {
                X = (x + Sigmoid(boxDimensions.X)) * CellWidth,
                Y = (y + Sigmoid(boxDimensions.Y)) * CellHeight,
                Width = (float)Math.Exp(boxDimensions.Width) * CellWidth * _anchors[box * 2],
                Height = (float)Math.Exp(boxDimensions.Height) * CellHeight * _anchors[box * 2 + 1],
            };
        }

        private float[] ExtractClasses(IReadOnlyList<float> modelOutput, int x, int y, int channel)
        {
            var predictedClasses = new float[_classCount];
            var predictedClassOffset = channel + BoxInfoFeatureCount;
            for (var predictedClass = 0; predictedClass < _classCount; predictedClass++)
            {
                predictedClasses[predictedClass] = modelOutput[GetOffset(x, y, predictedClass + predictedClassOffset)];
            }
            return Softmax(predictedClasses);
        }

        private ValueTuple<int, float> GetTopResult(IEnumerable<float> predictedClasses)
        {
            return predictedClasses
                .Select((predictedClass, index) => (Index: index, Value: predictedClass))
                .OrderByDescending(result => result.Value)
                .First();
        }

        public IList<BoundingBox> ParseOutputs(IReadOnlyList<float> modelOutputs, float threshold)
        {
            var boxes = new List<BoundingBox>();

            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColCount; column++)
                {
                    for (var box = 0; box < BoxesPerCell; box++)
                    {
                        var channel = box * (_classCount + BoxInfoFeatureCount);
                        var boundingBoxDimensions = ExtractBoundingBoxDimensions(modelOutputs, row, column, channel);
                        var confidence = GetConfidence(modelOutputs, row, column, channel);
                        var mappedBoundingBox = MapBoundingBoxToCell(row, column, box, boundingBoxDimensions);

                        if (confidence < threshold)
                            continue;

                        var predictedClasses = ExtractClasses(modelOutputs, row, column, channel);

                        var (topResultIndex, topResultScore) = GetTopResult(predictedClasses);
                        var topScore = topResultScore * confidence;

                        if (topScore < threshold)
                            continue;

                        boxes.Add(new BoundingBox
                        {
                            Dimensions = new BoundingBoxDimensions
                            {
                                X = mappedBoundingBox.X - mappedBoundingBox.Width / 2,
                                Y = mappedBoundingBox.Y - mappedBoundingBox.Height / 2,
                                Width = mappedBoundingBox.Width,
                                Height = mappedBoundingBox.Height,
                            },
                            Confidence = topScore,
                            Label = _labels[topResultIndex],
                            BoxColor = ClassColors[topResultIndex]
                        });
                    }
                }
            }
            return boxes;
        }
    }
}
