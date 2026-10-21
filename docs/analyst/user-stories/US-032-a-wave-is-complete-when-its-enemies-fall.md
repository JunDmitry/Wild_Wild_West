# US-032 — A wave is complete when its enemies fall

- **Feature:** F-008 — Wave progression
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-057`, `BR-058`, `BR-062`

## Story

As a player, I want a wave to be complete only when every enemy in it,
including its boss, has been defeated, so that clearing a wave always means the
arena is genuinely emptied and the next fight is not fought alongside the
last one.

## Examples

**E1 — the wave stays open while enemies remain**
- Given a Wave still has Active Enemies,
- when one of them is defeated and others are still Active,
- then the Wave does not complete.

**E2 — defeating the boss completes the wave**
- Given the Wave's last remaining Active Enemy is its Boss,
- when that Boss is defeated,
- then the Wave completes.

**E3 — a wave with no regular enemies still needs its boss**
- Given a Wave began in Boss Combat because it has no Regular Enemies,
- when its Boss is defeated,
- then the Wave completes.

## Notes

A wave's required Enemies are the Regular Enemies and the Boss. A Boss cannot
become active while Regular Enemies remain to be spawned or alive, so the Boss
is in practice the last enemy a player faces in a wave.
