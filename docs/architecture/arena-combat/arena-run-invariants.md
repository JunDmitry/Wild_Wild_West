# Arena Run Aggregate Invariants

## Identity

INV-001: An Arena Run has exactly one ArenaRunId for its entire lifetime.

INV-002: A Player has exactly one PlayerId for its entire lifetime.

INV-003: An EnemyId is unique among all Enemies belonging to an Arena Run.

INV-004: Domain identities are supplied to the Domain. The Arena Run does not generate entity identities.

## Player

INV-010: An Arena Run contains exactly one Player.

INV-011: Player Health cannot exceed its maximum.

INV-012: Player Health cannot be lower than zero.

INV-013: A defeated Player cannot perform movement, weapon selection, or attack operations.

## Enemy

INV-020: Every active Enemy belongs to exactly one Arena Run.

INV-021: Enemy Health cannot exceed its maximum.

INV-022: Enemy Health cannot be lower than zero.

INV-023: A defeated Enemy is not part of the active Enemy collection.

INV-024: One Melee Attack can damage a specific Enemy at most once.

## Wave

INV-030: Exactly one Wave is current while an Arena Run is active.

INV-031: Wave numbers begin at one.

INV-032: A new Wave cannot begin before the current Wave is completed.

INV-033: A Boss cannot become active while Regular Enemies remain to be spawned or remain alive.

INV-034: At most one Boss may participate in a Wave.

INV-035: A Wave cannot complete before all required Regular Enemies and its Boss are defeated.

INV-036: Completion of a non-final Wave begins the next Wave.

INV-037: Completion of the final Wave results in Arena Run Victory.

## Arena Run Lifecycle

INV-040: An Arena Run starts according to its initial lifecycle state and can enter Playing only through a valid lifecycle transition.

INV-041: Defeat and Victory are terminal domain states.

INV-042: Player defeat results in Arena Run Defeat.

INV-043: An Arena Run cannot be both defeated and victorious.

INV-044: Combat progression does not continue after Defeat or Victory.

INV-045: Restarting gameplay creates a new Arena Run rather than resetting the terminal Arena Run.

## Combat

INV-050: Damage is non-negative.

INV-051: An Attack cannot execute before its combat readiness time.

INV-052: A successfully executed Attack advances the corresponding combat readiness time.

INV-053: Switching the selected Weapon takes effect before an Attack requested during the same gameplay step.

INV-054: An Attack affects only targets belonging to the same Arena Run.

## External Interactions

INV-060: The Domain does not allocate ArenaRunId, PlayerId, or EnemyId.

INV-061: The Domain does not determine Unity collision, navigation, or physical spawn placement results.

INV-062: Every external Interaction Request is identified by its ArenaRunId, InteractionId, and AggregateRevision.

INV-063: An Interaction Resolution is accepted only when ArenaRunId, InteractionId, AggregateRevision, interaction type, and expected interaction stage match the pending Interaction Request.

INV-064: An Interaction Resolution created for an older AggregateRevision cannot mutate a newer Arena Run revision.

INV-065: An accepted state-changing aggregate operation advances AggregateRevision exactly once.

INV-066: A read-only operation, a rejected operation, and creation of an Interaction Request without a state change do not advance AggregateRevision.

INV-067: A rejected or stale Interaction Resolution does not change aggregate state, advance AggregateRevision, or produce Domain Events.

INV-068: Applying the same Interaction Resolution more than once cannot duplicate a domain transition.

INV-069: InteractionId is allocated by ArenaRun from a monotonically increasing sequence local to that Arena Run.

INV-070: One Enemy Movement Batch is one Interaction and therefore has one InteractionId; individual batch entries are correlated by EnemyId.

## Domain Events

INV-080: Domain Events describe facts that have already occurred.

INV-081: Domain Events do not contain Unity-specific types.

INV-082: A Domain Event cannot mutate the Arena Run.

INV-083: Continuous visual synchronization such as movement rendering is not represented by mandatory per-frame Domain Events.