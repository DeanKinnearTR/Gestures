using System;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using RealtimeRecognition.Resources;
using RecognitionEngine;

namespace RealtimeRecognition
{
    //Tutorial: https://docs.microsoft.com/en-us/dotnet/machine-learning/tutorials/object-detection-onnx
    //Generate ONXX method: https://www.customvision.ai/ Azure account needed. Can create for free (f0). TODO: Create own onxx generator
    //Read ONNX model properties: https://github.com/lutzroeder/netron (input & output properties)
    //PM> Install-Package Microsoft.ML.OnnxRuntime -Version 1.11.0, All all versions use an internal c++ binary dll, but not all versions come with said binary dll, wtf

    public class Program
    {
        private static void Main()
        {
            var lastImageType = ImageType.None;
            var defaultConsoleColor = Console.ForegroundColor;

            using (var identifier = new Identifier(@"model.onnx", 0.4f))
            {
                using (var capture = new VideoCapture(0))
                {
                    capture.Open(0);
                    if (!capture.IsOpened())
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine("Camera 0 failed to open.");
                        Console.ForegroundColor = defaultConsoleColor;
                        Console.ReadLine();
                        return;
                    }

                    var waves = new WavFiles();

                    while (true)
                    {
                        var frame = new Mat();
                        capture.Read(frame);
                        using (var image = frame.ToBitmap())
                        {
                            //image.Save($@"C:\$WIP\Gestures\Test\{DateTime.Now.Ticks}.jpg", ImageFormat.Jpeg);
                            var result = identifier.IdentifyImageType(image);
                            if (result == ImageType.Unknown && lastImageType == ImageType.Unknown) continue;
                            lastImageType = result;

                            switch (result)
                            {
                                case ImageType.Bird:
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.WriteLine(result);
                                    waves.PlayNegative();
                                    break;

                                case ImageType.Thumb:
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine(result);
                                    waves.PlayPositive();
                                    break;

                                default:
                                    Console.ForegroundColor = defaultConsoleColor;
                                    Console.WriteLine("Idle");
                                    break;
                            }
                        }

                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
        }
    }
}
