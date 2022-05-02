namespace RecognitionEngine
{
    internal struct ImageSettings
    {
        public const int ImageHeight = 416;
        public const int ImageWidth = 416;
        public static string[] Labels = {
            "Bird", "Thumb"
        };

        public const string OutputColumnName = "model_outputs0";
        public const string InputColumnName = "data";
    }
}
