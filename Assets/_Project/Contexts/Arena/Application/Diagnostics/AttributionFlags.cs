using System;
using System.Collections.Generic;

namespace Game.Arena.Application.Diagnostics
{
    /// <summary>
    /// TEMPORARY attribution instrumentation for coder task t9-gc-residual-b.
    /// Reads the player's command-line switches once and exposes them to the
    /// tick and dispatch paths so a single measurement build can be run as
    /// several A/B variants. Every flag is inert when absent. This file and
    /// every call site that reads it are deleted before the acceptance runs;
    /// the committed tree must not contain attribution switches.
    /// </summary>
    public static class AttributionFlags
    {
        private const string NotTickFlag = "-perfnotick";
        private const string SkipDispatchFlag = "-perfskipdispatch";
        private const string SkipDispatchStageFlag = "-perfskipstage";
        private const string SkipTickStageFlag = "-perfskiptickstage";
        private const string SkipHandlerFlag = "-perfskiphandler";
        private const string WaveEnemiesFlag = "-perfwaveenemies";

        private static readonly string[] Arguments = Environment.GetCommandLineArgs();

        private static readonly HashSet<string> DispatchStages =
            CollectValues(SkipDispatchStageFlag);

        private static readonly HashSet<string> TickStages =
            CollectValues(SkipTickStageFlag);

        private static readonly HashSet<string> Handlers =
            CollectValues(SkipHandlerFlag);

        public static bool NotTick => HasFlag(NotTickFlag);

        public static bool SkipDispatch => HasFlag(SkipDispatchFlag);

        public static int? WaveEnemies => ReadInt(WaveEnemiesFlag);

        public static bool SkipsDispatchStage(string stageName)
        {
            return DispatchStages.Contains(stageName);
        }

        public static bool SkipsTickStage(string stageName)
        {
            return TickStages.Contains(stageName);
        }

        public static bool SkipsHandler(string handlerTypeName)
        {
            return Handlers.Contains(handlerTypeName);
        }

        private static HashSet<string> CollectValues(string flag)
        {
            HashSet<string> values = new(StringComparer.OrdinalIgnoreCase);
            string prefix = flag + "=";

            for (int index = 0; index < Arguments.Length; index++)
            {
                string argument = Arguments[index];

                if (string.Equals(argument, flag, StringComparison.OrdinalIgnoreCase)
                    && index < Arguments.Length - 1)
                {
                    values.Add(Arguments[index + 1]);
                }
                else if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    values.Add(argument.Substring(prefix.Length));
                }
            }

            return values;
        }

        private static bool HasFlag(string flag)
        {
            string prefix = flag + "=";

            for (int index = 0; index < Arguments.Length; index++)
            {
                if (string.Equals(Arguments[index], flag, StringComparison.OrdinalIgnoreCase)
                    || Arguments[index].StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static int? ReadInt(string flag)
        {
            string prefix = flag + "=";

            for (int index = 0; index < Arguments.Length; index++)
            {
                string argument = Arguments[index];

                if (string.Equals(argument, flag, StringComparison.OrdinalIgnoreCase)
                    && index < Arguments.Length - 1
                    && int.TryParse(Arguments[index + 1], out int separateValue))
                {
                    return separateValue;
                }

                if (argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(argument.Substring(prefix.Length), out int equalsValue))
                {
                    return equalsValue;
                }
            }

            return null;
        }
    }
}
