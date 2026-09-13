# Content

| ID |	Компонент |	Проблема |	Статус |
|---|---|---|---|
| LEG-001 |	Simulation.Execute |	Результат WeaponRules.TrySwitch игнорируется: дальнейшие действия используют state.Player, а не switchOutcome.Player. |	Open |
| LEG-002 |	SimulationPlanner + WeaponRules |	При Q + LMB planner создаёт melee query, но execute использует прежнее дальнее оружие. |	Open |
| LEG-003 |	WaveRules.TickRegular |	enemySpawned инициализируется как true, даже если враг не был создан. |	Open |
| LEG-004 |	WaveRules.TickBoss |	enemySpawned инициализируется как true, даже если босс не был создан. |	Open |
| LEG-005 |	WaveRules.Tick |	При WavePhase.Completed возвращается default(WaveExecutionOutcome), что может вернуть null-коллекцию врагов. |	Open |
| LEG-006 |	SimulationPlan |	Flat-plan строит melee query до применения движения игрока. |	Open |
| LEG-007 |	SimulationPlan |	Spawn query планируется до проверки итогового поражения игрока. |	Open |
| LEG-008 |	EnemyRules |	Движение врагов игнорирует стены, препятствия и реальные 3D-collision constraints Unity. |	Open |
| LEG-009 |	EnemyRules |	После смерти игрока обработка оставшихся врагов текущего тика не остановлена явно. |	Open |
| LEG-010 |	GameState |	IReadOnlyDictionary не обеспечивает глубокую неизменяемость при передаче реализации Dictionary. |	Open |
| LEG-011 |	GameConfig |	IReadOnlyList поверх массива не исключает mutation через alias или cast. |	Open |
| LEG-012 |	SimulationResult |	Результат построен из boolean flags и primitive state encoding. |	Planned removal |
| LEG-013 |	SimulationResult |	Коллекции фактов могут быть получены через mutable alias. |	Planned removal |
| LEG-014 |	AttackResolution |	Нет корреляции между конкретным AttackQuery и его resolution. |	Planned removal |
| LEG-015 |	FrameInput / FrameContext |	Внешний слой потенциально обязан угадывать, какие игровые запросы потребуются ядру. |	Planned removal |
| LEG-016 |	GameConfig |	HasBoss допускает конфигурацию без босса, хотя game rule требует босса в каждой из трёх волн. |	Review in T-1 |
| LEG-017 |	WaveRules |	Спавн одного обычного врага на тик является неявной механикой без отдельной spawn policy. |	Review in T-5 |
| LEG-018 |	EntityId |	Общий технический идентификатор не выражает предметную роль ArenaRun, Player или Enemy. |	Planned removal |