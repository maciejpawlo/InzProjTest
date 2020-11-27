using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InzProjTest.Core.Interfaces;
using MathNet.Numerics;

namespace InzProjTest.Core.Services
{
    public class SignalAnalyzer : ISignalAnalyzer
    {
        float[] ISignalAnalyzer.AverageSignal(List<Complex32[]> framedSignal) //todo zmiania argumentu na List<Complex32>
        {
            var magnitudes = framedSignal.Select(x => x.Select(v => v.Magnitude).ToArray()).ToList();
            float[] result = new float[framedSignal[0].Length];
            var testRes = Enumerable.Range(0, framedSignal[0].Length).Select(i => magnitudes.Sum(p => p[i]))
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
            #region WYSZUKIWANIE NA PODSTAWIE I HARMONICZNEJ
            //var indexArray = new int[3];
            ////zakres czestotliwosci pracy pompy
            //var firstHarmStartFreq = 30;
            //var firstHarmStopFreq = 59;
            ////wyszukiwanie indeksow odpowiadajacych dwóm częstotliwościom
            //var closestToFirstHarmStart = positiveFrequencies.Aggregate((x, y) =>
            //    Math.Abs(x - firstHarmStartFreq) < Math.Abs(y - firstHarmStartFreq) ? x : y);
            //var closestToFirstHarmStartIndex = Array.IndexOf(positiveFrequencies, closestToFirstHarmStart);

            //var closestToFirstHarmStop = positiveFrequencies.Aggregate((x, y) =>
            //    Math.Abs(x - firstHarmStopFreq) < Math.Abs(y - firstHarmStopFreq) ? x : y);
            //var closestToFirstHarmStopIndex = Array.IndexOf(positiveFrequencies, closestToFirstHarmStop);
            ////Maximum z zakresu czestotliwosci => I harmoniczna
            //var firstHarmonicMagnitude = positiveMagnitudes.Skip(closestToFirstHarmStartIndex - 1)
            //    .Take(closestToFirstHarmStopIndex - closestToFirstHarmStartIndex + 1).Max();
            //var firstHarmonicIndex = Array.IndexOf(positiveMagnitudes, firstHarmonicMagnitude);
            ////Wyszukiwanie III harmonicznej
            //var thirdHarmFrequencyEstimated = positiveFrequencies[firstHarmonicIndex] * 3;

            //var thirdHarmonicFreq = positiveFrequencies.Aggregate((x, y) =>
            //    Math.Abs(x - thirdHarmFrequencyEstimated) < Math.Abs(y - thirdHarmFrequencyEstimated) ? x : y);
            //var thirdHarmonicIndex = Array.IndexOf(positiveFrequencies, thirdHarmonicFreq);
            ////Wyszukiwanie IV harmonicznej 
            //var forthHarmFrequencyEstimated = positiveFrequencies[firstHarmonicIndex] * 4;

            //var forthHarmonicFreq = positiveFrequencies.Aggregate((x, y) =>
            //    Math.Abs(x - forthHarmFrequencyEstimated) < Math.Abs(y - forthHarmFrequencyEstimated) ? x : y);
            //var forthHarmonicIndex = Array.IndexOf(positiveFrequencies, forthHarmonicFreq);

            //indexArray[0] = firstHarmonicIndex;
            //indexArray[1] = thirdHarmonicIndex;
            //indexArray[2] = forthHarmonicIndex;

            //return indexArray;

            #endregion

            //WYSZUKIWANIE IV HARMONICZNEJ 
            var indexArray = new int[3];
            var forthHarmStartFreq = 120; //30Hz*4
            var forthHarmStopFreq = 236; //59Hz*4
            //wyszukiwanie indexów odpowiadającym częstotliwościom zakresu
            var closestToForthtHarmStart = positiveFrequencies.Aggregate((x, y) =>
                Math.Abs(x - forthHarmStartFreq) < Math.Abs(y - forthHarmStartFreq) ? x : y);
            var closestToForthHarmStartIndex = Array.IndexOf(positiveFrequencies, closestToForthtHarmStart);
            var closestToForthHarmStop = positiveFrequencies.Aggregate((x, y) =>
                Math.Abs(x - forthHarmStopFreq) < Math.Abs(y - forthHarmStopFreq) ? x : y);
            var closestToForthHarmStopIndex = Array.IndexOf(positiveFrequencies, closestToForthHarmStop);
            //MAX z zakresu częstotliwości ---> IV Harmoniczna 
            var forthHarmonicMagnitude = positiveMagnitudes.Skip(closestToForthHarmStartIndex - 1)
                .Take(closestToForthHarmStopIndex - closestToForthHarmStartIndex + 1).Max();
            var forthHarmonicIndex = Array.IndexOf(positiveMagnitudes, forthHarmonicMagnitude);

            //TODO WALIDACJA INDEXOW --> SPRAWDZIC JEDEN INDEX W LEWO I JEDEN W PRAWO

            var firstHarmonicIndex = forthHarmonicIndex / 4;
            var thirdHarmonicIndex = firstHarmonicIndex * 3;
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

        double ISignalAnalyzer.MeasureHarmonicAmplitude(double[] positiveFrequencies, float[] positiveMagnitudes, int index) //TODO poki co zwraca prosta pomiedzy dwoma pkt
        {
            var frequenciesRight = positiveFrequencies[(index + 40)..(index + 70)];
            var frequenciesLeft = positiveFrequencies[(index - 70)..(index - 40)];

            var magnitudesRight = positiveMagnitudes[(index + 40)..(index + 70)];
            var magnitudesLeft = positiveMagnitudes[(index - 70)..(index - 40)];

            //uśrednianie punktów
            var rightX = frequenciesRight.Average();
            var rightY = magnitudesRight.Average();

            var leftX = frequenciesLeft.Average();
            var leftY = magnitudesLeft.Average();
            //prosta między dwoma punktami
            var a = (rightY - leftY) / (rightX - leftX);
            var b = (leftY - a * leftX);
            //wektor stworzony na podstawie y=ax+b
            var vector = new double[positiveFrequencies.Length];
            for (int i = 0; i < positiveFrequencies.Length; i++)
            {
                vector[i] = a * positiveFrequencies[i] + b;
            }
            //obliczanie amplitudy
            var amplitudeCondition = positiveMagnitudes[index] - vector[index];
            var amplitude = amplitudeCondition < 0 ? 0 : amplitudeCondition;
            return amplitude;
            //return vector; //DO RYSOWANIA FUNKCJI LINIOWEJ
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
