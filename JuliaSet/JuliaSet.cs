using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliaSet
{
    public class JuliaSet
    {
        public static double offsetX = 0;
        public static double offsetY = 0;
        public static double Mod2(Vector2D vec)
        {
            return vec.X * vec.X + vec.Y * vec.Y;
        }
        public static Vector2D ComputeNext(Vector2D z, Vector2D c)
        {
            return new Vector2D(z.X * z.X - z.Y * z.Y + c.X, 2 * z.X * z.Y + c.Y);
        }
        public static int GetIterations(Complex z, Complex constant, int maxIterations)
        {
            int iterations = 0;
            while (z.Mod2() < 4 && iterations < maxIterations)
            {
                z = z.ComputeNext(constant);
                iterations++;
            }
            return iterations;
        }

        public static double GetIterationsSmooth(Complex z, Complex constant, int maxIterations)
        {
            Complex zn = z;
            int iterations = 0;
            while (zn.Mod2() < 4 && iterations < maxIterations)
            {
                zn = zn.ComputeNext(constant);
                iterations++;
            }
            double mod = Math.Sqrt(zn.Mod2());
            double smooth = iterations - Math.Log(Math.Log(mod)) / Math.Log(2);
            smooth = Math.Max(0, Math.Min(smooth, maxIterations));
            return smooth;
        }
        public static double GetIterationsSmootReduceFunction(Complex z, Complex constant, int maxIterations)
        {
            double mod2 = z.Real * z.Real + z.Imaginary * z.Imaginary;
            int iterations = 0;
            while (mod2 < 4 && iterations < maxIterations)
            {
                double zTempReal = z.Real * z.Real - z.Imaginary * z.Imaginary + constant.Real;
                z.Imaginary = 2 * z.Real * z.Imaginary + constant.Imaginary;
                z.Real = zTempReal;

                mod2 = z.Real * z.Real + z.Imaginary * z.Imaginary;
                iterations++;
            }
            double smooth = iterations - Math.Log(Math.Log(Math.Sqrt(mod2))) / Math.Log(2);
            smooth = Math.Max(0, Math.Min(smooth, maxIterations));
            return smooth;
        }
        public static double GetIterationsSmootReduceFunctionOpti(Complex z, Complex constant, int maxIterations)
        {
            double zReal = z.Real;
            double zImaginary = z.Imaginary;

            zReal += offsetX;
            zImaginary += offsetY;

            double cReal = constant.Real;
            double cImaginary = constant.Imaginary;

            double mod2 = zReal * zReal + zImaginary * zImaginary;
            int iterations = 0;
            while (mod2 < 4 && iterations < maxIterations)
            {
                double zTempReal = zReal * zReal - zImaginary * zImaginary + cReal;
                zImaginary = 2 * zReal * zImaginary + cImaginary;
                zReal = zTempReal;

                mod2 = zReal * zReal + zImaginary * zImaginary;
                iterations++;
            }
            double smooth = iterations - Math.Log(Math.Log(Math.Sqrt(mod2))) / Math.Log(2);
            smooth = Math.Max(0, Math.Min(smooth, maxIterations));
            return smooth;
        }
    }

}
