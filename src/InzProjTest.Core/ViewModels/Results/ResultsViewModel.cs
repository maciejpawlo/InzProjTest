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

namespace InzProjTest.Core.ViewModels.Results
{
    public class ResultsViewModel : BaseViewModel<float[]>
    {
        private readonly IMvxNavigationService _navigationService;

        #region Properties

        public float[] FftData { get; set; }
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

        public ResultsViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
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
                var magnitude = FftData;
                var maxMag = magnitude.Max();
                for (int i = 0; i < magnitude.Length; i++) //zamiana magnitude --> decibel
                {
                    magnitude[i] = (float)(20 * Math.Log10(magnitude[i]/maxMag));
                }
                var freq = Fourier.FrequencyScale(FftData.Length, 4000); //todo przenoszenie sampleRate, ogarniecie skali na x (screenshot YT)
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
                    IsZoomEnabled = true,
                    IsPanEnabled = true,
                    
                });
                model.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    TitleColor = OxyColors.Black,
                    AxislineColor = OxyColors.Black,
                    TicklineColor = OxyColors.Black,
                    IsZoomEnabled = true,
                    IsPanEnabled = true,
                    Minimum = -80
                });
                var series1 = new LineSeries
                {
                    Color = OxyColors.Red,
                    StrokeThickness = 0.5
                };
                for (int i = 0; i < magnitude.Length; i++)
                {
                    series1.Points.Add(new DataPoint(freq[i], magnitude[i]));
                }

                model.Series.Add(series1);
                PlotModel = model;
            }));
        }

        public override void Prepare(float[] parameter) => FftData = parameter;
    }
}
