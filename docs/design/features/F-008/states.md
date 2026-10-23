# States

Feature F-008 — Wave progression.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 wave indicator | no run in progress — no wave indicator | — (the wave number is known before the first enemy enters; there is no loading state) | — (no error state; the wave only changes on a wave beginning) | run terminal — the indicator stays frozen on the wave reached, Defeat or Victory | run Playing — the indicator shows the wave in progress and the total, "Wave N of 3" | — (a wave is in progress or not; the indicator is never partly shown) |
| S-001 new-wave announcement | no wave is beginning — no announcement is shown | a wave has just begun — the announcement is visible and the indicator has already advanced | — (a wave beginning is not an error state) | run terminal — no new announcement appears; any visible one finishes or is cleared | a wave begins — the announcement is shown and fades on its own | — (one announcement at a time; a second wave cannot begin before the first announcement's moment has passed) |
| S-001 gameplay inputs | no input this step — nothing changes | a wave has just begun — the inputs still act; the announcement does not block them | — (no error state; a refused or inert input changes nothing) | player defeated or run terminal — movement, view, switch, and attack change nothing (F-001, F-005) | run Playing — inputs act as F-001, F-002, F-003, and F-004 define | — (a wave change does not partly disable an input) |

## Notes

- The indicator's `disabled` value is the terminal run state, not an unavailable control: it keeps the last wave it showed rather than blanking.
- The announcement's `loading` value is its visible lifetime: it is shown at the moment the wave begins and disappears without input.
- A wave beginning is never an error. There is no failure path for the counter: it advances by one when a wave begins and at no other time (A3).
- The final wave is not a separate state — the indicator reads "Wave 3 of 3" and then the Victory window appears (FL-042).
- The health indicator's states remain F-005's and the victory screen's states remain F-011's; F-008 adds no row for either.
