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
using Xamarin.Essentials;
using Complex = System.Numerics.Complex;

namespace InzProjTest.Core.ViewModels.Main
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ISignalAnalyzer _signalAnalyzer;
        private readonly IWavReaderService _wavReader;
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
        private bool _isRecSessionChecked;
        public bool IsRecSessionChecked
        {
            get => _isRecSessionChecked;
            set
            {
                _isRecSessionChecked = value;
                RaisePropertyChanged(() => IsRecSessionChecked);
            }
        }

        private int _recSessionCounter;

        public int RecSessionCounter
        {
            get => _recSessionCounter;
            set => SetProperty(ref _recSessionCounter, value);
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                RaisePropertyChanged(() => FirstName);
            }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                RaisePropertyChanged(() => LastName);
            }
        }
        #endregion
        public MainViewModel(IMvxNavigationService navigationService, ISignalAnalyzer signalAnalyzer, IWavReaderService wavReader)
        {
            _navigationService = navigationService;
            _signalAnalyzer = signalAnalyzer;
            _wavReader = wavReader;
            RecordSoundAsyncCommand = new MvxAsyncCommand(RecordSoundAsync);
            OpenFilesExplorerCommand = new MvxAsyncCommand(OpenFilePickerAsync);
            AnalyzeSignalCommand = new MvxAsyncCommand(AnalyzeSignalAsync);
        }

        private async Task RecordSoundAsync()
        {
            var permissionMic = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (permissionMic != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.Microphone>();
                return;
            }
            DateTime todaysTime = DateTime.Now;
            string filename = "";
            if (IsRecSessionChecked && !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            {
                FirstName = FirstName.Trim();
                LastName = LastName.Trim();
                filename = $"{FirstName}_" + $"{LastName}_" + $"{todaysTime:yyyyMMdd}_{todaysTime.Hour}{todaysTime.Minute}{todaysTime.Second}" + ".wav";
            }
            else if (IsRecSessionChecked && (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName)))
            {
                await Mvx.IoCProvider.Resolve<IUserDialogs>().AlertAsync("Pola tekstowe nie mogą być puste!", "Błąd", "Ok");
                return;
            }
            else
            {
                filename = "rec_" + $"{todaysTime:yyyyMMdd}_{todaysTime.Hour}{todaysTime.Minute}{todaysTime.Second}" + ".wav";
            }
            var recorder = new AudioRecorderService()
            {
                StopRecordingOnSilence = false,
                TotalAudioTimeout = TimeSpan.FromSeconds(30),
                StopRecordingAfterTimeout =  true,
                FilePath = IsRecSessionChecked ? Mvx.IoCProvider.Resolve<ILocalFileHelper>().GetPatientPath(filename, FirstName, LastName)
                    : Mvx.IoCProvider.Resolve<ILocalFileHelper>().GetPath(filename),
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
            var device = DeviceInfo.Platform;
            string[] fileTypes = null; //ograniczenie dostępnych rozszerzeń do .wav
            if (device == DevicePlatform.Android)
                fileTypes = new string[]{ "audio/x-wav" };
            if (device == DevicePlatform.iOS)
                fileTypes = new string[] { "public.wav" };

            FileData file = await CrossFilePicker.Current.PickFile(fileTypes);
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
            var fftInput = _wavReader.ReadWavFile(FilePath, out var sampleRate);
            Mvx.IoCProvider.Resolve<IUserDialogs>().ShowLoading("Trwa analiza syngału...");
            var framedFft = _signalAnalyzer.FrameSignal(fftInput, 10);
            float[] averagedSignal = new float[framedFft[0].Length];
            await Task.Run(() =>
            {
                foreach (var signal in framedFft)
                {
                    Fourier.Forward(signal, FourierOptions.Matlab);
                }
                averagedSignal = _signalAnalyzer.AverageSignal(framedFft);
            });
            Signal mySignal = new Signal
            {
                Filename = FileName,
                Filepath = FilePath,
                Data = averagedSignal,
                SampleRate = sampleRate,
                CreationDate = this.CreationDate
            };
            Mvx.IoCProvider.Resolve<IUserDialogs>().HideLoading();
            await _navigationService.Navigate<ResultsViewModel, Signal>(mySignal);
        }
    }
    
}
