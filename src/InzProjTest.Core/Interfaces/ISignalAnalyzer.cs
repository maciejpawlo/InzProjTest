using System.Collections.Generic;
using MathNet.Numerics;

namespace InzProjTest.Core.Interfaces
{
    public interface ISignalAnalyzer
    {
        List<Complex32[]> FrameSignal(Complex32[] fftInput, int framesCount); //ewentualnie zmienic na  List<float[]>??
        float[] AverageSignal(List<float[]> framedSignal);
        double[] GetPositiveFrequencyScale(double[] frequencies);
        float[] GetPositiveFreqMagnitudes(float[] magnitudes, double[] frequencies);
        float[] ToDecibelScale(float[] magnitudes);
        int[] GetHarmonics(double[] positiveFrequencies, float[] positiveMagnitudes);
    }
}