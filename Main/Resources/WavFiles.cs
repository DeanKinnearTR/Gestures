using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Reflection;

namespace RealtimeRecognition.Resources
{
    internal class WavFiles
    {
        private readonly List<byte[]> _positive;
        private readonly List<byte[]> _negative;
        private readonly Random _random;
        private int _lastPositiveIndex = -1;
        private int _lastNegativeIndex = -1;

        public WavFiles()
        {
            _positive = new List<byte[]>();
            _negative = new List<byte[]>();

            _random = new Random(DateTime.Now.Millisecond);

            var assembly = typeof(WavFiles).GetTypeInfo().Assembly;
            var resourceNames = assembly.GetManifestResourceNames();

            foreach (var resourceName in resourceNames)
            {
                using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceName.Contains(".WavFiles.Positive.", StringComparison.OrdinalIgnoreCase))
                    {
                        _positive.Add(resourceStream.ToByteArray());
                    }
                    else if (resourceName.Contains(".WavFiles.Negative.", StringComparison.OrdinalIgnoreCase))
                    {
                        _negative.Add(resourceStream.ToByteArray());
                    }
                }
            }
        }

        public void PlayPositive()
        {
            int randomIndex;
            do
                randomIndex = _random.Next(0, _positive.Count - 1);
            while (randomIndex == _lastPositiveIndex);
            _lastPositiveIndex = randomIndex;

            PlaySound(_positive[randomIndex]);
        }

        public void PlayNegative()
        {
            int randomIndex;
            do
                randomIndex = _random.Next(0, _negative.Count - 1);
            while (randomIndex == _lastNegativeIndex);
            _lastNegativeIndex = randomIndex;

            PlaySound(_negative[randomIndex]);
        }

        private static void PlaySound(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
            using (var player = new SoundPlayer(stream))
            {
                player.PlaySync();
            }
        }
    }
}