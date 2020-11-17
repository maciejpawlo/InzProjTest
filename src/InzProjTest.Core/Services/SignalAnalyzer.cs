using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using InzProjTest.Core.Interfaces;
using MathNet.Numerics;

namespace InzProjTest.Core.Services
{
    public class SignalAnalyzer : ISignalAnalyzer
    {
        float[] ISignalAnalyzer.AverageSignal(List<float[]> framedSignal) //todo zmiania argumentu na List<Complex32>
        {
            float[] result = new float[framedSignal[0].Length];
            float[] tmp = new float[framedSignal[0].Length];
            var testRes = Enumerable.Range(0, framedSignal[0].Length).Select(i => framedSignal.Sum(p => p[i]))
                .ToArray();
            result = testRes.Select(x => x / framedSignal.Count).ToArray();
            return result;
        }

        List<Complex32[]> ISignalAnalyzer.FrameSignal(Complex32[] fftInput, int framesCount)
        {
            var framedFft = fftInput.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / (fftInput.Length / framesCount))
                .Select(x => x.Select(v => v.Value).ToArray())
                .ToList();
            return framedFft;
        }

        int[] ISignalAnalyzer.GetHarmonics(double[] positiveFrequencies, float[] positiveMagnitudes) //TODO REFRACTOR
        {
            var indexArray = new int[3];
            //zakres czestotliwosci pracy pompy
            var firstHarmStartFreq = 30;
            var firstHarmStopFreq = 59;
            //wyszukiwanie indeksow odpowiadajacych dwóm częstotliwościom
            var closestToFirstHarmStart = positiveFrequencies.Aggregate((x, y) =>
                Math.Abs(x - firstHarmStartFreq) < Math.Abs(y - firstHarmStartFreq) ? x : y);
            var closestToFirstHarmStartIndex = Array.IndexOf(positiveFrequencies, closestToFirstHarmStart);

            var closestToFirstHarmStop = positiveFrequencies.Aggregate((x, y) =>
                Math.Abs(x - firstHarmStopFreq) < Math.Abs(y - firstHarmStopFreq) ? x : y);
            var closestToFirstHarmStopIndex = Array.IndexOf(positiveFrequencies, closestToFirstHarmStop);
            //Maximum z zakresu czestotliwosci => I harmoniczna
            var firstHarmonicMagnitude = positiveMagnitudes.Skip(closestToFirstHarmStartIndex - 1)
                .Take(closestToFirstHarmStopIndex - closestToFirstHarmStartIndex + 1).Max();
            var firstHarmonicIndex = Array.IndexOf(positiveMagnitudes, firstHarmonicMagnitude);
            //Wyszukiwanie III harmonicznej
            var thirdHarmFrequencyEstimated = positiveFrequencies[firstHarmonicIndex] * 3;

            var thirdHarmonicFreq = positiveFrequencies.Aggregate((x, y) =>
                Math.Abs(x - thirdHarmFrequencyEstimated) < Math.Abs(y - thirdHarmFrequencyEstimated) ? x : y);
            var thirdHarmonicIndex = Array.IndexOf(positiveFrequencies, thirdHarmonicFreq);
            //Wyszukiwanie IV harmonicznej 
            var forthHarmFrequencyEstimated = positiveFrequencies[firstHarmonicIndex] * 4;

            var forthHarmonicFreq = positiveFrequencies.Aggregate((x, y) =>
                Math.Abs(x - forthHarmFrequencyEstimated) < Math.Abs(y - forthHarmFrequencyEstimated) ? x : y);
            var forthHarmonicIndex = Array.IndexOf(positiveFrequencies, forthHarmonicFreq);

            indexArray[0] = firstHarmonicIndex;
            indexArray[1] = thirdHarmonicIndex;
            indexArray[2] = forthHarmonicIndex;

            return indexArray;
        }

        float[] ISignalAnalyzer.GetPositiveFreqMagnitudes(float[] magnitude, double[] frequencies)
        {
            var firstNegativeFreq = frequencies.FirstOrDefault(i => i < 0);
            var firstNegativeFreqIndex = Array.IndexOf(frequencies, firstNegativeFreq);
            return magnitude.Take(firstNegativeFreqIndex).ToArray();
        }

        double[] ISignalAnalyzer.GetPositiveFrequencyScale(double[] frequencies)
        {
            return frequencies.Where(x => x >= 0).ToArray();
        }

        float[] ISignalAnalyzer.ToDecibelScale(float[] magnitude)
        {
            var maxMag = magnitude.Max();
            for (int i = 0; i < magnitude.Length; i++) //zamiana magnitude --> decibel
            {
                magnitude[i] = (float)(20 * Math.Log10(magnitude[i] / maxMag));
            }
            return magnitude;
        }
    }
}
