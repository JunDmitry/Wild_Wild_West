namespace Game.Arena.Application.Ticks
{
    /// <summary>
    /// Debug-only guard for the two-tick lifetime of published tick values
    /// (ADR-0035 requirement 12). In release builds every method is a no-op
    /// and no generation state exists.
    /// </summary>
    internal static class ArenaTickLifetimeGuard
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static int s_current;

        public static int Advance()
        {
            return ++s_current;
        }

        public static void Validate(int generation)
        {
            if (s_current - generation > 1)
            {
                throw new System.InvalidOperationException(
                    "An Arena tick value was used outside its two-tick lifetime window.");
            }
        }
#else
        public static int Advance()
        {
            return 0;
        }

        public static void Validate(int generation)
        {
        }
#endif
    }
}
