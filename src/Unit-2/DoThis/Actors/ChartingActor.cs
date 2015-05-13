using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using Akka.Actor;

namespace ChartApp.Actors
{
    public class ChartingActor : ReceiveActor
    {
        #region Messages

        public class InitializeChart
        {
            public InitializeChart(Dictionary<string, Series> initialSeries)
            {
                InitialSeries = initialSeries;
            }

            public Dictionary<string, Series> InitialSeries { get; private set; }
        }

        public class AddSeries
        {
            private readonly Series series;

            public AddSeries(Series series)
            {
                this.series = series;
            }

            public Series Series { get { return series; } }
        }

        public class RemoveSeries
        {
            private readonly string seriesName;

            public RemoveSeries(string seriesName)
            {
                this.seriesName = seriesName;
            }

            public string SeriesName { get { return seriesName; } }
        }
        #endregion

        private readonly Chart _chart;
        private Dictionary<string, Series> _seriesIndex;

        public ChartingActor(Chart chart) : this(chart, new Dictionary<string, Series>())
        {
        }

        public ChartingActor(Chart chart, Dictionary<string, Series> seriesIndex)
        {
            _chart = chart;
            _seriesIndex = seriesIndex;

            Receive<InitializeChart>(ic => HandleInitialize(ic));
            Receive<AddSeries>(addSeries => HandleAddSeries(addSeries));
        }

        #region Individual Message Type Handlers

        private void HandleInitialize(InitializeChart ic)
        {
            if (ic.InitialSeries != null)
            {
                //swap the two series out
                _seriesIndex = ic.InitialSeries;
            }

            //delete any existing series
            _chart.Series.Clear();

            //attempt to render the initial chart
            if (_seriesIndex.Any())
            {
                foreach (var series in _seriesIndex)
                {
                    //force both the chart and the internal index to use the same names
                    series.Value.Name = series.Key;
                    _chart.Series.Add(series.Value);
                }
            }
        }

        private void HandleAddSeries(AddSeries addSeries)
        {
            var series = addSeries.Series;
            if (!string.IsNullOrEmpty(series.Name) && !_seriesIndex.ContainsKey(series.Name))
            {
                _seriesIndex.Add(series.Name, series);
                _chart.Series.Add(series);
            }
        }

        #endregion
    }
}
