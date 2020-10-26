using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.Audio.Windows;
using Accord.DirectSound;
using Accord.Math;
using Acr.UserDialogs;
using InzProjTest.Core.Helpers;
using InzProjTest.Core.Interfaces;
using InzProjTest.Core.ViewModels.Results;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using NAudio.Wave;
using Plugin.AudioRecorder;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Xamarin.Essentials;

namespace InzProjTest.Core.ViewModels.Main
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        #region Commands
        public IMvxAsyncCommand RecordSoundAsyncCommand { get; set; }
        public IMvxAsyncCommand OpenFilesExplorerCommand { get; set; }
        public IMvxCommand AnalyzeSignalCommand { get; set; }
        public IMvxCommand StopRecordingCommand { get; set; }

        #endregion

        #region Properties
        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                RaisePropertyChanged(() => FilePath);
            }
        }
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                RaisePropertyChanged(() => FileName);
            }
        }
        #endregion
        public MainViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
            RecordSoundAsyncCommand = new MvxAsyncCommand(RecordSoundAsync);
            OpenFilesExplorerCommand = new MvxAsyncCommand(OpenFilePickerAsync);
            AnalyzeSignalCommand = new MvxAsyncCommand(AnalyzeSignalAsync);
            StopRecordingCommand = new MvxCommand(StopRecording); //natywna próba nagrywania/stopowania nagrania, AKTUALNIE NIE UŻYWANA
        }

        private async Task RecordSoundAsync()
        {
            DateTime todaysTime = DateTime.Now;
            var filename = "rec_" + string.Format("{0}_{1}{2}{3}", todaysTime.ToString("yyyyMMdd"), todaysTime.Hour,
                todaysTime.Minute, todaysTime.Second) + ".wav";

            var recorder = new AudioRecorderService()
            {
                StopRecordingOnSilence = false,
                TotalAudioTimeout = TimeSpan.FromSeconds(11),
                StopRecordingAfterTimeout =  true,
                FilePath = Mvx.IoCProvider.Resolve<ILocalFileHelper>().GetPath(filename),
            };
            Mvx.IoCProvider.Resolve<IUserDialogs>().ShowLoading("Nagrywanie...");
            var recordTask = await recorder.StartRecording();
            var audiofile = await recordTask;
            FilePath = recorder.FilePath; //normalnie audiofile, do testow zmiana
            FileName = filename;
            Mvx.IoCProvider.Resolve<IUserDialogs>().HideLoading();
        }

        private async Task OpenFilePickerAsync()
        {
            FileData file = await CrossFilePicker.Current.PickFile();
            if (file == null)
            {
                return;
            }
            FilePath = file.FilePath;
            FileName = file.FileName;
        }

        private void StopRecording() //natywna próba nagrywania/stopowania nagrania, AKTUALNIE NIE UŻYWANA
        {
            Mvx.IoCProvider.Resolve<IRecordAudioService>().StopRecording();
        }

        private Task AnalyzeSignalAsync()
        {
            if (FilePath == null)
            {
                return Mvx.IoCProvider.Resolve<IUserDialogs>().AlertAsync("Nie wybrano żadnego pliku.", "Błąd odczytu pliku", "OK");
            }
            #region test

            //Signal audio;
            //using (AudioFileReader reader = new AudioFileReader(FilePath))
            //{
            //    float[] samples = new float[reader.Length / 4];
            //    int offset = 0;
            //    while (reader.Position + 4 * 4096 < reader.Length)
            //    {
            //        offset += reader.Read(samples, offset, 4096);
            //    }
            //    audio = Signal.FromArray(samples, reader.WaveFormat.SampleRate);
            //}
            //double[] sound;
            //using (WaveFileReader reader = new WaveFileReader(FilePath))
            //{
            //    byte[] bytesBuffer = new byte[reader.Length];
            //    int read = reader.Read(bytesBuffer, 0, bytesBuffer.Length);
            //    var floatSamples = new double[read / 2];
            //    for (int sampleIndex = 0; sampleIndex < read / 2; sampleIndex++)
            //    {
            //        var intSampleValue = BitConverter.ToInt16(bytesBuffer, sampleIndex * 2);
            //        floatSamples[sampleIndex] = intSampleValue / 32768.0;
            //    }

            //    sound = floatSamples;
            //}
            #endregion

            //double[] l;
            //double[] r;
            //var a = WavReader.ReadWav(FilePath, out l, out r);
            //var audiodecode = AudioDecoder.DecodeFromFile(FilePath);
            //Signal testSignal;
            //using (WaveFileReader reader = new WaveFileReader(FilePath))
            //{
            //    byte[] buffer = new byte[reader.Length];
            //    int read = reader.Read(buffer, 0, buffer.Length);
            //    short[] sampleBuffer = new short[read / 2];
            //    Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);
            //    testSignal = Signal.FromArray(sampleBuffer, (int)reader.SampleCount, 1, 48000,SampleFormat.Format16Bit);
            //}


            var waveDecoder = new WaveDecoder(FilePath);
            Signal test = waveDecoder.Decode();

            var window = RaisedCosineWindow.Hamming(16384);
            var testComplex = window.Apply(test, 0).ToComplex();
            testComplex.ForwardFourierTransform();
            return _navigationService.Navigate<ResultsViewModel, ComplexSignal>(testComplex);
        }
        
    }
}
