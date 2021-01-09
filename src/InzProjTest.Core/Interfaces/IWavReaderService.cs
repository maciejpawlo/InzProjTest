using MathNet.Numerics;

namespace InzProjTest.Core.Interfaces
{
    public interface IWavReaderService
    {
        Complex32[] ReadWavFile(string filepath, out int sampleRate);
    }
}
