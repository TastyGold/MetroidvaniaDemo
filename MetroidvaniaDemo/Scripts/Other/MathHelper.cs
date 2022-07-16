using Raylib_cs;

namespace MathExtras
{
    public static class MathHelper
    {
        public static int PartialSum(this int[] vs, int index)
        {
            int total = 0;

            for (int i = 0; i < index; i++)
            {
                total += vs[i];
            }

            return total;
        }

        public static int[] GetIntegerSequence(int start, int end)
        {
            int[] vs = new int[end - start + 1];
            for (int i = start; i <= end; i++)
            {
                vs[i - start] = i;
            }
            return vs;
        }

        public static int ToNearestInteger(this float n)
        {
            if (n >= 0) return (n % 1 >= 0.5f) ? (int)n + 1 : (int)n;
            else return (-n % 1 >= 0.5f) ? (int)n - 1 : (int)n;
        }

        public static Rectangle Multiply(this Rectangle r, int s)
        {
            return new Rectangle(r.x * s, r.y * s, r.width * s, r.height * s);
        }
    }
}