global using EVector3 = ZombieSurvival.Engine.Vector3;

using System.Diagnostics.CodeAnalysis;

namespace ZombieSurvival.Engine;

public struct Vector3(float x = 0, float y = 0, float z = 0)
{
    public float X = x;
    public float Y = y;
    public float Z = z;

    public static Vector3 Zero => zero;
    public static Vector3 One => one;
    public static Vector3 Right => right;
    public static Vector3 Up => up;
    public static Vector3 Forward => foward;

    private static readonly Vector3 zero = new(0, 0, 0);
    private static readonly Vector3 one = new(1, 1, 1);
    private static readonly Vector3 right = new(1, 0, 0);
    private static readonly Vector3 up = new(0, 1, 0);
    private static readonly Vector3 foward = new(0, 0, 1);

    public static Vector3 operator /(Vector3 left, Vector3 right)
    {
        return new(
            left.X / right.X,
            left.Y / right.Y,
            left.Z / right.Z
        );
    }

    public static Vector3 operator /(Vector3 left, float right)
    {
        return new(
            left.X / right,
            left.Y / right,
            left.Z / right
        );
    }

    public static Vector3 operator *(Vector3 left, Vector3 right)
    {
        return new(
            left.X * right.X,
            left.Y * right.Y,
            left.Z * right.Z
        );
    }

    public static Vector3 operator *(Vector3 left, float right)
    {
        return new(
            left.X * right,
            left.Y * right,
            left.Z * right
        );
    }

    public static Vector3 operator *(float left, Vector3 right)
    {
        return new(
            left * right.X,
            left * right.Y,
            left * right.Z
        );
    }

    public static Vector3 operator +(Vector3 left, Vector3 right)
    {
        return new(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z
        );
    }

    public static Vector3 operator -(Vector3 left, Vector3 right)
    {
        return new(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z
        );
    }

    public static Vector3 operator -(Vector3 vector)
    {
        return new(-vector.X, -vector.Y, -vector.Z);
    }

    public static bool operator ==(Vector3 left, Vector3 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector3 left, Vector3 right)
    {
        return !(left == right);
    }

    public static explicit operator GLVector3(Vector3 vector) => new(vector.X, vector.Y, vector.Z);

    public static explicit operator Vector3(GLVector3 vector) => new(vector.X, vector.Y, vector.Z);

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Vector3 other)
            throw new InvalidCastException($"Object has to be Vector3");
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }

    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static Vector3 Cross(Vector3 left, Vector3 right)
    {
        return new(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X)
        );
    }

    public float this[int index]
    {
        readonly get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 2, nameof(index));

            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => 0,
            };
        }

        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 2, nameof(index));

            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
            }
        }
    }

    public readonly float Magnitude
    {
        get { return MathF.Sqrt(X * X + Y * Y + Z * Z); }
    }

    public readonly Vector3 Unit => this / Magnitude;
}

public struct Vector2(float x = 0, float y = 0)
{
    public float X = x;
    public float Y = y;

    public static Vector2 Zero => zero;
    public static Vector2 One => one;
    public static Vector2 Right => right;
    public static Vector2 Up => up;

    private static readonly Vector2 zero = new(0, 0);
    private static readonly Vector2 one = new(1, 1);
    private static readonly Vector2 right = new(1, 0);
    private static readonly Vector2 up = new(0, 1);

    public static Vector2 operator /(Vector2 left, Vector2 right)
    {
        return new(
            left.X / right.X,
            left.Y / right.Y
        );
    }

    public static Vector2 operator /(Vector2 left, float right)
    {
        return new(
            left.X / right,
            left.Y / right
        );
    }

    public static Vector2 operator *(Vector2 left, Vector2 right)
    {
        return new(
            left.X * right.X,
            left.Y * right.Y
        );
    }

    public static Vector2 operator *(Vector2 left, float right)
    {
        return new(
            left.X * right,
            left.Y * right
        );
    }

    public static Vector2 operator *(float left, Vector2 right)
    {
        return new(
            left * right.X,
            left * right.Y
        );
    }

    public static Vector2 operator +(Vector2 left, Vector2 right)
    {
        return new(
            left.X + right.X,
            left.Y + right.Y
        );
    }

    public static Vector2 operator -(Vector2 left, Vector2 right)
    {
        return new(
            left.X - right.X,
            left.Y - right.Y
        );
    }

    public static Vector2 operator ~(Vector2 vector)
    {
        return new(-vector.X, -vector.Y);
    }

    public static bool operator ==(Vector2 left, Vector2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2 left, Vector2 right)
    {
        return !(left == right);
    }

    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is not Vector2 other)
            throw new InvalidCastException($"Object has to be Vector2");
        return X == other.X && Y == other.Y;
    }

    public override readonly string ToString()
    {
        return $"({X}, {Y})";
    }

    public override readonly int GetHashCode()
    {
        return base.GetHashCode();
    }

    public float this[int index]
    {
        readonly get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 1, nameof(index));

            return index switch
            {
                0 => X,
                1 => Y,
                _ => 0,
            };
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, 1, nameof(index));

            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
            }
        }
    }

    public readonly float Magnitude
    {
        get { return MathF.Sqrt(X * X + Y * Y); }
    }

    public readonly Vector2 Unit => this / Magnitude;
}
