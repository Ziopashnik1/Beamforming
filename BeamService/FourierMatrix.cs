using System;
using System.Numerics;

namespace BeamService
{
    public class FourierMatrix : MatrixComplex
    {
        public FourierMatrix(int N, bool IsInv = false) 
            : base(N, N)
        {
            const double pi2 = 2 * Math.PI;
            if (IsInv)
                for (var n = 0; n < N; n++)
                    for (var m = 0; m < N; m++)
                    {
                        var phi = pi2 * n * m / N;
                        f_Data[n, m] = new Complex(Math.Cos(phi) / N, Math.Sin(phi));
                    }
            else
                for (var n = 0; n < N; n++)
                    for (var m = 0; m < N; m++)
                    {
                        var phi = pi2 * n * m / N;
                        f_Data[n, m] = new Complex(Math.Cos(phi) / N, -Math.Sin(phi) / N);
                    }
        }
    }
}
