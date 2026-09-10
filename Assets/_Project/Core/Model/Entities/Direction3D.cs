using System;

namespace Game.Core.Model.Entities
{
    /// <summary>
    /// Represents a 3D direction vector defined by X, Y, and Z components.
    /// </summary>
    public readonly struct Direction3D : IEquatable<Direction3D>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Direction3D"/> struct with the specified components.
        /// </summary>
        /// <param name="x">The X component of the direction.</param>
        /// <param name="y">The Y component of the direction.</param>
        /// <param name="z">The Z component of the direction.</param>
        public Direction3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the X component of the direction.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Gets the Y component of the direction.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the Z component of the direction.
        /// </summary>
        public float Z { get; }

        /// <summary>
        /// Returns the hash code for this <see cref="Direction3D"/> instance.
        /// </summary>
        /// <returns>A hash code computed from the X, Y, and Z values.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return obj is Direction3D direction && Equals(direction);
        }

        /// <summary>
        /// Determines whether the current <see cref="Direction3D"/> is equal to another <see cref="Direction3D"/>.
        /// </summary>
        /// <param name="other">The <see cref="Direction3D"/> to compare with this instance.</param>
        /// <returns>True if all components (X, Y, Z) match exactly; otherwise, false.</returns>
        public bool Equals(Direction3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
    }
}
