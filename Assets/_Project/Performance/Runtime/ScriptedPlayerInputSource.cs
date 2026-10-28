using System;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Performance
{
    /// <summary>
    /// The deterministic measurement cadence (ADR-0034,
    /// docs/architect/migration/performance-measurement.md § Scripted input cadence).
    /// It reads no enemy state and keeps no per-frame allocation: misses still
    /// execute the targeting, physics-query, and attack-impact paths the thresholds
    /// measure.
    /// </summary>
    public sealed class ScriptedPlayerInputSource : IPlayerInputSource
    {
        private const double PhaseSeconds = 2d;
        private const double AimDegreesPerSecond = 36d;

        private static readonly MovementAxes[] Phases =
        {
            new(1f, 0.5f),
            new(1f, -0.5f),
            new(-1f, -0.5f),
            new(-1f, 0.5f),
        };

        private bool _started;
        private double _startTime;
        private long _lastWholeSecond = -1;
        private readonly PlayerFrameInput _frameInput = new();

        /// <summary>
        /// Marks the moment the run became active. The cadence is measured in
        /// unscaled wall time from here.
        /// </summary>
        public void StartRun(double unscaledTime)
        {
            _started = true;
            _startTime = unscaledTime;
            _lastWholeSecond = -1;
        }

        public PlayerFrameInput Read()
        {
            if (_started == false)
            {
                _frameInput.Fill(
                    MovementAxes.Zero,
                    Direction3D.Forward,
                    false,
                    false);

                return _frameInput;
            }

            double elapsed = Time.unscaledTimeAsDouble - _startTime;

            if (elapsed < 0d)
            {
                elapsed = 0d;
            }

            int phaseIndex = (int)(elapsed / PhaseSeconds) % Phases.Length;
            MovementAxes movementAxes = Phases[phaseIndex];

            double radians = elapsed * AimDegreesPerSecond * Math.PI / 180d;
            Direction3D viewDirection = Direction3D.Forward;

            if (Direction3D.TryFrom(
                    new Displacement3D((float)Math.Sin(radians), 0f, (float)Math.Cos(radians)),
                    out Direction3D swept))
            {
                viewDirection = swept;
            }

            long second = (long)elapsed;
            bool firstFrameOfSecond = second != _lastWholeSecond;

            if (firstFrameOfSecond)
            {
                _lastWholeSecond = second;
            }

            bool switchWeapon = firstFrameOfSecond
                && (second % 5 == 0 || second % 5 == 1);

            bool attack = firstFrameOfSecond;

            _frameInput.Fill(
                movementAxes,
                viewDirection,
                attack,
                switchWeapon);

            return _frameInput;
        }
    }
}
