# States

Feature F-011 — Victory after three waves.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-002 victory window | not shown — the run has not been won | — (the window appears in the same operation as the winning attack; no loading state) | — (reaching Victory always shows the window; no error state) | — (the window has no control to disable; it is not dismissible) | shown — the third Wave completed; the title and the message are present together over the still arena | — (title and message appear together; no partial content) |
| S-001 arena after victory | no enemy remains — the third Wave's last enemy was its Boss | — (the view is already present; it does not load or reload) | — (the arena shows no error) | run won — no enemy enters, no attack resolves, and the view stops changing | run Playing — the arena changes as F-006, F-007, and F-009 define | a Wave is in progress — enemies enter and are defeated while the run is Playing |
| S-001 gameplay inputs after victory | no input this step — nothing changes | — (inputs have no loading state) | — (no error state; a refused input changes nothing and is not announced) | run won — movement, view, switch, and attack change nothing; S-002 holds the focus | run Playing — inputs act as F-001, F-002, F-003, F-004, and F-006 define | — (after Victory there is no partial input state) |

## Notes

- The window's empty state is its absence: before the third Wave completes, S-002 is not shown.
- The `disabled` value for the inputs is the post-victory state, not an unavailable control. The run is terminal, so the inputs are inert.
- The window has no control, so it has no control-state row of its own; its `disabled` column records that it cannot be dismissed.
- The health and equipped-weapon indicators hold their last values after Victory. They do not reset, empty, or disappear.
- No state exists between the winning attack and the window. The third Wave's completion, the transition to Victory, and the window's appearance occur in one operation.
- A refused input after Victory produces no visual change and no announcement; it is not an error state.
