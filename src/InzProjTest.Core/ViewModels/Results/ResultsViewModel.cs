using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Accord.Audio;
using Accord.Math;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace InzProjTest.Core.ViewModels.Results
{
    public class ResultsViewModel : BaseViewModel<ComplexSignal>
    {
        private readonly IMvxNavigationService _navigationService;

        #region Properties

        public ComplexSignal FftData { get; set; }

        #endregion

        public ResultsViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        public PlotModel PlotModel => GeneratePlotModel();

        private PlotModel GeneratePlotModel()
        {
            var channel = FftData.GetChannel(0);
            var magnitude=  Accord.Audio.Tools.GetMagnitudeSpectrum(channel);
            var freq = Accord.Audio.Tools.GetFrequencyVector(channel.Length, FftData.SampleRate);
            magnitude[0] = 0; 
            float[] g = new float[magnitude.Length];
            for (int i = 0; i < magnitude.Length; i++)
                g[i] = (float)magnitude[i];

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
            for (int i = 0; i < g.Length; i++)
            {
                series1.Points.Add(new DataPoint(freq[i],g[i]));
            }

            model.Series.Add(series1);

            return model;
        }

        public override void Prepare(ComplexSignal parameter) => FftData = parameter;
    }
}
