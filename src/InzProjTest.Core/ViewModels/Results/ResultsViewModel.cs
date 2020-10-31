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

namespace InzProjTest.Core.ViewModels.Results
{
    public class ResultsViewModel : BaseViewModel<Complex32[]>
    {
        private readonly IMvxNavigationService _navigationService;

        #region Properties

        public Complex32[] FftData { get; set; }
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
                float[] magnitude = FftData.Select(x => x.Magnitude).ToArray();
                var freq = Fourier.FrequencyScale(FftData.Length, 4000); //todo przenoszenie sampleRate, ogarniecie skali na x (screenshot YT)
                var hzPerSample = 4000 / FftData.Length; // SampleRate/SamplesNumber  
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
                    Minimum = -1000,
                    Maximum = 1000,
                });
                model.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    TitleColor = OxyColors.Black,
                    AxislineColor = OxyColors.Black,
                    TicklineColor = OxyColors.Black,
                });
                var series1 = new LineSeries
                {
                    Color = OxyColors.Red,
                };


                for (int i = 0; i < magnitude.Length; i++)
                {
                    series1.Points.Add(new DataPoint(freq[i], magnitude[i]));
                }

                model.Series.Add(series1);
                PlotModel = model;
            }));
        }

        public override void Prepare(Complex32[] parameter) => FftData = parameter;
    }
}
