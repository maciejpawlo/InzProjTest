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
        private PlotModel _myModel;
        public PlotModel PlotModel
        {
            get => _myModel;
            set
            {
                _myModel = value;
                RaisePropertyChanged(() => PlotModel);
            }
        }

        private double _firstHarm;
        public double FirstHarm
        {
            get => _firstHarm;
            set
            {
                _firstHarm = value;
                RaisePropertyChanged(() => FirstHarm);
            }
        }

        private double _thirdHarm;
        public double ThirdHarm
        {
            get => _thirdHarm;
            set
            {
                _thirdHarm = value;
                RaisePropertyChanged(() => ThirdHarm);
            }
        }
        private double _forthHarm;
        public double ForthHarm
        {
            get => _forthHarm;
            set
            {
                _forthHarm = value;
                RaisePropertyChanged(() => ForthHarm);
            }
        }
        private double _thirdToForthRatio;
        public double ThirdToForthRatio
        {
            get => _thirdToForthRatio;
            set
            {
                _thirdToForthRatio = value;
                RaisePropertyChanged(() => ThirdToForthRatio);
            }
        }
        private double _rpmValue;
        public double RpmValue
        {
            get => _rpmValue;
            set
            {
                _rpmValue = value;
                RaisePropertyChanged(() => RpmValue);
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
                var magnitude = new float[FftData.Data.Length];
                Array.Copy(FftData.Data, magnitude, magnitude.Length);
                magnitude = _signalAnalyzer.ToDecibelScale(magnitude);
                var freq = Fourier.FrequencyScale(FftData.Data.Length, FftData.SampleRate);

                var positiveFreqTest = _signalAnalyzer.GetPositiveFrequencyScale(freq);
                var positiveMagTest = _signalAnalyzer.GetPositiveFreqMagnitudes(magnitude, freq);
                var harmonicIndices = _signalAnalyzer.GetHarmonics(positiveFreqTest, positiveMagTest);

                var harmfreq1 = positiveFreqTest[harmonicIndices[0]];
                var harmfreq3 = positiveFreqTest[harmonicIndices[1]];
                var harmfreq4 = positiveFreqTest[harmonicIndices[2]];

                RpmValue = harmfreq1 * 60;
                FirstHarm = Math.Round(harmfreq1, 0);
                ThirdHarm = Math.Round(harmfreq3, 0);
                ForthHarm = Math.Round(harmfreq4, 0);

                var model = new PlotModel
                {
                    PlotAreaBorderColor = OxyColors.Black,
                    LegendTextColor = OxyColors.Black,
                    LegendTitleColor = OxyColors.Black,
                    TextColor = OxyColors.Black,
                    Background = OxyColors.White,
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
                    seriesHarmonics.Points.Add(new DataPoint(positiveFreqTest[harmonicIndices[i]],
                        positiveMagTest[harmonicIndices[i]]));
                }
                for (int i = 0; i < positiveFreqTest.Length; i++)
                {
                    series1.Points.Add(new DataPoint(positiveFreqTest[i], positiveMagTest[i]));
                }

                model.Series.Add(series1);
                model.Series.Add(seriesHarmonics);
                PlotModel = model;
            }));
        }

        public override void Prepare(Signal parameter) => FftData = parameter;
    }
}
