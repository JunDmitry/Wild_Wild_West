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

INV-014: An Arena Run cannot start with a Player whose Health is depleted or uninitialized.

INV-015: An Arena Run starts with the Ranged weapon selected.

## Enemy

INV-020: Every active Enemy belongs to exactly one Arena Run.

INV-021: Enemy Health cannot exceed its maximum.

INV-022: Enemy Health cannot be lower than zero.

INV-023: A defeated Enemy is not part of the active Enemy collection.

INV-024: One Melee Attack can damage a specific Enemy at most once.

INV-025: An Enemy with a pending Attack does not receive a movement intent.

INV-026: An Enemy already within its attack range does not receive a movement intent.

INV-027: Enemy movement intents are created in ascending EnemyId order.

INV-028: Creating an Enemy Movement Batch Request does not change state, advance AggregateRevision, or produce Domain Events.

INV-029: An Enemy Movement Batch Resolution must contain exactly one entry for every Enemy Movement Intent in the corresponding request.

INV-030: A rejected Enemy Movement Batch Resolution does not move any Enemy, does not close the pending Interaction, does not advance AggregateRevision, and does not produce Domain Events.

INV-031: A valid Enemy Movement Batch Resolution that does not move any Enemy closes the pending Interaction without advancing AggregateRevision.

INV-032: A valid Enemy Movement Batch Resolution that moves at least one Enemy advances AggregateRevision exactly once and produces no Domain Events.

## Wave

INV-040: Exactly one Wave is current while an Arena Run is active.

INV-041: Wave numbers begin at one.

INV-042: A new Wave cannot begin before the current Wave is completed.

INV-043: A Boss cannot become active while Regular Enemies remain to be spawned or remain alive.

INV-044: At most one Boss may participate in a Wave.

INV-045: A Wave cannot complete before all required Regular Enemies and its Boss are defeated.

INV-046: Completion of a non-final Wave begins the next Wave.

INV-047: Completion of the final Wave results in Arena Run Victory.

INV-048: A Wave with zero Regular Enemies begins in Boss Combat.

INV-049: Defeating the Boss completes the current Wave.

INV-050: Completing a non-final Wave immediately starts the next Wave within the same aggregate operation.

INV-051: Completing the final Wave transitions the Arena Run to Victory within the same aggregate operation.

## Arena Run Lifecycle

INV-060: An Arena Run starts according to its initial lifecycle state and can enter Playing only through a valid lifecycle transition.

INV-061: Defeat and Victory are terminal domain states.

INV-062: Player defeat results in Arena Run Defeat.

INV-063: An Arena Run cannot be both defeated and victorious.

INV-064: Combat progression does not continue after Defeat or Victory.

INV-065: Restarting gameplay creates a new Arena Run rather than resetting the terminal Arena Run.

## Game Time

INV-075: An Arena Run holds a monotonically increasing current Game Time starting at zero.

INV-076: Advancing Game Time is a state-changing operation: it advances AggregateRevision exactly once, is rejected while an Interaction is pending or the Arena Run is terminal, and produces no Domain Events.

INV-077: Combat readiness is compared against the Arena Run's current Game Time, never against a caller-supplied time.

INV-078: A new Arena Run starts with all weapons ready.

## External Interactions

INV-080: The Domain does not allocate ArenaRunId, PlayerId, or EnemyId.

INV-081: The Domain does not determine Unity collision, navigation, or physical spawn placement results.

INV-082: Every external Interaction Request is identified by its ArenaRunId, InteractionId, and AggregateRevision.

INV-083: An Interaction Resolution is accepted only when ArenaRunId, InteractionId, AggregateRevision, interaction type, and expected interaction stage match the pending Interaction Request.

INV-084: An Interaction Resolution created for an older AggregateRevision cannot mutate a newer Arena Run revision.

INV-085: An accepted state-changing aggregate operation advances AggregateRevision exactly once.

INV-086: A read-only operation, a rejected operation, and creation of an Interaction Request without a state change do not advance AggregateRevision.

INV-087: A rejected or stale Interaction Resolution does not change aggregate state, advance AggregateRevision, or produce Domain Events.

INV-088: Applying the same Interaction Resolution more than once cannot duplicate a domain transition.

INV-089: InteractionId is allocated by ArenaRun from a monotonically increasing sequence local to that Arena Run.

INV-090: One Enemy Movement Batch is one Interaction and therefore has one InteractionId; individual batch entries are correlated by EnemyId.

INV-091: At most one Interaction is pending for an Arena Run at any moment.

INV-092: A pending Interaction is closed only by successful application of its resolution or by explicit cancellation through the Arena Run API.

INV-093: Resolutions are validated in a fixed order: foreign Arena Run, unknown Interaction, closed Interaction, stale revision, kind mismatch.

INV-094: A rejected Interaction Resolution, including a resolution with an invalid payload, does not close the pending Interaction.

INV-095: Cancelling a pending Interaction does not advance AggregateRevision and does not produce Domain Events.

INV-096: An accepted position for any Planar Movement Intent must lie on the ground plane and on the requested movement path: not behind the origin, not beyond the requested distance, and not laterally displaced beyond geometric tolerance. This rule is owned by the Movement Path Policy and applies to Player and Enemy movement alike.

INV-097: The external world may shorten a requested movement but cannot extend, redirect, or relocate it.

INV-098: While an Interaction is pending, the Arena Run rejects state-changing commands other than applying or cancelling that Interaction.

INV-099: Opening a second Interaction while one is pending is a programming error of the orchestrator and is signalled as an exception rather than a rejected command.

## Enemy Spawning

INV-110: Every Wave contains exactly one Boss. A Wave with zero Regular Enemies begins in Boss Combat.

INV-111: A Regular Enemy spawn is due only while the Wave is in Regular Combat and Regular Enemies remain to spawn.

INV-112: A Boss spawn is due only while the Wave is in Boss Combat and the Boss has not spawned.

INV-113: The Arena Run does not schedule spawns; it answers whether a spawn is due when asked.

INV-114: A spawned Enemy is placed on the ground height, outside the Arena Bounds, and within the Spawn Band.

INV-115: A Spawn Resolution with a None or duplicate EnemyId is rejected without side effects.

INV-116: A successful spawn advances AggregateRevision once and produces exactly one EnemySpawned Domain Event.

## Combat

INV-120: Damage is positive.

INV-121: Health cannot be reduced below zero.

INV-122: Attack Start does not deal Damage.

INV-123: An attacker can have at most one pending Attack.

INV-124: Cooldown is applied at Attack Start.

INV-125: Attack Impact is resolved against the current Arena Run state, not the state at Attack Start.

INV-126: The target hit by Attack Impact may differ from the target that allowed Attack Start.

INV-127: Switching the Player weapon is rejected while the Player has a pending Attack.

INV-128: A successfully started Player Attack advances AggregateRevision exactly once and produces one PlayerAttackStarted Domain Event.

INV-129: A rejected Attack Start does not change state, advance AggregateRevision, or produce Domain Events.

INV-130: An Attack Impact can be requested only when the Player has a pending Attack and the current Game Time has reached its Impact time.

INV-131: Attack Impact targets must be active Enemies of the same Arena Run within the weapon range extended by the target Collision Radius.

INV-132: A ranged Attack Impact affects at most one Enemy.

INV-133: Repeated Enemy identifiers in a melee Attack Impact are collapsed; each Enemy is damaged at most once per Attack.

INV-134: An accepted Attack Impact completes the pending Attack, advances AggregateRevision exactly once, and produces a PlayerAttackCompleted Domain Event regardless of hit or miss.

INV-135: A rejected Attack Impact leaves the pending Attack and the pending Interaction untouched.

INV-136: An Enemy whose Health is depleted is removed from the active Enemy collection within the same operation that applied the lethal Damage.

INV-137: An Enemy starts an Attack only while it is active, has no pending Attack, its attack readiness has been reached, and the Player is within its attack range extended by both Collision Radii.

INV-138: Starting Enemy Attacks is one atomic aggregate operation processed in ascending EnemyId order; it advances AggregateRevision exactly once regardless of how many Attacks started.

INV-139: If no Enemy is eligible, starting Enemy Attacks does not change state, advance AggregateRevision, or produce Domain Events.

INV-140: Applied Damage cannot exceed Requested Damage.

INV-141: Applied Damage equals the actual reduction of Health.

INV-142: Damage applied to depleted Health is rejected.

INV-143: A pending Attack of a defeated attacker is cancelled before the attacker is removed or the Arena Run becomes terminal.

INV-144: Attack cancellation produces a typed Domain Event and does not produce an AttackCompleted event.

INV-145: All events produced by one enemy impact batch share one resulting AggregateRevision.

INV-146: An Enemy Attack Impact is resolved only when its Impact time has been reached.

INV-147: An Enemy Attack Impact hits only when the Player is within the attacker's Enemy Attack Range at impact time.

INV-148: Resolving due Enemy Attack Impacts is one atomic aggregate operation processed in ascending EnemyId order that advances AggregateRevision exactly once when at least one Attack is resolved.

INV-149: Each Enemy Attack Impact hit produces one PlayerDamaged event followed by an EnemyAttackCompleted event.

INV-150: A missed Enemy Attack Impact completes the Attack and does not restore attack readiness.

## Domain Events

INV-160: Domain Events describe facts that have already occurred.

INV-161: Domain Events do not contain Unity-specific types.

INV-162: A Domain Event cannot mutate the Arena Run.

INV-163: Continuous visual synchronization such as movement rendering is not represented by mandatory per-frame Domain Events.

INV-164: All Domain Events produced by one atomic aggregate operation carry the same resulting AggregateRevision.

INV-165: PlayerAttackCompleted is produced after direct attack consequences such as damage, enemy defeat, wave completion, next wave start, or victory.