using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace InzProjTest.Core.Helpers
{
    public static class WavReader
    {
        public static bool ReadWav(string filepath, out double[] L, out double[] R)
        {
            L = R = null;
            var readfile = File.ReadAllBytes(filepath);
            using (FileStream fs = File.Open(filepath, FileMode.Open))
            {
                //WaveDecoder waveDecoder = new WaveDecoder(fs);
                var reader = new BinaryReader(fs);
                // chunk 0
                var chunkID = reader.ReadChars(4);
                int fileSize = reader.ReadInt32();
                var riffType = reader.ReadChars(4);

                // chunk 1
                int fmtID = reader.ReadInt32();
                int fmtSize = reader.ReadInt32(); // bytes for this chunk
                int fmtCode = reader.ReadInt16();
                int channels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32(); //czestotliwosc probkowania
                int byteRate = reader.ReadInt32();
                int fmtBlockAlign = reader.ReadInt16();
                int bitDepth = reader.ReadInt16(); //rozdzielczosc bitowa 

                if (fmtSize == 18)
                {
                    // Read any extra values
                    int fmtExtraSize = reader.ReadInt16();
                    reader.ReadBytes(fmtExtraSize);
                }

                int bytes;
                var dataHeader = reader.ReadChars(4);

                bytes = reader.ReadInt32(); //rozmiar pliku?
                byte[] byteArray = reader.ReadBytes(bytes);


                int bytesForSamp = bitDepth / 8;
                int samps = bytes / bytesForSamp; //ilosc probek == dlugosc tablicy wyjsciowej 
                double[] soundData = new double[samps];
                switch (bitDepth)
                {
                    case 64:
                        //double[] asDouble = new double[samps];
                        Buffer.BlockCopy(byteArray, 0, soundData, 0, byteArray.Length);
                        //asFloat = Array.ConvertAll(asDouble, e => (float)e);

                        break;
                    case 32:
                        //asFloat = new float[samps];
                        Buffer.BlockCopy(byteArray, 0, soundData, 0, byteArray.Length);

                        break;
                    case 16:
                        //Int16[] asInt16 = new Int16[samps];
                        // Buffer.BlockCopy(byteArray, 0, soundData, 0, samps);
                        //asFloat = Array.ConvertAll(asInt16, e => e / (float)Int16.MaxValue);
                        Buffer.BlockCopy(byteArray, 0, soundData, 0, byteArray.Length);
                        break;
                    default:
                        return false;
                }

                switch (channels)
                {
                    case 1:
                        L = soundData;
                        R = null;
                        return true;
                    case 2:
                        L = new double[samps];
                        R = new double[samps];
                        for (int i = 0, s = 0; i < samps; i++)
                        {
                            L[i] = soundData[s++];
                            R[i] = soundData[s++];
                        }

                        return true;
                    default:
                        return false;
                }
            }
        }

        public static byte[] ReadWav(string filepath)
        {
            byte[] wavAudio;
            using (FileStream fs = File.Open(filepath, FileMode.Open))
            {
                var reader = new BinaryReader(fs);
                // chunk 0
                var chunkID = reader.ReadChars(4);
                int fileSize = reader.ReadInt32();
                var riffType = reader.ReadChars(4);

                // chunk 1
                int fmtID = reader.ReadInt32();
                int fmtSize = reader.ReadInt32(); // ilosc bitow chunka 
                int fmtCode = reader.ReadInt16();
                int channels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32(); //czestotliwosc probkowania
                int byteRate = reader.ReadInt32();
                int fmtBlockAlign = reader.ReadInt16();
                int bitDepth = reader.ReadInt16(); //rozdzielczosc bitowa 

                if (fmtSize == 18)
                {
                    int fmtExtraSize = reader.ReadInt16();
                    reader.ReadBytes(fmtExtraSize);
                }

                int bytes;
                var dataHeader = reader.ReadChars(4);

                bytes = reader.ReadInt32(); //rozmiar pliku?
                byte[] byteArray = reader.ReadBytes(bytes);
                wavAudio = byteArray;
            }

            return wavAudio;

        }
    }

}

