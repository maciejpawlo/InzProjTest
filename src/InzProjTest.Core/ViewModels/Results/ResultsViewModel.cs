using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace InzProjTest.Core.ViewModels.Results
{
    public class ResultsViewModel : BaseViewModel<Complex32[]>
    {
        private readonly IMvxNavigationService _navigationService;

        #region Properties

        public Complex32[] FftData { get; set; }

        #endregion

        public ResultsViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        public PlotModel PlotModel => GeneratePlotModel();

        private PlotModel GeneratePlotModel() //todo asynchroniczna zeby nie blokować UI thread
        {
            float[] magnitude = new float[FftData.Length];
            for (int i = 0; i < FftData.Length; i++)
            {
                magnitude[i] = FftData[i].MagnitudeSquared;
            }
            var freq = Fourier.FrequencyScale(FftData.Length, 44100); //todo przenoszenie sampleRate, ogarniecie skali na x (screenshot YT)
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
                MarkerStroke = OxyColors.Red,
                Color = OxyColors.Red
            };
            for (int i = 0; i < magnitude.Length; i++)
            {
                series1.Points.Add(new DataPoint(freq[i],magnitude[i]));
            }
            model.Series.Add(series1);
            return model;
        }

        public override void Prepare(Complex32[] parameter) => FftData = parameter;
    }
}
