using System;

namespace Game.Arena.Domain.Interactions
{
    public readonly struct InteractionAdmission : IEquatable<InteractionAdmission>
    {
        private InteractionAdmission(InteractionRejectionReason reason)
        {
            RejectionReason = reason;
        }

        public static InteractionAdmission Admitted { get; } = new InteractionAdmission(InteractionRejectionReason.None);

        public InteractionRejectionReason RejectionReason { get; }

        public bool IsAdmitted => RejectionReason == InteractionRejectionReason.None;

        public static InteractionAdmission Rejected(InteractionRejectionReason reason)
        {
            if (reason == InteractionRejectionReason.None)
            {
                throw new ArgumentOutOfRangeException(nameof(reason));
            }

            return new InteractionAdmission(reason);
        }

        public bool Equals(InteractionAdmission other)
        {
            return RejectionReason == other.RejectionReason;
        }

        public override bool Equals(object obj)
        {
            return obj is InteractionAdmission other && Equals(other);
        }

        public override int GetHashCode()
        {
            return RejectionReason.GetHashCode();
        }

        public static bool operator ==(InteractionAdmission left, InteractionAdmission right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InteractionAdmission left, InteractionAdmission right)
        {
            return !left.Equals(right);
        }
    }
}
