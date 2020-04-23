using System;
using System.Collections.Generic;
using MathService;
using MathService.Annotations;

namespace Antennas
{
    //public class PatternManager
    //{
    //    [NotNull] private readonly Antenna f_Antenna;
    //    private readonly double f_f0;
    //    private readonly double f_Th1;
    //    private readonly double f_Th2;
    //    private readonly double f_dth0;
    //    private readonly double f_Phi;

    //    private ObservableLinkedList<PatternValue> f_Values = new ObservableLinkedList<PatternValue>();

    //    public double Th1 => f_Th1;
    //    public double Th2 => f_Th2;
    //    public double Phi => f_Phi;

    //    public double F0 => f_f0;

    //    public ObservableLinkedList<PatternValue> Values => f_Values; 

    //    public PatternManager([NotNull] Antenna antenna, double f0,
    //        double phi = 0, double th1 = Consts.pi_neg, double th2 = Consts.pi, double dth0 = 1 * Consts.ToRad,
    //        double eps_db = -30)
    //    {
    //        if (eps_db > 0) throw new ArgumentOutOfRangeException(nameof(eps_db), @"Значение точности в дБ должно быть меньше 0");
    //        if(f0 <= 0) throw new ArgumentOutOfRangeException(nameof(f0), @"Частота должна быть больше 0");

    //        f_Antenna = antenna ?? throw new ArgumentNullException(nameof(antenna));
    //        f_f0 = f0;
    //        f_Th1 = th1;
    //        f_Th2 = th2;
    //        f_dth0 = dth0;
    //        f_Phi = phi;
    //    }
    //}
}
