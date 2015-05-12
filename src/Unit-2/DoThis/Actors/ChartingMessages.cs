using Akka.Actor;

namespace ChartApp.Actors
{
    public class GatherMetrics { }

    public class Metric
    {
        private readonly float counterValue;

        private readonly string series;

        public Metric(string series, float counterValue)
        {
            this.series = series;
            this.counterValue = counterValue;
        }

        public float CounterValue { get { return counterValue; } }

        public string Series { get { return series; } }
    }

    public enum CounterType
    {
        Cpu,
        Memory,
        Disk
    }

    public class SubscribeCounter
    {
        private readonly CounterType counter;

        private readonly IActorRef subscriber;

        public SubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            this.counter = counter;
            this.subscriber = subscriber;
        }

        public CounterType Counter { get { return counter; } }

        public IActorRef Subscriber { get { return subscriber; } }
    }

    public class UnsubscribeCounter
    {
        private readonly CounterType counter;

        private readonly IActorRef subscriber;

        public UnsubscribeCounter(CounterType counter, IActorRef subscriber)
        {
            this.counter = counter;
            this.subscriber = subscriber;
        }

        public CounterType Counter { get { return counter; } }

        public IActorRef Subscriber { get { return subscriber; } }
    }
}
