using System;
using Game.Arena.Application.Input;
using Game.Arena.Domain.Geometry;
using NUnit.Framework;

namespace Game.Arena.Application.Tests.Input
{
    [TestFixture]
    public sealed class PlayerFrameInputTests
    {
        [Test]
        public void ConstructorPreservesInputValues()
        {
            MovementInput movement = MovementInput.FromVector(
                new Displacement3D(0.5f, 0f, 0f));

            PlayerFrameInput input = new(
                movement,
                Direction3D.Right,
                true,
                true);

            Assert.That(input.Movement, Is.EqualTo(movement));
            Assert.That(input.AimDirection, Is.EqualTo(Direction3D.Right));
            Assert.That(input.AttackPressed, Is.True);
            Assert.That(input.SwitchWeaponPressed, Is.True);
        }

        [Test]
        public void ConstructorRejectsInvalidAimDirection()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new PlayerFrameInput(
                        MovementInput.Zero,
                        default,
                        false,
                        false);
                });
        }

        [Test]
        public void AimIsAvailableWithoutNewAttackRequest()
        {
            PlayerFrameInput input = new PlayerFrameInput(
                MovementInput.Zero,
                Direction3D.Left,
                false,
                false);

            Assert.That(input.AttackPressed, Is.False);
            Assert.That(input.AimDirection, Is.EqualTo(Direction3D.Left));
        }
    }
}
