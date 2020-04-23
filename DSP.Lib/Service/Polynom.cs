using System;
using System.Numerics;
using MathCore.Annotations;

namespace DSP.Lib.Service
{
    public class Polynom
    {
        public static double[] GetCoefficients([NotNull] double[] x)
        {
            if (x is null) throw new ArgumentNullException(nameof(x));
            if (x.Length == 0) throw new ArgumentException("Длина массива корней полинома должна быть больше 0");
            if (x.Length == 1) return new[] { -x[0], 1 };

            var a = new double[x.Length + 1];

            a[0] = -x[0];
            a[1] = 1;

            for (var k = 2; k < x.Length + 1; k++)
            {
                a[k] = a[k - 1];
                for (var i = k - 1; i > 0; i--)
                    a[i] = a[i - 1] - a[i] * x[k - 1];
                a[0] = -a[0] * x[k - 1];
            }

            return a;
        }

        public static Complex[] GetCoefficients([NotNull] Complex[] x)
        {
            if (x is null) throw new ArgumentNullException(nameof(x));
            if (x.Length == 0) throw new ArgumentException("Длина массива корней полинома должна быть больше 0");
            if (x.Length == 1) return new[] { -x[0], 1 };

            var a = new Complex[x.Length + 1];

            a[0] = -x[0];
            a[1] = 1;

            for (var k = 2; k < x.Length + 1; k++)
            {
                a[k] = a[k - 1];
                for (var i = k - 1; i > 0; i--)
                    a[i] = a[i - 1] - a[i] * x[k - 1];
                a[0] = -a[0] * x[k - 1];
            }

            return a;
        }

        private double[] _A;

        public int Order => _A.Length - 1;

        public Polynom(double[] A)
        {
            if (A is null) throw new ArgumentNullException(nameof(A));
            if (A.Length == 0) throw new ArgumentException("Длина массива коэффициентов полинома не может быть равна 0");
            _A = A;
        }

        public double GetValue(double x)
        {
            var sum = _A[_A.Length - 1];
            for (var i = _A.Length - 2; i >= 0; i--)
                sum = sum * x + _A[i];
            return sum;
        }

        public Complex GetValue(Complex x)
        {
            Complex sum = _A[_A.Length - 1];
            for (var i = _A.Length - 2; i >= 0; i--)
                sum = sum * x + _A[i];
            return sum;
        }
    }
}
