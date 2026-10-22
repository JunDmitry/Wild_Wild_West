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
            MovementAxes axes = new MovementAxes(0.5f, -0.25f);

            PlayerFrameInput input = new(
                axes,
                Direction3D.Right,
                true,
                true);

            Assert.That(input.MovementAxes, Is.EqualTo(axes));
            Assert.That(input.ViewDirection, Is.EqualTo(Direction3D.Right));
            Assert.That(input.AttackPressed, Is.True);
            Assert.That(input.SwitchWeaponPressed, Is.True);
        }

        [Test]
        public void ConstructorRejectsInvalidViewDirection()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new PlayerFrameInput(
                        MovementAxes.Zero,
                        default,
                        false,
                        false);
                });
        }

        [Test]
        public void ConstructorRejectsNonHorizontalViewDirection()
        {
            Assert.Throws<ArgumentException>(
                () =>
                {
                    _ = new PlayerFrameInput(
                        MovementAxes.Zero,
                        Direction3D.Up,
                        false,
                        false);
                });
        }

        [Test]
        public void ViewDirectionIsAvailableWithoutNewAttackRequest()
        {
            PlayerFrameInput input = new PlayerFrameInput(
                MovementAxes.Zero,
                Direction3D.Left,
                false,
                false);

            Assert.That(input.AttackPressed, Is.False);
            Assert.That(input.ViewDirection, Is.EqualTo(Direction3D.Left));
        }
    }
}
