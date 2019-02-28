using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using DSP.Lib;
using MathService;

namespace BeamService
{
    public class DigitalAntennaArray : ViewModel
    {
        private const double c = 3e8;
        private static readonly Complex i1 = Complex.i;
        private const double pi2 = Math.PI * 2;
        private const double toRad = Math.PI / 180;

        private ADC[] f_ADC;
        private DigitalFilter[] f_Filters;
        private MatrixComplex f_Wt;
        private double f_th0;
        private MatrixComplex f_Wth0;
        private MatrixComplex f_W_inv;
        private int f_N;
        private double f_tj;
        private double f_d;
        private double f_fd;
        private int f_n;
        private double f_MaxValue;
        private int f_Nd;   //нужно сдлеать матрицы ПФ саморасчитываемыми, без инициализации новой ЦАР
        private IAntenna f_Element = new Uniform();

        private double f_FilterF0;
        private double f_FilterDf;

        public double FilterF0
        {
            get => f_FilterF0;
            set
            {
                if (Equals(f_FilterF0, value)) return;
                f_FilterF0 = value;
                if (f_Filters is null) f_Filters = new DigitalFilter[f_N];
                if (f_FilterF0 > 0 && f_FilterDf > 0)
                    for (var i = 0; i < f_Filters.Length; i++)
                        f_Filters[i] = new BandPassRLC(value, f_FilterDf, 1 / f_fd);
                OnPropertyChanged();
            }
        }

        public double FilterDf
        {
            get => f_FilterDf;
            set
            {
                if (Equals(f_FilterDf, value)) return;
                f_FilterDf = value;
                if (f_Filters is null) f_Filters = new DigitalFilter[f_N];
                if (f_FilterF0 > 0 && f_FilterDf > 0)
                    for (var i = 0; i < f_Filters.Length; i++)
                        f_Filters[i] = new BandPassRLC(f_FilterF0, value, 1 / f_fd);
                OnPropertyChanged();
            }
        }

        /// <summary>Число элементов реешётки</summary>
        public int N
        {
            get => f_N;
            set
            {
                if (f_N == value) return;
                f_N = value;
                f_ADC = new ADC[value];
                f_Filters = new DigitalFilter[value];
                for (var i = 0; i < value; i++)
                {
                    f_ADC[i] = new ADC(n, fd, MaxValue, f_tj);
                    if (f_FilterF0 > 0 && f_FilterDf > 0)
                        f_Filters[i] = new BandPassRLC(f_FilterF0, f_FilterDf, 1 / fd);
                }
                f_Wth0 = Get_Wth0(f_th0);
                OnPropertyChanged();
                OnPropertyChanged(nameof(AperturaLength));
            }
        }

        /// <summary>Шаг решетки</summary>
        public double d
        {
            get => f_d;
            set
            {
                if (!Set(ref f_d, value)) return;
                OnPropertyChanged(nameof(AperturaLength));
            }
        }

        /// <summary>размер апертуры</summary>
        public double AperturaLength => d * (N - 1);

        /// <summary>Угол фазирования</summary>
        public double th0
        {
            get => f_th0;
            set
            {
                if (!Set(ref f_th0, value)) return;
                f_Wth0 = Get_Wth0(value);
            }
        }

        public ReadOnlyCollection<ADC> ADC => new ReadOnlyCollection<ADC>(f_ADC);

        /// <summary>Размер выборки цифрового сигнала </summary>
        public int Nd
        {
            get => f_Nd;
            set
            {
                if (value < 2) throw new ArgumentOutOfRangeException(nameof(Nd), "Размер выборки должен быть больше числа элементов");
                if (f_Nd == value) return;
                f_Nd = value;
                f_Wth0 = Get_Wth0(f_th0);
                f_Wt = MatrixComplex.Create(Nd, Nd, (i, j) => Complex.Exp(-pi2 * i * j / Nd) / Nd); //new FourierMatrix(Nd);                        // мне кажется алгоритм не отрабатывает так, как мы хотим
                f_W_inv = MatrixComplex.Create(Nd, Nd, (i, j) => Complex.Exp(pi2 * i * j / Nd));
                OnPropertyChanged();
                OnPropertyChanged(nameof(df));
            }
        }

        /// <summary>Шаг спектральных компонент в спектре</summary>
        public double df => fd / Nd;

        /// <summary>Частота дискретизации</summary>
        public double fd
        {
            get => f_fd;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Частота дискретизации должна быть больше 0");
                if (f_fd.Equals(value)) return;
                f_fd = value;
                if (f_Filters is null) f_Filters = new DigitalFilter[f_N];
                for (var i = 0; i < f_ADC.Length; i++)
                {
                    f_ADC[i].Fd = value;
                    if (f_FilterF0 > 0 && f_FilterDf > 0)
                        f_Filters[i] = new BandPassRLC(f_FilterF0, f_FilterDf, 1 / fd);
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(df));
            }
        }

        /// <summary>Величина джиттера</summary>
        public double tj
        {
            get => f_tj;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Величина джиттера не должна быть меньше 0");
                if (f_tj.Equals(value)) return;
                f_tj = value;
                for (var i = 0; i < f_ADC.Length; i++) f_ADC[i].tj = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Число разрядов в кода</summary> 
        public int n
        {
            get => f_n;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(n), "Число разрядов кода АЦП должно быть больше 0");
                if (f_n == value) return;
                f_n = value;
                for (var i = 0; i < f_ADC.Length; i++) f_ADC[i].N = value;
                OnPropertyChanged();
            }
        }

        public IAntenna Element
        {
            get => f_Element;
            set => Set(ref f_Element, value);
        }

        /// <summary>Максимальная амплитуда сигнала</summary>  
        public double MaxValue
        {
            get => f_MaxValue;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Амплитуда ограничителя АЦП должна быть больше 0");
                if (f_MaxValue.Equals(value)) return;
                f_MaxValue = value;
                for (var i = 0; i < f_ADC.Length; i++) f_ADC[i].MaxValue = value;
                OnPropertyChanged();
            }
        }
        /// <summary>Инициализация новой цифровой антенной решётки</summary>
        /// <param name="N">Число пространственных каналов</param>
        /// <param name="d">Шаг элементов в решетке</param>
        /// <param name="fd">Частота дискретизации</param>
        /// <param name="n">Чмсло разрядов в коде</param>
        /// <param name="Nd">Размер выборки цифрового сигнала</param>
        /// <param name="MaxValue">Динамический диапазон АЦП (амплитуда входного ограниччителя)</param>
        public DigitalAntennaArray(int N, double d, double fd, int n, int Nd, double MaxValue, double tj = 0)
        {
            if (N <= 0) throw new ArgumentOutOfRangeException(nameof(N), "Число элементов рештки должно быть неотрицательным значением");
            if (d <= 0) throw new ArgumentOutOfRangeException(nameof(d), "Шаг между элементами решётки не может быть меньше, либо равен нулю");
            if (fd <= 0) throw new ArgumentOutOfRangeException(nameof(fd), "Частота дискретизации должна быть положительным числом");
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "Число разрядов АЦП не может быть меньше, либо равно нулю");
            if (Nd <= 0) throw new ArgumentOutOfRangeException(nameof(Nd), "Размер выборки должен быть положительным числом");
            if (MaxValue <= 0) throw new ArgumentOutOfRangeException(nameof(MaxValue), "Максимальая амплитуда АЦП не может быть меньше, либо равна нулю");

            f_d = d;
            f_fd = fd;
            f_n = n;
            f_Nd = Nd;
            f_MaxValue = MaxValue;
            f_tj = tj;
            this.N = N; // Нужно установить именно через свойство, что бы создать новый массив АЦП

            f_Wt = MatrixComplex.Create(Nd, Nd, (i, j) => Complex.Exp(-pi2 * i * j / Nd) / Nd); //new FourierMatrix(Nd);                        // мне кажется алгоритм не отрабатывает так, как мы хотим
            f_W_inv = MatrixComplex.Create(Nd, Nd, (i, j) => Complex.Exp(pi2 * i * j / Nd)); //new FourierMatrix(Nd, true);  

        }
        /// <summary>
        /// Формирование падающей волны - набор сигналов решётки для конкретного источника, заданного углом падения волны на решётку и сигналом
        /// </summary>
        /// <param name="th">Угол падения волны</param>
        /// <param name="signal"></param>
        /// <returns></returns>
        public AnalogSignalSource[] GetSources(double th, Func<double, double> signal)
        {
            var sources = new AnalogSignalSource[N];
            for (var i = 0; i < N; i++)
            {
                var dt = i * d / c * Math.Sin(th);
                sources[i] = new AnalogSignalSource(t => signal(t - dt) * Element.Pattern(th).Abs);
            }
            return sources;
        }

        /// <summary>Формирование матрицы продескретизированных сигналов в каждом элементе решетки</summary>
        /// <param name="sources">Массив аналоговых источников сигналов на входах АЦП</param>
        /// <returns>Сигнальная матрица с выходов АЦП</returns>
        private Matrix GetSignalMatrix(AnalogSignalSource[] sources)
        {
            var s_data = new double[N, Nd];
            for (var i = 0; i < N; i++)
            {
                var signal = f_ADC[i].GetDiscretSignalValues(sources[i], Nd);
                if (f_Filters != null && f_Filters[i] != null)
                    signal = f_Filters[i].Filter(signal).ToArray();
                for (var j = 0; j < Nd; j++) s_data[i, j] = signal[j];
            }
            return new Matrix(s_data);
        }

        private async Task<Matrix> GetSignalMatrixAsync(AnalogSignalSource[] sources, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            var s_data = new double[N, Nd];

            await sources.Select((s, i) => Task.Run(async () =>
            {
                var signal = f_ADC[i].GetDiscretSignalValues(sources[i], Nd);
                var tt = signal.Select((v, j) => Task.Run(() => s_data[i, j] = signal[j], cancel));
                await Task.WhenAll(tt);
            }, cancel)).WhenAll().ConfigureAwait(false);

            cancel.ThrowIfCancellationRequested();

            return new Matrix(s_data);
        }

        public SamplesSignal[] GetInputDigitalSignals(double th, Func<double, double> signal)
        {
            var sources = GetSources(th, signal);
            var signals = new SamplesSignal[N];

            for (var i = 0; i < N; i++)
            {
                var s = f_ADC[i].GetDiscretSignalValues(sources[i], Nd);
                signals[i] = new SamplesSignal(s, f_ADC[i].dt);
            }

            return signals;
        }

        /// <summary>
        /// Метод получения матрицы спектров
        /// </summary>
        /// <param name="SignalMatrix"></param>
        /// <returns></returns>
        public MatrixComplex GetSpectralMatrix(Matrix SignalMatrix) => SignalMatrix * f_Wt;
        public Task<MatrixComplex> GetSpectralMatrixAsync(Matrix SignalMatrix, CancellationToken cancel) =>
            Task.Run(() => SignalMatrix * f_Wt, cancel);

        /// <summary>
        /// Создание фазирующей матрицы
        /// </summary>
        /// <returns></returns>
        private MatrixComplex Get_Wth0(double t0) => MatrixComplex.Create(N, Nd, (i, m) =>
        {
            var fm = m_correction(m, Nd) * df;
            return Complex.Exp(pi2 / c * fm * i * d * Math.Sin(t0));
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

            var result = new Complex[N, M];
            for (var i = 0; i < N; i++)
                for (var j = 0; j < M; j++)
                    result[i, j] = A[i, j] * B[i, j];
            return new MatrixComplex(result);
        }

        private static async Task<MatrixComplex> ElementMultiplyAsync(MatrixComplex A, MatrixComplex B, CancellationToken cancel)
        {
            if (A.M != B.M) throw new InvalidOperationException("Число столбцов матриц не совпадает");
            if (A.N != B.N) throw new InvalidOperationException("Число строк матриц не совпадает");
            cancel.ThrowIfCancellationRequested();

            var N = A.N;
            var M = B.M;

            var result = new Complex[N, M];
            if (N < M)
                await Enumerable.Range(0, N).Select(i => Task.Run(() =>
                {
                    for (var j = 0; j < M; j++)
                        result[i, j] = A[i, j] * B[i, j];
                }, cancel)).WhenAll().ConfigureAwait(false);
            else
                await Enumerable.Range(0, M).Select(j => Task.Run(() =>
                {
                    for (var i = 0; i < N; i++)
                        result[i, j] = A[i, j] * B[i, j];
                }, cancel)).WhenAll().ConfigureAwait(false);

            cancel.ThrowIfCancellationRequested();
            return new MatrixComplex(result);
        }

        /// <summary>
        /// фазирование и получение матрицы сфазированных спектров
        /// </summary>
        /// <param name="SpectralMatrix"></param>
        /// <returns></returns>
        private MatrixComplex ComputeResultMatrix(MatrixComplex SpectralMatrix) => ElementMultiply(SpectralMatrix, f_Wth0);
        private Task<MatrixComplex> ComputeResultMatrixAsync(MatrixComplex SpectralMatrix, CancellationToken cancel) =>
            ElementMultiplyAsync(SpectralMatrix, f_Wth0, cancel);

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

        private async Task<MatrixComplex> ComputeResultSignalAsync(MatrixComplex SpectralMatrix, CancellationToken cancel)
        {
            var result = await Task.Run(() => SpectralMatrix * f_W_inv, cancel).ConfigureAwait(false);
            cancel.ThrowIfCancellationRequested();
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

        private static async Task<MatrixComplex> SumRowsAsync(MatrixComplex A, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();
            var result = new Complex[1, A.M];

            await Enumerable.Range(0, A.M).Select(j => Task.Run(() =>
            {
                var summ = new Complex();
                for (var i = 0; i < A.N; i++) summ += A[i, j];
                result[0, j] = summ;
            }, cancel)).WhenAll().ConfigureAwait(false);
            cancel.ThrowIfCancellationRequested();

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
                var s = q[0, i].Abs;
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
            var sources = GetSources(th, signal);  // Определяем массив источников для элементов решётки
            var ss = GetSignalMatrix(sources);     // Определяем сигнальную матрицу на выходе АЦП всех элементов
            var SS = GetSpectralMatrix(ss);        // Получаем спектральную матрицу, как произведение ss*Wt
            var QQ = ComputeResultMatrix(SS);      // Диаграммообразование - доварачиваем спектр всех компонент спектральной матрицы с учётом сдвигов фаз
            var Q = SumRows(QQ);                   // Складываем элементы столбцов получая строку - матрицу спектра выходного сигнала схемы ЦДО
            var q = ComputeResultSignal(Q);        // Вычисляем обратное преобразование Фурье для получение выходного сигнала
            return GetPower(q);                    // Вычисляем мощность выходного сигнала
        }

        public SamplesSignal[] GetOutSignal(double th, Func<double, double> signal)
        {
            var sources = GetSources(th, signal);  // Определяем массив источников для элементов решётки
            var ss = GetSignalMatrix(sources);     // Определяем сигнальную матрицу на выходе АЦП всех элементов
            var SS = GetSpectralMatrix(ss);        // Получаем спектральную матрицу, как произведение ss*Wt
            var QQ = ComputeResultMatrix(SS);      // Диаграммообразование - доварачиваем спектр всех компонент спектральной матрицы с учётом сдвигов фаз
            var Q = SumRows(QQ);                   // Складываем элементы столбцов получая строку - матрицу спектра выходного сигнала схемы ЦДО
            var q = ComputeResultSignal(Q);        // Вычисляем обратное преобразование Фурье для получение выходного сигнала

            var samples_p = new double[q.M];
            var samples_q = new double[q.M];

            for (var i = 0; i < samples_p.Length; i++)
            {
                samples_p[i] = q[0, i].Re;
                samples_q[i] = q[0, i].Im;
            }

            var dt = f_ADC[0].dt;
            return new[]
            {
                new SamplesSignal(samples_p, dt),
                new SamplesSignal(samples_q, dt)
            };
        }

        /// <summary>Расчёт цифрового сигнала на выходе ЦДО</summary>
        /// <param name="scene">Радиосцена</param>
        /// <param name="angle_offset">Угловой поворот решётки</param>
        /// <returns>Квадратурный сигнал высхода ЦДО</returns>
        public (SamplesSignal P, SamplesSignal Q) GetOutSignal(RadioScene scene, double angle_offset = 0)
        {
            if (scene is null || scene.Count == 0) return (null, null);

            var sources = new AnalogSignalSource[N];

            foreach (var (thetta, signal) in scene)
            {
                var sources_i = GetSources(thetta - angle_offset, signal);
                for (var j = 0; j < N; j++)
                    sources[j] += sources_i[j];
            }

            var ss = GetSignalMatrix(sources);  // Определяем сигнальную матрицу на выходе АЦП всех элементов
            var SS = GetSpectralMatrix(ss);     // Получаем спектральную матрицу, как произведение ss*Wt
            var QQ = ComputeResultMatrix(SS);   // Диаграммообразование - доварачиваем спектр всех компонент спектральной матрицы с учётом сдвигов фаз
            var Q = SumRows(QQ);                // Складываем элементы столбцов получая строку - матрицу спектра выходного сигнала схемы ЦДО
            var q = ComputeResultSignal(Q);     // Вычисляем обратное преобразование Фурье для получение выходного сигнала

            var samples_p = new double[q.M];
            var samples_q = new double[q.M];

            for (var i = 0; i < samples_p.Length; i++)
            {
                samples_p[i] = q[0, i].Re;
                samples_q[i] = q[0, i].Im;
            }

            var dt = f_ADC[0].dt;
            return (new SamplesSignal(samples_p, dt), new SamplesSignal(samples_q, dt));
        }

        public async Task<(SamplesSignal P, SamplesSignal Q)> GetOutSignalAsync
        (
            RadioScene scene,
            double angle_offset = 0,
            IProgress<double> progress = null,
            CancellationToken cancel = default
        )
        {
            if (scene is null || scene.Count == 0) return (null, null);

            var sources = await Task.Run(() =>
            {
                var result = new AnalogSignalSource[N];

                foreach (var (thetta, signal) in scene)
                {
                    cancel.ThrowIfCancellationRequested();
                    var sources_i = GetSources(thetta - angle_offset, signal);
                    for (var j = 0; j < N; j++)
                        result[j] += sources_i[j];
                }

                return result;
            }, cancel).ConfigureAwait(false);

            var ss = await GetSignalMatrixAsync(sources, cancel).ConfigureAwait(false); // Определяем сигнальную матрицу на выходе АЦП всех элементов
            var SS = await GetSpectralMatrixAsync(ss, cancel).ConfigureAwait(false);     // Получаем спектральную матрицу, как произведение ss*Wt
            var QQ = await ComputeResultMatrixAsync(SS, cancel).ConfigureAwait(false);   // Диаграммообразование - доварачиваем спектр всех компонент спектральной матрицы с учётом сдвигов фаз
            var Q = await SumRowsAsync(QQ, cancel).ConfigureAwait(false);                // Складываем элементы столбцов получая строку - матрицу спектра выходного сигнала схемы ЦДО
            var q = await ComputeResultSignalAsync(Q, cancel).ConfigureAwait(false);     // Вычисляем обратное преобразование Фурье для получение выходного сигнала

            var samples_p = new double[q.M];
            var samples_q = new double[q.M];

            for (var i = 0; i < samples_p.Length; i++)
            {
                samples_p[i] = q[0, i].Re;
                samples_q[i] = q[0, i].Im;
            }

            var dt = f_ADC[0].dt;
            return (new SamplesSignal(samples_p, dt), new SamplesSignal(samples_q, dt));
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
