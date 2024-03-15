using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliaSet
{
    public class Complex
    {
        public double Real { get; set; }
        public double Imaginary { get; set; }

        public Complex(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }
        public double Mod2()
        {
            return Real * Real + Imaginary * Imaginary;
        }
        public Complex Square()
        {
            double real = Real * Real - Imaginary * Imaginary;
            double imaginary = 2 * Real * Imaginary;
            return new Complex(real, imaginary);
        }
        public Complex Add(Complex other)
        {
            return new Complex(Real + other.Real, Imaginary + other.Imaginary);
        }
        public Complex ComputeNext(Complex c)
        {
            return Square().Add(c);
        }
    }
}
