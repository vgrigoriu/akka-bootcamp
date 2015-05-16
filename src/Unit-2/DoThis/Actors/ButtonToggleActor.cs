using Akka.Actor;
using System.Windows.Forms;

namespace ChartApp.Actors
{
    public class ButtonToggleActor : UntypedActor
    {
        public class Toggle { }

        private readonly IActorRef coordinatorActor;
        private readonly Button myButton;
        private readonly CounterType myCounterType;
        private bool isToggledOn;


        public ButtonToggleActor(
            IActorRef coordinatorActor,
            Button myButton,
            CounterType myCounterType,
            bool isToggledOn = false)
        {
            this.coordinatorActor = coordinatorActor;
            this.myButton = myButton;
            this.myCounterType = myCounterType;
            this.isToggledOn = isToggledOn;
        }

        protected override void OnReceive(object message)
        {
            if (message is Toggle && isToggledOn)
            {
                coordinatorActor.Tell(
                    new PerformanceCounterCoordinatorActor.Unwatch(myCounterType));
                FlipToggle();
            }
            else if (message is Toggle && !isToggledOn)
            {
                coordinatorActor.Tell(
                    new PerformanceCounterCoordinatorActor.Watch(myCounterType));
                FlipToggle();
            }
            else
            {
                Unhandled(message);
            }
        }

        private void FlipToggle()
        {
            isToggledOn = !isToggledOn;
            myButton.Text = string.Format(
                "{0} ({1})",
                myCounterType.ToString(),
                isToggledOn ? "ON" : "OFF");
        }
    }
}
