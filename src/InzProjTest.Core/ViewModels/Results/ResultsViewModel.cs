using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Linq;
using System.Net.Http.Headers;
using System.Transactions;
using InzProjTest.Core.Interfaces;
using InzProjTest.Core.Models;
using InzProjTest.Core.Services;

namespace InzProjTest.Core.ViewModels.Results
{
    public class ResultsViewModel : BaseViewModel<Signal>
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ISignalAnalyzer _signalAnalyzer;
        #region Properties

        public Signal FftData { get; set; }
         //public Complex32[] FftData { get; set; }
        //public PlotModel PlotModel => GeneratePlotModel();
        PlotModel _myModel;
        public PlotModel PlotModel
        {
            get => _myModel;
            set
            {
                _myModel = value;
                RaisePropertyChanged(() => PlotModel);
            }
        }
        #endregion

        public ResultsViewModel(IMvxNavigationService navigationService, ISignalAnalyzer signalAnalyzer)
        {
            _navigationService = navigationService;
            _signalAnalyzer = signalAnalyzer;
        }

        public override async Task Initialize()
        {
            Mvx.IoCProvider.Resolve<IUserDialogs>().ShowLoading("Ładowanie...");
            await GeneratePlotModelAsync();
            Mvx.IoCProvider.Resolve<IUserDialogs>().HideLoading();
        }

        private async Task GeneratePlotModelAsync() //todo asynchroniczna zeby nie blokować UI thread
        {
            await Task.Run((() =>
            {
                //float[] magnitude = FftData.Select(x => x.Magnitude).ToArray();
                var magnitude = FftData.Data;
                float[] tempMagnitude = new float[magnitude.Length]; //TYLKO DO TESTÓW
                Array.Copy(FftData.Data, tempMagnitude, tempMagnitude.Length);
                var maxMag = magnitude.Max();
                for (int i = 0; i < magnitude.Length; i++) //zamiana magnitude --> decibel
                {
                    magnitude[i] = (float)(20 * Math.Log10(magnitude[i]/maxMag));
                }
                var test = _signalAnalyzer.ToDecibelScale(tempMagnitude); //TYLKO DO TESTÓW
                var freq = Fourier.FrequencyScale(FftData.Data.Length, FftData.SampleRate); //todo przenoszenie sampleRate, ogarniecie skali na x (screenshot YT)
                //WYCIAGANIE WEKTORA DODATNICH CZESTOTLIWOSCI I ODPOWIADAJACYCH AMPLITUD SYGNALU
                var positiveFreq = freq.Where(x => x >= 0).ToArray(); //dodatni wektor czestotliwosc
                var firstNegativeFreq = freq.FirstOrDefault(i => i < 0);
                var firstNegativeFreqIndex = Array.IndexOf(freq, firstNegativeFreq);
                var positiveFreqMagnitude = magnitude.Take(firstNegativeFreqIndex).ToArray();

                var harmonicIndices = _signalAnalyzer.GetHarmonics(positiveFreq, positiveFreqMagnitude);

                var model = new PlotModel
                {
                    PlotAreaBorderColor = OxyColors.Black,
                    LegendTextColor = OxyColors.Black,
                    LegendTitleColor = OxyColors.Black,
                    TextColor = OxyColors.Black,
                };
                model.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    TitleColor = OxyColors.Black,
                    AxislineColor = OxyColors.Black,
                    TicklineColor = OxyColors.Black,
                    Title = "Częstotliwość [Hz]",
                    Minimum = 0,
                    Maximum = 300,
                });
                model.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    TitleColor = OxyColors.Black,
                    AxislineColor = OxyColors.Black,
                    Title = "Amplituda [dB]",
                    TicklineColor = OxyColors.Black,
                    //Minimum = -70,
                    IntervalLength = 50,
                });
                var series1 = new LineSeries
                {
                    Color = OxyColors.Red,
                    StrokeThickness = 0.5
                };
                var seriesHarmonics = new LineSeries
                {
                    Color = OxyColors.Blue,
                    MarkerSize = 3,
                    MarkerType = MarkerType.Circle,
                    StrokeThickness = 0
                };
                #region rysowanie pełnego widma fft
                //for (int i = 0; i < magnitude.Length; i++)
                //{
                //    series1.Points.Add(new DataPoint(freq[i], magnitude[i]));
                //}
                #endregion

                for (int i = 0; i < harmonicIndices.Length; i++)
                {
                    seriesHarmonics.Points.Add(new DataPoint(positiveFreq[harmonicIndices[i]],
                        positiveFreqMagnitude[harmonicIndices[i]]));
                }
                for (int i = 0; i < positiveFreqMagnitude.Length; i++)
                {
                    series1.Points.Add(new DataPoint(positiveFreq[i], positiveFreqMagnitude[i]));
                }

                model.Series.Add(series1);
                model.Series.Add(seriesHarmonics);
                PlotModel = model;
            }));
        }

        public override void Prepare(Signal parameter) => FftData = parameter;
    }
}
