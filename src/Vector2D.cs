using System;
using System.Runtime.CompilerServices;

namespace UPG_SP_2024
{
    public struct Vector2D
    {
        public float X { get; }
        public float Y { get; }

        public Vector2D(float x, float y)
        {
            X = x;
            Y = y;
        }

        // Add two vectors
        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X + b.X, a.Y + b.Y);
        }
        
        public static bool operator ==(Vector2D a, Vector2D b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2D a, Vector2D b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        // Subtract two vectors
        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X - b.X, a.Y - b.Y);
        }

        // Multiply vector by a scalar
        public static Vector2D operator *(Vector2D a, float scalar)
        {
            return new Vector2D(a.X * scalar, a.Y * scalar);
        }

        // Divide vector by a scalar
        public static Vector2D operator /(Vector2D a, float scalar)
        {
            return new Vector2D(a.X / scalar, a.Y / scalar);
        }

        // Calculate magnitude of vector
        public float Magnitude()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        // Normalize vector
        public Vector2D Normalize()
        {
            float magnitude = Magnitude();
            return magnitude > 0 ? this / magnitude : new Vector2D(0, 0);
        }

    }
}
