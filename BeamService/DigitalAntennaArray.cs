using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BeamService
{
    public class DigitalAntennaArray
    {
        private const double c = 3e8;
        private static readonly Complex i1 = Complex.ImaginaryOne;
        private const double pi2 = Math.PI * 2;
        private const double toRad = Math.PI / 180;

        private readonly ADC[] f_ADC;
        private MatrixComplex f_Wt;
        private double f_th0;
        private MatrixComplex f_Wth0;
        private MatrixComplex f_W_inv;

        /// <summary>Число элементов реешётки</summary>
        public int N { get; }

        /// <summary>Шаг решетки</summary>
        public double d { get; }

        /// <summary>размер апертуры</summary>
        public double AperturaLength => d * (N - 1);

        /// <summary> Угол фазирования</summary>
        public double th0
        {
            get => f_th0;
            set
            {
                if(Equals(f_th0, value)) return;
                f_th0 = value;
                f_Wth0 = Get_Wth0(value);
            }
        }

        public ReadOnlyCollection<ADC> ADC => new ReadOnlyCollection<ADC>(f_ADC);

        /// <summary> размер выборки цифрового сигнала </summary>
        public int Nd { get; }

        /// <summary>Шаг спектральных компонент в спектре</summary>
        public double df => fd / Nd;   /// дискрет частоты ведь будет всегда несущая !!!!

        /// <summary>Частота дискретизации</summary>
        public double fd { get; }

        /// <summary>Число разрядов в кода</summary> 
        public int n { get; }

        public Func<double, double> ElementPattern { get; set; } = th => Math.Cos(th);

        /// <summary>Максимальная амплитуда сигнала</summary>  
        public double MaxValue { get; }

        /// <summary>
        /// Инициализация новой цифровой антенной решётки
        /// </summary>
        /// <param name="N">Число пространственных каналов</param>
        /// <param name="d">Шаг элементов в решетке</param>
        /// <param name="fd">Частота дискретизации</param>
        /// <param name="n">Чмсло разрядов в коде</param>
        /// <param name="Nd">Размер выборки цифрового сигнала</param>
        /// <param name="MaxValue"></param>
        public DigitalAntennaArray(int N, double d, double fd, int n, int Nd, double MaxValue, double tj = 0)
        {
            if (N <= 0) throw new ArgumentOutOfRangeException(nameof(N), "Число элементов рештки должно быть неотрицательным значением");
            if (d <= 0) throw new ArgumentOutOfRangeException(nameof(d), "Шаг между элементами решётки не может быть меньше, либо равен нулю");
            if (fd <= 0) throw new ArgumentOutOfRangeException(nameof(fd), "Частота дискретизации должна быть положительным числом");
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Число разрядов АЦП не может быть меньше, либо равно нулю");
            if (Nd <= 0) throw new ArgumentOutOfRangeException(nameof(Nd), "Размер выборки должен быть положительным числом");
            if (MaxValue <= 0) throw new ArgumentOutOfRangeException(nameof(MaxValue), "Максимальая амплитуда АЦП не может быть меньше, либо равна нулю");

            this.N = N;
            this.d = d;
            this.fd = fd;
            this.n = n;
            this.Nd = Nd;
            this.MaxValue = MaxValue;

            f_ADC = new ADC[N];
            for (var i = 0; i < N; i++) f_ADC[i] = new ADC(n, fd, MaxValue, tj);

            f_Wt = MatrixComplex.Create(Nd, Nd, (nn, mm) => Complex.Exp(-i1 * pi2 * nn * mm / Nd) / Nd);
            f_W_inv = MatrixComplex.Create(Nd, Nd, (i, j) => Complex.Exp(pi2 * i1 * i * j / Nd));
            f_Wth0 = Get_Wth0(0);
        }
        /// <summary>
        /// Формирование падающей волны
        /// </summary>
        /// <param name="th">Угол падения волны</param>
        /// <param name="signal"></param>
        /// <returns></returns>
        private Source[] GetSources(double th, Func<double, double> signal)
        {
            var sources = new Source[N];
            for (var i = 0; i < N; i++)
            {
                var dt = i * d / c * Math.Sin(th);
                sources[i] = new Source(t => signal(t - dt) * ElementPattern(th));
            }
            return sources;
        }


        /// <summary>
        /// Формирование матрицы продескретизированных сигналов в каждом элементе решетки 
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        private Matrix GetSignalMatrix(Source[] sources)
        {
            var s_data = new double[N, Nd];
            for (var i = 0; i < N; i++)
            {
                var signal = f_ADC[i].GetDiscretSignalValues(sources[i], Nd);
                for (var j = 0; j < Nd; j++) s_data[i, j] = signal[j];
            }
            return new Matrix(s_data);
        }

        /// <summary>
        /// Метод получения матрицы спектров
        /// </summary>
        /// <param name="SignalMatrix"></param>
        /// <returns></returns>
        private MatrixComplex GetSpectralMatrix(Matrix SignalMatrix) => SignalMatrix * f_Wt;

        /// <summary>
        /// Создание фазирующей матрицы
        /// </summary>
        /// <returns></returns>
        private MatrixComplex Get_Wth0(double t0) => MatrixComplex.Create(N, Nd, (i, m) =>
        {
            var fm = m_correction(m, Nd) * df;
            return Complex.Exp(i1 * pi2 / c * fm * i * d * Math.Sin(t0));
        });

        /// <summary>
        /// корректирующая функция
        /// </summary>
        /// <param name="m"></param>
        /// <param name="M"></param>
        /// <returns></returns>
        private static int m_correction(int m, int M) => m <= M / 2 ? m : m - M;

        /// <summary>
        /// поэлементное умножение матрицы на матрицу, используется при фазировании
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private static MatrixComplex ElementMultiply(MatrixComplex A, MatrixComplex B)
        {
            if (A.M != B.M) throw new InvalidOperationException("Число столбцов матриц не совпадает");
            if (A.N != B.N) throw new InvalidOperationException("Число строк матриц не совпадает");

            var N = A.N;
            var M = B.M;

            var result = new Complex[N,M];
            for (var i = 0; i < N; i++)
                for (var j = 0; j < M; j++)
                    result[i, j] = A[i, j] * B[i, j];
            return new MatrixComplex(result);
        }

        /// <summary>
        /// фазирование и получение матрицы сфазированных спектров
        /// </summary>
        /// <param name="SpectralMatrix"></param>
        /// <returns></returns>
        private MatrixComplex ComputeResultMatrix(MatrixComplex SpectralMatrix) => ElementMultiply(SpectralMatrix, f_Wth0);

        /// <summary>
        /// сигналы с одинаков ВОЗМОЖНО ОШИБКА!!!!!!!!!!!!!!!!!
        /// </summary>
        /// <param name="SpectralMatrix"></param>
        /// <returns></returns>
        private MatrixComplex ComputeResultSignal(MatrixComplex SpectralMatrix)
        {
            var result = SpectralMatrix * f_W_inv;
            if (result.N != 1) throw new InvalidOperationException("В результате вычислений получено более одной строки в выходной сигнальной матрице");
            return result;
        }

        /// <summary>
        /// Получение строки усиленного сигнала 
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        private static MatrixComplex SumRows(MatrixComplex A)
        {
            var result = new Complex[1, A.M];
            for (var j = 0; j < A.M; j++)
            {
                var summ = new Complex();
                for (var i = 0; i < A.N; i++) summ += A[i, j];
                result[0, j] = summ;
            }
            return new MatrixComplex(result);
        }

        /// <summary>
        /// Получение мощности усиленного сигнала
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        private static double GetPower(MatrixComplex q)
        {
            var P = 0d;
            for (var i = 0; i < q.M; i++)
            {
                var s = q[0, i].Magnitude;
                P += s * s;
            }
            return P / q.M;
        }

        /// <summary>
        /// Процесс диаграммообразования и усиления сигнала
        /// </summary>
        /// <param name="th">угол падения</param>
        /// <returns></returns>
        public double ComputePatternValue(double th, Func<double, double> signal)
        {            
            var sources = GetSources(th, signal);                             // Определяем массив источников для элементов решётки
            var ss = GetSignalMatrix(sources);                                // Определяем сигнальную матрицу на выходе АЦП всех элементов
            var SS = GetSpectralMatrix(ss);                                   // Получаем спектральную матрицу, как произведение ss*Wt
            var QQ = ComputeResultMatrix(SS);                                 // Диаграммообразование - доварачиваем спектр всех компонент спектральной матрицы с учётом сдвигов фаз
            var Q = SumRows(QQ);                                              // Складываем элементы столбцов получая строку - матрицу спектра выходного сигнала схемы ЦДО
            var q = ComputeResultSignal(Q);                                   // Вычисляем обратное преобразование Фурье для получение выходного сигнала
            return GetPower(q);                                               // Вычисляем мощность выходного сигнала
        }

        /// <summary>
        /// процесс диаграммообразования по мощности
        /// </summary>
        /// <param name="f0">частота сигнала</param>
        /// <param name="th1">левый предел</param>
        /// <param name="th2">правый предел</param>
        /// <param name="dth">угловой шаг</param>
        /// <returns></returns>
        public PatternValue[] ComputePattern(Func<double, double> signal, double th1 = -90 * toRad, double th2 = 90 * toRad, double dth = pi2 / 360 / 2)
        {
            var result = new List<PatternValue>(1000);
            var th = th1;
            while (th <= th2)
            {
                var value = ComputePatternValue(th, signal);
                result.Add(new PatternValue { Angle = th, Value = value });
                th += dth;
            }
            return result.ToArray();
        }
    }
}
