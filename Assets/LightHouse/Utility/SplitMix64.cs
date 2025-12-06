using System;

namespace LightHouse
{
    public struct SplitMix64
    {
        static ulong _staticState = 0;
        ulong _state;

        public SplitMix64(ulong seed)
        {
            _state = seed;
        }

        public ulong NextUInt64()
        {
            ulong z = (_state += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public double NextDouble()
        {
            return (NextUInt64() >> 11) * (1.0 / (1UL << 53));
        }

        public float NextFloat()
        {
            return (float)((NextUInt64() >> 40) * (1.0 / (1U << 24)));
        }

        public (double, double) NextGaussian()
        {
            double u1 = NextDouble();
            double u2 = NextDouble();
            double r = Math.Sqrt(-2.0 * Math.Log(u1));
            double theta = 2.0 * Math.PI * u2;
            double z0 = r * Math.Cos(theta);
            double z1 = r * Math.Sin(theta);
            return (z0, z1); // both ~ N(0,1)
        }

        public double NextBates6()
        {
            double u1 = NextDouble();
            double u2 = NextDouble();
            double u3 = NextDouble();
            double u4 = NextDouble();
            double u5 = NextDouble();
            double u6 = NextDouble();
            return (u1 + u2 + u3 + u4 + u5 + u6) / 6d;
        }

        public static ulong StaticNextUInt64()
        {
            ulong z = (_staticState += 0x9E3779B97F4A7C15UL);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }

        public static double StaticNextDouble()
        {
            return (StaticNextUInt64() >> 11) * (1.0 / (1UL << 53));
        }

        public static float StaticNextFloat()
        {
            return (float)((StaticNextUInt64() >> 40) * (1.0 / (1U << 24)));
        }

        public static (double, double) StaticNextGaussian()
        {
            double u1 = StaticNextDouble();
            double u2 = StaticNextDouble();
            double r = Math.Sqrt(-2.0 * Math.Log(u1));
            double theta = 2.0 * Math.PI * u2;
            double z0 = r * Math.Cos(theta);
            double z1 = r * Math.Sin(theta);
            return (z0, z1); // both ~ N(0,1)
        }

        public static double StaticNextBates6()
        {
            double u1 = StaticNextDouble();
            double u2 = StaticNextDouble();
            double u3 = StaticNextDouble();
            double u4 = StaticNextDouble();
            double u5 = StaticNextDouble();
            double u6 = StaticNextDouble();
            return (u1 + u2 + u3 + u4 + u5 + u6) / 6d;
        }
    }
}