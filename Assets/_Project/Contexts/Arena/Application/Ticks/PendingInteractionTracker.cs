using Game.Arena.Domain.Interactions;

namespace Game.Arena.Application.Ticks
{
    internal sealed class PendingInteractionTracker
    {
        private bool _hasCorrelation;
        private InteractionCorrelation _correlation;

        public bool HasCorrelation => _hasCorrelation;

        public InteractionCorrelation Correlation
        {
            get
            {
                if (_hasCorrelation == false)
                {
                    throw new System.InvalidOperationException("No interaction correlation is tracked.");
                }

                return _correlation;
            }
        }

        public void Track(InteractionCorrelation correlation)
        {
            _hasCorrelation = true;
            _correlation = correlation;
        }

        public void Clear()
        {
            _hasCorrelation = false;
            _correlation = default;
        }
    }
}
