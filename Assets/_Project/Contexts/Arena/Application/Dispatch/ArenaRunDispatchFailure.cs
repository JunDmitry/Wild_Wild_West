using System;

namespace Game.Arena.Application.Dispatch
{
    public sealed class ArenaRunDispatchFailure
    {
        public ArenaRunDispatchFailure(
            ArenaRunDispatchStage stage,
            string handlerName,
            Type payloadType,
            Exception exception)
        {
            if (Enum.IsDefined(typeof(ArenaRunDispatchStage), stage) == false)
            {
                throw new ArgumentException(
                    "Unknown dispatch stage.",
                    nameof(stage));
            }

            Stage = stage;
            HandlerName = handlerName ?? throw new ArgumentNullException(nameof(handlerName));
            PayloadType = payloadType ?? throw new ArgumentNullException(nameof(payloadType));
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        public ArenaRunDispatchStage Stage { get; }

        public string HandlerName { get; }

        public Type PayloadType { get; }

        public Exception Exception { get; }
    }
}
