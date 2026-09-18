# Arena Run Aggregate Invariants

## Identity

INV-001: An Arena Run has exactly one ArenaRunId for its entire lifetime.

INV-002: A Player has exactly one PlayerId for its entire lifetime.

INV-003: An EnemyId is unique among active Enemies of an Arena Run.

INV-004: ArenaRun, Player, and Enemy identities are supplied to the Domain. ArenaRun does not generate entity identities.

## Player

INV-010: An Arena Run contains exactly one Player.

INV-011: Player Health never exceeds its maximum.

INV-012: Player Health never becomes negative.

INV-013: A defeated Player cannot perform movement, weapon selection, attack start, or attack impact operations.

INV-014: An Arena Run cannot start with a Player whose Health is depleted or uninitialized.

INV-015: An Arena Run starts with the Ranged weapon selected.

## Enemy

INV-020: Every active Enemy belongs to exactly one Arena Run.

INV-021: Enemy Health never exceeds its maximum.

INV-022: Enemy Health never becomes negative.

INV-023: A defeated Enemy is removed from the active Enemy collection.

INV-024: One Melee Attack damages a specific Enemy at most once.

INV-025: An Enemy with a pending Attack does not receive a movement intent.

INV-026: An Enemy already within attack range does not receive a movement intent.

INV-027: Enemy batch operations are processed in ascending EnemyId order.

INV-028: Creating an Enemy Movement Batch Request does not change state, advance AggregateRevision, or produce Domain Events.

INV-029: An Enemy Movement Batch Resolution contains exactly one entry for every Enemy Movement Intent in the corresponding request.

INV-030: A rejected Enemy Movement Batch Resolution does not move any Enemy, does not close the pending Interaction, does not advance AggregateRevision, and does not produce Domain Events.

INV-031: A valid Enemy Movement Batch Resolution that does not move any Enemy closes the pending Interaction without advancing AggregateRevision.

INV-032: A valid Enemy Movement Batch Resolution that moves at least one Enemy advances AggregateRevision exactly once and produces no Domain Events.

## Wave

INV-040: Exactly one Wave is current while an Arena Run is active.

INV-041: Wave numbers begin at one.

INV-042: A new Wave cannot begin before the current Wave is completed.

INV-043: A Boss cannot become active while Regular Enemies remain to be spawned or alive.

INV-044: At most one Boss participates in a Wave.

INV-045: A Wave cannot complete before all required Regular Enemies and its Boss are defeated.

INV-046: Completing a non-final Wave starts the next Wave.

INV-047: Completing the final Wave results in Arena Run Victory.

INV-048: A Wave with zero Regular Enemies begins in Boss Combat.

INV-049: Defeating the Boss completes the current Wave.

INV-050: Completing a non-final Wave starts the next Wave within the same aggregate operation.

INV-051: Completing the final Wave transitions ArenaRun to Victory within the same aggregate operation.

## Arena Run Lifecycle

INV-060: ArenaRun starts in Playing.

INV-061: Defeat and Victory are terminal ArenaRun states.

INV-062: Player defeat transitions ArenaRun to Defeat.

INV-063: ArenaRun cannot be both defeated and victorious.

INV-064: Combat progression does not continue after Defeat or Victory.

INV-065: Restarting gameplay creates a new ArenaRun instead of resetting a terminal ArenaRun.

## Game Time

INV-070: ArenaRun holds monotonically increasing Game Time starting at zero.

INV-071: Advancing Game Time advances AggregateRevision exactly once, is rejected while an Interaction is pending or ArenaRun is terminal, and produces no Domain Events.

INV-072: Combat readiness is compared only against ArenaRun current Game Time.

INV-073: A new ArenaRun starts with all weapons and enemies ready to start their first attacks.

## External Interactions

INV-080: The Domain does not allocate ArenaRunId, PlayerId, or EnemyId.

INV-081: The Domain does not determine Unity collision, navigation, physical spawn placement, raycast, or overlap results.

INV-082: Every external Interaction Request contains ArenaRunId, InteractionId, and AggregateRevision.

INV-083: A resolution is accepted only when ArenaRunId, InteractionId, AggregateRevision, interaction kind, and expected interaction stage match the pending request.

INV-084: A resolution created for an older AggregateRevision cannot mutate a newer ArenaRun revision.

INV-085: Every accepted state-changing aggregate operation advances AggregateRevision exactly once.

INV-086: Read-only operations, rejected operations, and request creation without state change do not advance AggregateRevision.

INV-087: Rejected, stale, mismatched, and duplicate resolutions do not change aggregate state, advance AggregateRevision, or produce Domain Events.

INV-088: Applying the same Interaction Resolution more than once cannot duplicate a transition.

INV-089: InteractionId is allocated by ArenaRun from a monotonically increasing sequence local to that ArenaRun.

INV-090: One Enemy Movement Batch is one Interaction. Individual batch entries are correlated by EnemyId.

INV-091: At most one Interaction is pending for an ArenaRun at one time.

INV-092: A pending Interaction closes only by successful resolution application or explicit cancellation through ArenaRun.

INV-093: Resolution validation order is foreign run, unknown interaction, closed interaction, stale revision, and kind mismatch.

INV-094: A rejected resolution, including a resolution with invalid payload, does not close the pending Interaction.

INV-095: Cancelling a pending Interaction does not advance AggregateRevision and does not produce Domain Events.

INV-096: A valid accepted position lies on the requested planar movement path, remains on the ground plane, is not behind the source position, does not exceed requested distance, and does not exceed lateral tolerance.

INV-097: The external world may shorten movement but cannot extend, redirect, or relocate it.

INV-098: While an Interaction is pending, ArenaRun rejects state-changing commands other than applying or cancelling that Interaction.

INV-099: Opening a second Interaction while one is pending is an orchestrator programming error.

## Enemy Spawning

INV-110: Every Wave contains exactly one Boss.

INV-111: A Regular Enemy spawn is due only during Regular Combat while Regular Enemies remain to spawn.

INV-112: A Boss spawn is due only during Boss Combat while the Boss has not spawned.

INV-113: ArenaRun does not schedule spawn cadence. It only determines whether a spawn is due.

INV-114: A spawned Enemy is on the Arena ground height, outside Arena Bounds, and within Spawn Band.

INV-115: A Spawn Resolution with EnemyId.None or a duplicate EnemyId is rejected without side effects.

INV-116: A successful spawn advances AggregateRevision once and produces exactly one EnemySpawned Domain Event.

## Combat

INV-120: Damage is positive.

INV-121: Health never becomes negative.

INV-122: Attack Start does not apply Damage.

INV-123: A combatant has at most one pending Attack.

INV-124: Cooldown starts at Attack Start.

INV-125: Attack Impact resolves against current ArenaRun state rather than state at Attack Start.

INV-126: The target hit by Attack Impact may differ from any target that allowed Attack Start.

INV-127: Switching Player weapon is rejected while Player has a pending Attack.

INV-128: A successfully started Player Attack advances AggregateRevision once and produces PlayerAttackStarted.

INV-129: A rejected Attack Start does not change state, advance AggregateRevision, or produce Domain Events.

INV-130: Player Attack Impact can be requested only when Player has a pending Attack and Game Time reached ImpactAt.

INV-131: Player Attack Impact targets are active Enemies in the same ArenaRun and within valid weapon range.

INV-132: A Ranged Attack Impact affects at most one Enemy.

INV-133: Repeated EnemyId values in a Melee Attack Impact are collapsed.

INV-134: An accepted Player Attack Impact completes the pending attack, advances AggregateRevision once, and produces PlayerAttackCompleted for both hit and miss.

INV-135: A rejected Player Attack Impact leaves the pending Attack and pending Interaction untouched.

INV-136: A depleted Enemy is removed within the same operation that applied lethal Damage.

INV-137: Enemy Attack Start requires an active Enemy, no pending attack, readiness reached, and Player within enemy attack range.

INV-138: Starting Enemy Attacks is one atomic operation processed in ascending EnemyId order and advances AggregateRevision once when at least one attack starts.

INV-139: Starting Enemy Attacks with no eligible Enemy does not change state, advance AggregateRevision, or produce Domain Events.

INV-140: Applied Damage never exceeds requested Damage.

INV-141: Applied Damage equals actual Health reduction.

INV-142: Applying Damage to depleted Health is rejected.

INV-143: A pending Attack of a defeated attacker is cancelled before removing the attacker or terminating ArenaRun.

INV-144: Attack cancellation produces a typed Domain Event and does not produce AttackCompleted.

INV-145: All Domain Events from one Enemy Attack Impact batch share one resulting AggregateRevision.

INV-146: Enemy Attack Impact resolves only after ImpactAt.

INV-147: Enemy Attack Impact hits only when Player is within enemy attack range at impact time.

INV-148: Resolving due Enemy Attack Impacts is one atomic operation processed in ascending EnemyId order and advances AggregateRevision once when at least one attack resolves.

INV-149: Each Enemy Attack Impact hit produces PlayerDamaged before EnemyAttackCompleted.

INV-150: A missed Enemy Attack Impact completes the attack and does not restore readiness.

INV-151: Player Health reaching zero transitions ArenaRun to Defeat in the same aggregate operation.

INV-152: After lethal Enemy Attack Impact, remaining pending attacks do not deal Damage and are cancelled.

INV-153: Defeat cancels Player pending Attack with AttackerDefeated and Enemy pending Attacks with ArenaRunTerminated.

INV-154: ArenaRunDefeated is the final Domain Event of the defeating operation.

INV-155: A defeated Enemy pending Attack is cancelled with AttackerDefeated before Enemy removal.

## Domain Events

INV-160: Domain Events describe facts that already occurred.

INV-161: Domain Events contain no Unity-specific types.

INV-162: Domain Events cannot mutate ArenaRun.

INV-163: Continuous synchronization such as rendered movement is not represented by mandatory per-frame Domain Events.

INV-164: All Domain Events created by one atomic aggregate operation carry the same resulting AggregateRevision.

INV-165: PlayerAttackCompleted is emitted after direct player attack consequences, including damage, enemy defeat, wave progression, and victory.