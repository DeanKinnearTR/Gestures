using Microsoft.ML.Data;

namespace RecognitionEngine
{
    internal class ImageOutputData
    {
        [ColumnName(ImageSettings.OutputColumnName)]
        public float[] PredictedLabels { get; set; }
    }
}
//KEEP THIS!
//internal class ImageOutputData
//{
//    [ColumnName("classLabel")]
//    public string[] PredictedLabels { get; set; }
//    [ColumnName("loss")]
//    [OnnxSequenceType(typeof(IDictionary<string, float>))]
//    public IEnumerable<IDictionary<string, float>> Scores { get; set; }
//}