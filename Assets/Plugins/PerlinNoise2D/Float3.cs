namespace Plus.Noise
{
    public struct QuadVectors
    {
        public Float3 BottomLeft { get; }
        public Float3 BottomRight { get; }
        public Float3 TopLeft { get; }
        public Float3 TopRight { get; }

        public QuadVectors(Float3 bottomLeft, Float3 bottomRight, Float3 topLeft, Float3 topRight)
        {
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            TopLeft = topLeft;
            TopRight = topRight;
        }
    }

    // Todo: для 2d-случая, можно использовать XZ структуру
    public struct Float3
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static float Dot(Float3 first, Float3 second)
        {
            return (first.X * second.X + first.Y * second.Y + first.Z * second.Z);
        }

        public static Float3 operator +(Float3 first, Float3 second)
        {
            return new Float3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }

        public static Float3 operator -(Float3 first, Float3 second)
        {
            return new Float3(first.X - second.X, first.Y - second.Y, first.Z - second.Z);
        }
    }
}

