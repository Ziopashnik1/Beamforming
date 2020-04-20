using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BeamServiceTests
{
    [TestClass]
    public class DigitalAntennaArray_Tests
    {
        private const double toRad = Math.PI / 180;

        //[TestMethod]
        //public void ComputePatternValue_MainDirection_Test()
        //{
        //    const int N = 8;        // Число элементов
        //    const double d = 0.15;  // Шаг между элементами в метрах
        //    const double fd = 8e9;  // Частота дискретизации в Гц
        //    const int n = 8;        // Разряднооость кода в битах
        //    const int Nd = 8;       // Размер выборки цифрового сигнала
        //    const double MaxValue = 2; // Максимальная амплитуда АЦП
        //    var array = new DigitalAntennaArray(N, d, fd, n, Nd, MaxValue);

        //    const double th = 0;
        //    const double f0 = 1e9;
        //    var F = array.ComputePatternValue(th, f0); // Расчёт ДН для одного уголового положения

        //    const double P0 = N * N / 2;
        //    Assert.AreEqual(P0, F, 0.002 * P0);
        //}

        //[TestMethod, Description("Тестирование ДН для угла падения 15 градусов"), Timeout(1000)]
        //public void ComputePatternValue_th0_15deg_Test()
        //{
        //    const int N = 8;        // Число элементов
        //    const double d = 0.15;  // Шаг между элементами в метрах
        //    const double fd = 8e9;  // Частота дискретизации в Гц
        //    const int n = 8;        // Разряднооость кода в битах
        //    const int Nd = 8;       // Размер выборки цифрового сигнала
        //    const double MaxValue = 2; // Максимальная амплитуда АЦП
        //    var array = new DigitalAntennaArray(N, d, fd, n, Nd, MaxValue);

        //    const double th = 15 * toRad;
        //    array.th0 = th;
        //    const double f0 = 1e9;
        //    var F = array.ComputePatternValue(th, f0); // Расчёт ДН для одного уголового положения

        //    const double P0 = N * N / 2;
        //    Assert.AreEqual(P0, F, 0.002 * P0);
        //}
    }
}
