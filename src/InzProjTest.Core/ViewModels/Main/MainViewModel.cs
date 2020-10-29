using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
        #endregion
        public MainViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
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
                TotalAudioTimeout = TimeSpan.FromSeconds(11),
                StopRecordingAfterTimeout =  true,
                FilePath = Mvx.IoCProvider.Resolve<ILocalFileHelper>().GetPath(filename),
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
        }
        private async Task AnalyzeSignalAsync()
        {
            if (FilePath == null)
            {
                await Mvx.IoCProvider.Resolve<IUserDialogs>().AlertAsync("Nie wybrano żadnego pliku.", "Błąd odczytu pliku", "OK");
                return;
            }
            AudioFileReader reader = new AudioFileReader(FilePath); //DZIAŁA!!!!
            ISampleProvider isp = reader.ToSampleProvider();
            float[] buffer = new float[reader.Length / 2];
            isp.Read(buffer, 0, buffer.Length);

            //todo najblizsza potega dwojki
            var window = Window.Hamming(16384);
            Complex32[] fftInput = new Complex32[buffer.Length]; //testowo wersja z oknem
            for (int i = 0; i < fftInput.Length; i++)
            {
                fftInput[i] = new Complex32(buffer[i], 0);
            }
            Mvx.IoCProvider.Resolve<IUserDialogs>().ShowLoading("Trwa analiza syngału...");
            await Task.Run(()=>
            {
                Fourier.Forward(fftInput, FourierOptions.Matlab);
            });
            await _navigationService.Navigate<ResultsViewModel, Complex32[]>(fftInput);
            Mvx.IoCProvider.Resolve<IUserDialogs>().HideLoading();
        }
        
    }
}
