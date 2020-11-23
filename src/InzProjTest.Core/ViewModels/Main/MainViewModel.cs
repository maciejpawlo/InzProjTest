using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using InzProjTest.Core.Interfaces;
using InzProjTest.Core.Models;
using InzProjTest.Core.ViewModels.Results;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using NAudio.Wave;
using Plugin.AudioRecorder;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using MathNet;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NAudio.Dsp;
using Complex = System.Numerics.Complex;

namespace InzProjTest.Core.ViewModels.Main
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ISignalAnalyzer _signalAnalyzer;
        #region Commands
        public IMvxAsyncCommand RecordSoundAsyncCommand { get; set; }
        public IMvxAsyncCommand OpenFilesExplorerCommand { get; set; }
        public IMvxAsyncCommand AnalyzeSignalCommand { get; set; }
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
        private DateTime _creationDate;
        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                _creationDate = value;
                RaisePropertyChanged(() => CreationDate);
            }
        }
        #endregion
        public MainViewModel(IMvxNavigationService navigationService, ISignalAnalyzer signalAnalyzer)
        {
            _navigationService = navigationService;
            _signalAnalyzer = signalAnalyzer;
            RecordSoundAsyncCommand = new MvxAsyncCommand(RecordSoundAsync);
            OpenFilesExplorerCommand = new MvxAsyncCommand(OpenFilePickerAsync);
            AnalyzeSignalCommand = new MvxAsyncCommand(AnalyzeSignalAsync);
        }

        private async Task RecordSoundAsync()
        {
            DateTime todaysTime = DateTime.Now;
            var filename = "rec_" + $"{todaysTime:yyyyMMdd}_{todaysTime.Hour}{todaysTime.Minute}{todaysTime.Second}" + ".wav";
            var recorder = new AudioRecorderService()
            {
                StopRecordingOnSilence = false,
                TotalAudioTimeout = TimeSpan.FromSeconds(30),
                StopRecordingAfterTimeout =  true,
                FilePath = Mvx.IoCProvider.Resolve<ILocalFileHelper>().GetPath(filename),
                PreferredSampleRate = 4000,
            };
            Mvx.IoCProvider.Resolve<IUserDialogs>().ShowLoading("Nagrywanie...");
            var recordTask = await recorder.StartRecording();
            var audiofile = await recordTask;
            FilePath = audiofile;
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
            FileInfo fi = new FileInfo(FilePath);
            CreationDate = fi.CreationTime;
        }
        private async Task AnalyzeSignalAsync()
        {
            if (FilePath == null)
            {
                await Mvx.IoCProvider.Resolve<IUserDialogs>().AlertAsync("Nie wybrano żadnego pliku.", "Błąd odczytu pliku", "OK");
                return;
            }
            AudioFileReader reader = new AudioFileReader(FilePath); //odczyt wav
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
            Mvx.IoCProvider.Resolve<IUserDialogs>().ShowLoading("Trwa analiza syngału...");
            var framedFft = _signalAnalyzer.FrameSignal(fftInput, 10);
            float[] averagedSignal = new float[framedFft[0].Length];
            await Task.Run(() =>
            {
                foreach (var signal in framedFft)
                {
                    Fourier.Forward(signal, FourierOptions.Matlab);
                }
                var magnitudes = framedFft.Select(x => x.Select(v => v.Magnitude).ToArray()).ToList();
                averagedSignal = _signalAnalyzer.AverageSignal(magnitudes);
            });
            Signal mySignal = new Signal
            {
                Filename = FileName,
                Filepath = FilePath,
                Data = averagedSignal,
                SampleRate = isp.WaveFormat.SampleRate,
                CreationDate = this.CreationDate
            };
            Mvx.IoCProvider.Resolve<IUserDialogs>().HideLoading();
            await _navigationService.Navigate<ResultsViewModel, Signal>(mySignal);
        }
    }
}
