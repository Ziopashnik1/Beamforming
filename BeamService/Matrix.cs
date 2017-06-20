using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BeamService
{
    public class Matrix
    {
        public static Matrix Create(int N, int M, Func<int, int, double> create)
        {
            var result = new double[N, M];
            for (var i = 0; i < N; i++)
                for (var j = 0; j < M; j++)
                    result[i, j] = create(i, j);
            return new Matrix(result);
        }

        private readonly int f_N;
        private readonly int f_M;

        private readonly double[,] f_Data;

        /// <summary>Число строк</summary>
        public int N => f_N;

        /// <summary>Число столбцов</summary>
        public int M => f_M;

        public double[,] Data => f_Data;

        public double this[int i, int j]
        {
            get { return f_Data[i, j]; }
            set { f_Data[i, j] = value; }
        }

        /// <summary>Инициализация новой матрицы</summary>
        /// <param name="N">Число строк</param>
        /// <param name="M">Число столбцов</param>
        public Matrix(int N, int M)
        {
            f_N = N;
            f_M = M;
            f_Data = new double[N, M];
        }

        public Matrix(double[,] data)
        {
            f_Data = (double[,])data.Clone();
            f_N = data.GetLength(0);
            f_M = data.GetLength(1);
        }

        public Matrix GetTranspon()
        {
            var result = new double[f_M, f_N];
            for (var i = 0; i < f_N; i++)
                for (var j = 0; j < f_M; j++)
                    result[j, i] = f_Data[i, j];
            return new Matrix(result);
        }

        public double GetDeterminant()
        {
            if (f_N != f_M)
                throw new InvalidOperationException("Нельзя найти определитель неквадратной матрицы!");
            var n = f_N;
            if (n == 1) return this[0, 0];

            if (n == 2) return this[0, 0] * this[1, 1] - this[0, 1] * this[1, 0];

            var lv_DataArray = (double[,])f_Data.Clone();

            var det = 1.0;
            for (var k = 0; k < n; k++)
            {
                int i;
                int j;
                if (lv_DataArray[k, k].Equals(0))
                {
                    j = k;
                    while (j < n && lv_DataArray[k, j].Equals(0)) j++;

                    if (j == n || lv_DataArray[k, j].Equals(0)) return 0;

                    for (i = k; i < n; i++)
                    {
                        var save = lv_DataArray[i, j];
                        lv_DataArray[i, j] = lv_DataArray[i, k];
                        lv_DataArray[i, k] = save;
                    }
                    det = -det;
                }
                var doagonal_item = lv_DataArray[k, k];

                det *= doagonal_item;

                if (k >= n) continue;

                for (i = k + 1; i < n; i++)
                {
                    var b = lv_DataArray[i, k] / lv_DataArray[k, k];
                    for (j = k; j < n; j++)
                        lv_DataArray[i, j] -= b * lv_DataArray[k, j];
                }
            }
            return det;
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine("{");
            for (var i = 0; i < f_N; i++)
            {
                result.Append(" {");
                for (var j = 0; j < f_M; j++)
                {
                    result.AppendFormat(" {0},", f_Data[i, j]);
                }
                if (result[result.Length - 1] != '}')
                    result.Length--;
                result.AppendLine(" }"); 
            }
            result.AppendLine("}");
            return result.ToString();
        }

        public string ToString(string format)
        {
            var result = new StringBuilder();
            result.AppendLine("{");
            for (var i = 0; i < f_N; i++)
            {
                result.Append(" {");
                for (var j = 0; j < f_M; j++)
                {
                    result.AppendFormat(" {0},", f_Data[i, j].ToString(format));
                }
                if (result[result.Length - 1] != '}')
                    result.Length--;
                result.AppendLine(" }");
            }
            result.AppendLine("}");
            return result.ToString();
        }

        public static Matrix operator +(Matrix A, Matrix B)
        {
            if (A.N != B.N || A.M != B.M)
                throw new ArgumentException("Размерности матриц не совпадают", nameof(B));

            var result = new double[A.N, A.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < A.M; j++)
                    result[i, j] = a[i, j] + b[i, j];
            return new Matrix(result);
        }

        public static Matrix operator +(Matrix A, double B)
        {
            var result = new double[A.N, A.M];
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < A.M; j++)
                    result[i, j] = A.f_Data[i, j] + B;
            return new Matrix(result);
        }

        public static Matrix operator /(Matrix A, double B)
        {
            var result = new double[A.N, A.M];
            for (var i = 0; i < A.N; i++)
            for (var j = 0; j < A.M; j++)
                result[i, j] = A.f_Data[i, j] / B;
            return new Matrix(result);
        }

        public static Matrix operator +(double A, Matrix B)
        {
            return B + A;
        }        

        public static Matrix operator -(Matrix A, Matrix B)
        {
            if (A.N != B.N || A.M != B.M)
                throw new ArgumentException("Размерности матриц не совпадают", nameof(B));

            var result = new double[A.N, A.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < A.M; j++)
                    result[i, j] = a[i, j] - b[i, j];
            return new Matrix(result);
        }

        public static Matrix operator -(Matrix A)
        {
            var result = new double[A.N, A.M];
            var a = A.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < A.M; j++)
                    result[i, j] = -a[i, j];
            return new Matrix(result);
        }

        public static Matrix operator *(Matrix A, Matrix B)
        {
            if (A.M != B.N) throw new ArgumentException("Размерности матриц не согласованы", nameof(B));

            var result = new double[A.N, B.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < B.M; j++)
                    for (var k = 0; k < B.M; k++)
                        result[i, j] += a[i, k] * b[k, j];
            return new Matrix(result);
        }


        public static Matrix GetStolb(Matrix A, int k)
        {
            var result = new double[A.N, 1];
            var a = A.Data;
            for (var i = 0; i < A.N; i++) result[i, 0] = a[i, k];
            return new Matrix(result);
        }

        public static Matrix GetStrok(Matrix A, int k)
        {
            var result = new double[1, A.M];
            var a = A.Data;
            for (var j = 0; j < A.M; j++) result[0, j] = a[k, j];
            return new Matrix(result);
        }

    }
    

    public class MatrixComplex
    {
        private readonly Complex[,] f_Data;

        /// <summary>Создать матрицу</summary>
        /// <param name="N">Число строк</param>
        /// <param name="M">Число столбцов</param>
        /// <param name="create">Порождающая функция</param>
        /// <returns>Матрица с указанным числом строк и столбцов, заполненная указанным методом</returns>
        public static MatrixComplex Create(int N, int M, Func<int, int, Complex> create)
        {
            var result = new Complex[N, M];
             for (var i = 0; i < N; i++)
                for (var j = 0; j < M; j++)
                    result[i, j] = create(i, j);
            return new MatrixComplex(result);
        }

        private readonly int f_N;
        private readonly int f_M;

        //private Complex[,] f_Data;

        /// <summary>Число строк</summary>
        public int N => f_N;

        /// <summary>Число столбцов</summary>
        public int M => f_M;

        public Complex[,] Data => f_Data;

        public Complex this[int i, int j]
        {
            get { return f_Data[i, j]; }
            set { f_Data[i, j] = value; }
        }

        /// <summary>Инициализация новой матрицы</summary>
        /// <param name="N">Число строк</param>
        /// <param name="M">Число столбцов</param>
        public MatrixComplex(int N, int M)
        {
            f_N = N;
            f_M = M;
            f_Data = new Complex[N, M];
        }

        public MatrixComplex(Complex[,] data)
        {
            f_Data = (Complex[,])data.Clone();
            f_N = data.GetLength(0);
            f_M = data.GetLength(1);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine("{");
            for (var i = 0; i < f_N; i++)
            {
                result.Append(" {");
                for (var j = 0; j < f_M; j++)
                {
                    result.AppendFormat(" {0},", f_Data[i, j]);
                }
                if (result[result.Length - 1] != '}')
                    result.Length--;
                result.AppendLine(" }");
            }
            result.AppendLine("}");
            return result.ToString();
        }

        public string ToString(string format)
        {
            var result = new StringBuilder();
            result.AppendLine("{");
            for (var i = 0; i < f_N; i++)
            {
                result.Append(" {");
                for (var j = 0; j < f_M; j++)
                {
                    result.AppendFormat(" {0},", f_Data[i, j].ToString(format));
                }
                if (result[result.Length - 1] != '}')
                    result.Length--;
                result.AppendLine(" }");
            }
            result.AppendLine("}");
            return result.ToString();
        }

        public MatrixComplex GetTranspon()
        {
            var result = new Complex[f_M, f_N];
            for (var i = 0; i < f_N; i++)
                for (var j = 0; j < f_M; j++)
                    result[j, i] = f_Data[i, j];
            return new MatrixComplex(result);
        }

        public Complex GetDeterminant()
        {
            if (f_N != f_M)
                throw new InvalidOperationException("Нельзя найти определитель неквадратной матрицы!");
            var n = f_N;
            if (n == 1) return this[0, 0];

            if (n == 2) return this[0, 0] * this[1, 1] - this[0, 1] * this[1, 0];

            var lv_DataArray = (Complex[,])f_Data.Clone();

            Complex det = 1.0;
            for (var k = 0; k < n; k++)
            {
                int i;
                int j;
                if (lv_DataArray[k, k].Equals(0))
                {
                    j = k;
                    while (j < n && lv_DataArray[k, j].Equals(0)) j++;

                    if (j == n || lv_DataArray[k, j].Equals(0)) return 0;

                    for (i = k; i < n; i++)
                    {
                        var save = lv_DataArray[i, j];
                        lv_DataArray[i, j] = lv_DataArray[i, k];
                        lv_DataArray[i, k] = save;
                    }
                    det = -det;
                }
                var doagonal_item = lv_DataArray[k, k];

                det *= doagonal_item;

                if (k >= n) continue;

                for (i = k + 1; i < n; i++)
                {
                    var b = lv_DataArray[i, k] / lv_DataArray[k, k];
                    for (j = k; j < n; j++)
                        lv_DataArray[i, j] -= b * lv_DataArray[k, j];
                }
            }
            return det;
        }

        public MatrixComplex GetComplexConj()
        {
            var result = new Complex[f_N, f_M];
            for (var i = 0; i < f_N; i++)
                for (var j = 0; j < f_M; j++)
                    result[i, j] = new Complex(f_Data[i, j].Real, -f_Data[i, j].Imaginary);
            return new MatrixComplex(result);
        }

        public static MatrixComplex operator +(Matrix A, MatrixComplex B)
        {
            if (A.N != B.N || A.M != B.M)
                throw new ArgumentException("Размерности матриц не совпадают", nameof(B));

            var result = new Complex[A.N, A.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < A.M; j++)
                    result[i, j] = a[i, j] + b[i, j];
            return new MatrixComplex(result);
        }

        public static MatrixComplex operator +(MatrixComplex A, Matrix B)
        {
            return B + A;
        }

        public static MatrixComplex operator +(MatrixComplex A, MatrixComplex B)
        {
            if (A.N != B.N || A.M != B.M)
                throw new ArgumentException("Размерности матриц не совпадают", nameof(B));

            var result = new Complex[A.N, A.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < A.M; j++)
                    result[i, j] = a[i, j] + b[i, j];
            return new MatrixComplex(result);
        }

        public static MatrixComplex operator -(MatrixComplex A, MatrixComplex B)
        {
            if (A.N != B.N || A.M != B.M)
                throw new ArgumentException("Размерности матриц не совпадают", nameof(B));

            var result = new Complex[A.N, A.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < A.M; j++)
                    result[i, j] = a[i, j] - b[i, j];
            return new MatrixComplex(result);
        }

        public static MatrixComplex operator -(MatrixComplex A, Matrix B)
        {
            if (A.N != B.N || A.M != B.M)
                throw new ArgumentException("Размерности матриц не совпадают", nameof(B));

            var result = new Complex[A.N, A.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < A.M; j++)
                    result[i, j] = a[i, j] - b[i, j];
            return new MatrixComplex(result);
        }

        public static MatrixComplex operator -(Matrix A, MatrixComplex B)
        {
            return B - A;
        }

        public static MatrixComplex operator -(MatrixComplex A)
        {
            var result = new Complex[A.N, A.M];
            var a = A.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < A.M; j++)
                    result[i, j] = -a[i, j];
            return new MatrixComplex(result);
        }

        public static MatrixComplex operator *(MatrixComplex A, MatrixComplex B)
        {
            if (A.M != B.N) throw new ArgumentException("Размерности матриц не согласованы", nameof(B));

            var result = new Complex[A.N, B.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < B.M; j++)
                    for (var k = 0; k < B.M; k++)
                        result[i, j] += a[i, k] * b[k, j];
            return new MatrixComplex(result);
        }

        public static MatrixComplex operator *(MatrixComplex A, Matrix B)
        {
            if (A.M != B.N) throw new ArgumentException("Размерности матриц не согласованы", nameof(B));

            var result = new Complex[A.N, B.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < B.M; j++)
                    for (var k = 0; k < B.M; k++)
                        result[i, j] += a[i, k] * b[k, j];
            return new MatrixComplex(result);
        }

        public static MatrixComplex operator *(Matrix A, MatrixComplex B)
        {
            if (A.M != B.N) throw new ArgumentException("Размерности матриц не согласованы", nameof(B));

            var result = new Complex[A.N, B.M];
            var a = A.Data;
            var b = B.Data;
            for (var i = 0; i < A.N; i++)
                for (var j = 0; j < B.M; j++)
                    for (var k = 0; k < B.M; k++)
                        result[i, j] += a[i, k] * b[k, j];
            return new MatrixComplex(result);
        }

        public static MatrixComplex GetStolb(MatrixComplex A, int k)
        {
            var result = new Complex[A.N, 1];
            var a = A.Data;
            for (var i = 0; i < A.N; i++) result[i, 0] = a[i, k];
            return new MatrixComplex(result);
        }

        public static MatrixComplex GetStrok(MatrixComplex A, int k)
        {
            var result = new Complex[1, A.M];
            var a = A.Data;
            for (var j = 0; j < A.M; j++) result[0, j] = a[k, j];
            return new MatrixComplex(result);
        }

        public Matrix GetReal()
        {
            var result = new double[N, M];
            for (var i = 0; i < N ; i++)
                for (var j = 0; j < M ; j++)
                    result[i, j] = f_Data[i, j].Real;
            return new Matrix(result);
        }

        public Matrix GetIm()
        {
            var result = new double[N, M];
            for (var i = 0; i < N; i++)
                for (var j = 0; j < M; j++)
                    result[i, j] = f_Data[i, j].Imaginary;
            return new Matrix(result);
        }

        public Matrix GetMod()
        {
            var result = new double[N, M];
            var a = Data; 
            for (var i = 0; i < M; i++)
                for (var j = 0; j < M; j++)
                    result[i, j] = Math.Sqrt(Math.Pow(a[i, j].Real, 2) + Math.Pow(a[i, j].Imaginary, 2)) ;
            return new Matrix(result);
        }

        public Matrix GetArg()
        {
            var result = new double[N, M];
            var a = Data;
            for (var i = 0; i < M; i++)
                for (var j = 0; j < M; j++)
                     result[i, j] = a[i, j].Phase;
             return new Matrix(result);
        }

    }
}
