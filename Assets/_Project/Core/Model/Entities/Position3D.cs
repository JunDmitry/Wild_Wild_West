using System;

namespace Game.Core.Model.Entities
{
    /// <summary>
    /// Represents a 3D position defined by X, Y, and Z coordinates.
    /// </summary>
    public readonly struct Position3D : IEquatable<Position3D>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Position3D"/> struct with the specified coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public Position3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets a <see cref="Position3D"/> instance representing the origin (0, 0, 0).
        /// </summary>
        public static Position3D Zero { get; } = new Position3D(0, 0, 0);

        /// <summary>
        /// Gets the X coordinate of the position.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Gets the Y coordinate of the position.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Gets the Z coordinate of the position.
        /// </summary>
        public float Z { get; }

        public static bool operator ==(Position3D a, Position3D b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Position3D a, Position3D b)
        {
            return a.Equals(b) == false;
        }

        /// <summary>
        /// Returns the hash code for this <see cref="Position3D"/> instance.
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
            return obj is Position3D position && Equals(position);
        }

        /// <summary>
        /// Determines whether the current <see cref="Position3D"/> is equal to another <see cref="Position3D"/>.
        /// </summary>
        /// <param name="other">The <see cref="Position3D"/> to compare with this instance.</param>
        /// <returns>True if all coordinates (X, Y, Z) match; otherwise, false.</returns>
        public bool Equals(Position3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }
    }
}
