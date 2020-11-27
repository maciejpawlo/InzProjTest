using System;
using System.Collections.Generic;
using System.Text;
using InzProjTest.Core.Interfaces;
using MathNet.Numerics;
using NAudio.Wave;

namespace InzProjTest.Core.Services
{
    class WavReaderService : IWavReaderService
    {
        Complex32[] IWavReaderService.ReadWavFile(string filepath, out int sampleRate)
        {
            AudioFileReader reader = new AudioFileReader(filepath); //odczyt wav
            ISampleProvider isp = reader.ToSampleProvider();
            float[] buffer;
            switch (isp.WaveFormat.BitsPerSample) //wybor rozmiary bufora w zaleznosci od bitspersample
            {
                case 16:
                    buffer = new float[reader.Length / 2];
                    break;
                case 32:
                    buffer = new float[reader.Length / 4];
                    break;
                default:
                    buffer = new float[reader.Length / 2];
                    break;
            }
            isp.Read(buffer, 0, buffer.Length);
            Array.Resize(ref buffer, 120000);//dociąganie wav do odpowiedniej długości (wypełnianie zerami)
            Complex32[] fftInput = new Complex32[buffer.Length]; //testowo wersja bez okna
            for (int i = 0; i < fftInput.Length; i++)
            {
                fftInput[i] = new Complex32(buffer[i], 0);
            }
            sampleRate = isp.WaveFormat.SampleRate;
            return fftInput;
        } 
    }
}
