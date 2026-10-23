# States

Feature F-005 — Player health and death.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 health indicator | — (health always has a value while the view is shown) | — (the indicator updates in the same operation as the damage; there is no loading state) | — (the indicator always reflects the latest applied damage; no error state is shown) | player defeated — the readout shows zero and the fill is empty; the indicator does not change again until a new run | run Playing — the readout shows current / maximum and the fill its proportion; each landed attack reduces it at once | damaged but alive — the readout and fill show the reduced current value |
| S-001 gameplay inputs (movement, view, switch, attack) | no input this step — nothing changes | — (inputs have no loading state) | — (no error state; a refused or inert input changes nothing and is not announced) | player defeated — movement, switch, attack, and view rotation change nothing; the view holds its last direction (F-001b); the state lasts until a new run | run Playing — inputs act; a landed enemy attack reduces health and the indicator updates at once | an attack is pending — the attack input starts no new attack and a switch is refused until it resolves (F-002); enemy damage still updates health |

## Notes

- The health indicator is a display, not a control; its `disabled` state is the defeated state, not an unavailable control.
- The defeated state begins with the same operation that reaches zero health. No state exists between "alive with health above zero" and "defeated with health zero".
- Health never exceeds the maximum and never falls below zero, so the `success` and `partial` values are bounded by the maximum and the `disabled` value is exactly zero.
