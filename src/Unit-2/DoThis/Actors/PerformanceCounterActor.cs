using System;
using Akka.Actor;
using System.Diagnostics;
using System.Collections.Generic;

namespace ChartApp.Actors
{
    public class PerformanceCounterActor : UntypedActor
    {
        private readonly Cancelable cancelPublishing;
        private readonly Func<PerformanceCounter> performanceCounterGenerator;
        private readonly string seriesName;
        private readonly HashSet<IActorRef> subscriptions;
        private PerformanceCounter counter;

        public PerformanceCounterActor(string seriesName, Func<PerformanceCounter> performanceCounterGenerator)
        {
            this.seriesName = seriesName;
            this.performanceCounterGenerator = performanceCounterGenerator;

            subscriptions = new HashSet<IActorRef>();

            cancelPublishing = new Cancelable(Context.System.Scheduler);
        }

        protected override void PreStart()
        {
            counter = performanceCounterGenerator();

            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.FromMilliseconds(250),
                TimeSpan.FromMilliseconds(250),
                Self,
                new GatherMetrics(),
                Self,
                cancelPublishing);
        }

        protected override void PostStop()
        {
            try
            {
                cancelPublishing.Cancel(throwOnFirstException: false);
                counter.Dispose();
            }
            catch
            {
                // don't care about exceptions
            }
            finally
            {
                base.PostStop();
            }
        }

        protected override void OnReceive(object message)
        {
            if (message is GatherMetrics)
            {
                var metric = new Metric(seriesName, counter.NextValue());

                foreach (var sub in subscriptions)
                {
                    sub.Tell(metric);
                }
            }
            else if (message is SubscribeCounter)
            {
                var sc = message as SubscribeCounter;
                subscriptions.Add(sc.Subscriber);
            }
            else if (message is UnsubscribeCounter)
            {
                var uc = message as UnsubscribeCounter;
                subscriptions.Remove(uc.Subscriber);
            }
        }
    }
}
