using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartApp.Actors
{
    public class PerformanceCounterCoordinatorActor : ReceiveActor
    {
        public class Watch
        {
            private readonly CounterType counter;

            public Watch(CounterType counter)
            {
                this.counter = counter;
            }

            public CounterType Counter
            {
                get
                {
                    return counter;
                }
            }
        }

        public class Unwatch
        {
            private readonly CounterType counter;

            public Unwatch(CounterType counter)
            {
                this.counter = counter;
            }

            public CounterType Counter
            {
                get
                {
                    return counter;
                }
            }
        }

        private static readonly Dictionary<CounterType, Func<PerformanceCounter>> CounterGenerators = new Dictionary<CounterType, Func<PerformanceCounter>>
        {
            {CounterType.Cpu, () => new PerformanceCounter("Processor", "% Processor Time", "_Total", true) },
            {CounterType.Memory, () => new PerformanceCounter("Memory", "% Commited Bytes In Use", true) },
            {CounterType.Disk, () => new PerformanceCounter("LogicalDisk", "% Disk Time", "_Total", true) }
        };

        private static readonly Dictionary<CounterType, Func<Series>> CounterSeries = new Dictionary<CounterType, Func<Series>>
        {
            {
                CounterType.Cpu,
                () => new Series(CounterType.Cpu.ToString())
                {
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.DarkGreen
                }
            },
            {
                CounterType.Memory,
                () => new Series(CounterType.Memory.ToString())
                {
                    ChartType = SeriesChartType.FastLine,
                    Color = Color.MediumBlue
                }
            },
            {
                CounterType.Disk,
                () => new Series(CounterType.Disk.ToString())
                {
                    ChartType = SeriesChartType.SplineArea,
                    Color = Color.DarkRed
                }
            }
        };

        private readonly Dictionary<CounterType, IActorRef> counterActors;

        private readonly IActorRef chartingActor;

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor)
            : this(chartingActor, new Dictionary<CounterType, IActorRef>())
        {

        }

        public PerformanceCounterCoordinatorActor(IActorRef chartingActor, Dictionary<CounterType, IActorRef> counterActors)
        {
            this.chartingActor = chartingActor;
            this.counterActors = counterActors;

            Receive<Watch>(watch =>
            {
                var counter = watch.Counter;
                if (counterActors.ContainsKey(counter))
                {
                    var counterActor = Context.ActorOf(Props.Create(() =>
                        new PerformanceCounterActor(counter.ToString(), CounterGenerators[counter])));

                    counterActors[counter] = counterActor;
                }

                chartingActor.Tell(new ChartingActor.AddSeries(CounterSeries[counter]()));

                counterActors[counter].Tell(new SubscribeCounter(counter, chartingActor));
            });

            Receive<Unwatch>(unwatch =>
            {
                var counter = unwatch.Counter;
                if (!counterActors.ContainsKey(counter))
                {
                    return;
                }

                counterActors[counter].Tell(new UnsubscribeCounter(counter, chartingActor));

                chartingActor.Tell(new ChartingActor.RemoveSeries(counter.ToString()));
            });
        }
    }
}
