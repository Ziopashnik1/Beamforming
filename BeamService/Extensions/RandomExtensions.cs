using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class RandomExtensions
    {
        public static void Test()
        {
            var name = new Random().Next("Ivanov", "Petrov", "Sidorov");
        }

        public static T Next<T>(this Random rnd, params T[] variant) => variant[rnd.Next(variant.Length)];

        ///// <summary>Случайное число с нормальным распределением</summary>
        ///// <param name="rnd">Генератор случайных чисел</param>
        ///// <param name="mu">Математическое ожидание</param>
        ///// <param name="D">Дисперсия</param>
        ///// <returns>Случайное число с нормальным распределением</returns>
        //public static double NextNormal(this Random rnd, double mu = 0, double D = 1)
        //{
        //    var a = 1.0 - rnd.NextDouble();
        //    var b = 1.0 - rnd.NextDouble();
            
        //    var x = Math.Sqrt(-2.0 * Math.Log(a)) * Math.Sin(2d * Math.PI * b);
        //    return x * D + mu;
        //}

        public static IEnumerable<double> NextNormal(this Random rnd, int count, double mu = 0, double D = 1)
        {
            for (int i = 0; i < count; i++)
                yield return rnd.NextNormal(mu, D);
        }

        public static IEnumerable<double> NextDouble(this Random rnd, int count)
        {
            for (int i = 0; i < count; i++)
                yield return rnd.NextDouble();
        }

        public static double[] NormalVector(this Random rnd, int count, double mu = 0, double D = 1)
        {
            var result = new double[count];

            rnd.FillNormalVector(result, mu, D);

            return result;
        }

        public static void FillNormalVector(this Random rnd, double[] values, double mu = 0, double D = 1)
        {
            for (int i = 0; i < values.Length; i++)
                values[i] = rnd.NextNormal(mu, D);
        }
    }
}
