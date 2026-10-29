using System;
using System.Collections.Generic;

namespace Game.Arena.Application.Diagnostics
{
    /// <summary>
    /// TEMPORARY attribution instrumentation for coder task t9-gc-residual-b.
    /// Parses the player's command-line switches exactly once at type
    /// initialisation and exposes them to the tick and dispatch paths so a
    /// single measurement build can be run as several A/B variants. Every
    /// flag is inert when absent, and every accessor is allocation-free after
    /// initialisation. This file and every call site that reads it are
    /// deleted before the acceptance runs; the committed tree must not
    /// contain attribution switches.
    /// </summary>
    public static class AttributionFlags
    {
        private const string NotTickFlag = "-perfnotick";
        private const string SkipDispatchFlag = "-perfskipdispatch";
        private const string SkipDispatchStageFlag = "-perfskipstage";
        private const string SkipTickStageFlag = "-perfskiptickstage";
        private const string SkipHandlerFlag = "-perfskiphandler";
        private const string SkipPhysicsFlag = "-perfskipphysics";
        private const string SkipResolveFlag = "-perfskipresolve";
        private const string SkipTickStagesFlag = "-perfskipstages";
        private const string SkipBuildResultFlag = "-perfskipbuild";
        private const string WaveEnemiesFlag = "-perfwaveenemies";

        private static readonly HashSet<string> Tokens =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, HashSet<string>> Values =
            new(StringComparer.OrdinalIgnoreCase);

        static AttributionFlags()
        {
            string[] arguments = Environment.GetCommandLineArgs();

            for (int index = 0; index < arguments.Length; index++)
            {
                string argument = arguments[index];

                if (string.IsNullOrEmpty(argument))
                {
                    continue;
                }

                int separator = argument.IndexOf('=');

                if (separator > 0 && argument[0] == '-')
                {
                    AddValue(argument.Substring(0, separator), argument.Substring(separator + 1));
                    continue;
                }

                Tokens.Add(argument);

                if (argument[0] == '-' && index < arguments.Length - 1)
                {
                    string next = arguments[index + 1];

                    if (next.Length > 0 && next[0] != '-')
                    {
                        AddValue(argument, next);
                    }
                }
            }
        }

        public static bool NotTick => Has(NotTickFlag);

        public static bool SkipDispatch => Has(SkipDispatchFlag);

        public static bool SkipPhysicsQueries => Has(SkipPhysicsFlag);

        public static bool SkipMovementResolve => Has(SkipResolveFlag);

        public static bool SkipTickStages => Has(SkipTickStagesFlag);

        public static bool SkipBuildResult => Has(SkipBuildResultFlag);

        public static int? WaveEnemies => ReadInt(WaveEnemiesFlag);

        public static bool SkipsDispatchStage(string stageName)
        {
            return HasValue(SkipDispatchStageFlag, stageName);
        }

        public static bool SkipsTickStage(string stageName)
        {
            return HasValue(SkipTickStageFlag, stageName);
        }

        public static bool SkipsHandler(string handlerTypeName)
        {
            return HasValue(SkipHandlerFlag, handlerTypeName);
        }

        private static void AddValue(string flag, string value)
        {
            Tokens.Add(flag);

            if (Values.TryGetValue(flag, out HashSet<string> values) == false)
            {
                values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                Values.Add(flag, values);
            }

            values.Add(value);
        }

        private static bool Has(string flag)
        {
            return Tokens.Contains(flag);
        }

        private static bool HasValue(string flag, string value)
        {
            return Values.TryGetValue(flag, out HashSet<string> values)
                && values.Contains(value);
        }

        private static int? ReadInt(string flag)
        {
            if (Values.TryGetValue(flag, out HashSet<string> values) == false)
            {
                return null;
            }

            foreach (string value in values)
            {
                if (int.TryParse(value, out int parsed))
                {
                    return parsed;
                }
            }

            return null;
        }
    }
}
