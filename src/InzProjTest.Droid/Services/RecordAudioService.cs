using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Accord.Audio.Formats;
using Android.Media;
using InzProjTest.Core.Helpers;
using InzProjTest.Core.Interfaces;
using NAudio.Wave;

namespace InzProjTest.Droid.Services
{
    public class RecordAudioService : IRecordAudioService
    {
        public AudioRecord Recorder { get; set; }
        public byte[] AudioBuffer { get; set; }
        public string FilePath { get; set; }
        public int bufferSize { get; set; }
        public RecordAudioService()
        {
            bufferSize = AudioRecord.GetMinBufferSize(44100, ChannelIn.Mono, Encoding.Pcm16bit);;
            AudioBuffer = new byte[bufferSize]; //nie wiem jak ustalac odpowiedniy rozmiar buforu do nagran
            Recorder = new AudioRecord(AudioSource.Mic, 44100, ChannelIn.Mono, Encoding.Pcm16bit, AudioBuffer.Length);
        }
        public void StartRecording(string filepath)
        {
            //Recorder.StartRecording();
            FilePath = filepath;
            RecordAudio();
            //todo zapis naglowka pliku .WAV
        }

        private async void RecordAudio()
        {
            Recorder.StartRecording();
            await Task.Run(() => ReadAudio());
        }

        private void ReadAudio()
        {
            Recorder.Read(AudioBuffer, 0, AudioBuffer.Length);
            byte[] realData = AudioBuffer;
            using (FileStream fs = new FileStream(FilePath, FileMode.Create))
            {
                WaveHeaderWriter.WriteHeader(fs, realData.Length, 1, 44100);
                fs.Write(realData, 0, realData.Length);
                fs.Close();
            }
        }

        public void StopRecording()
        {
            Recorder.Stop();
            Recorder.Release();
        }
    }
}
