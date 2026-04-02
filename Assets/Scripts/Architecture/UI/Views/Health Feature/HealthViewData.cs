namespace Assets.Scripts.Architecture.UI.Views.HealthFeature
{
    public readonly struct HealthViewData
    {
        public HealthViewData(float current, float max)
        {
            Current = current;
            Max = max;
        }

        public float Current { get; }
        public float Max { get; }
    }
}