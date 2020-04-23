using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MathCore;
using MathCore.Extensions.Expressions;
using MathCore.Vectors;

namespace Antennas
{
    /// <summary>Антенный элемент</summary>
    public class AntennaItem : Antenna
    {
        private const double c_ToRad = Math.PI / 180;
        private const double pi_c = Consts.pi2 / Consts.SpeedOfLight;

        private string f_Name;
        private SpaceAngle f_Direction;
        private Vector3D f_Location;
        private Antenna f_Element;
        private Complex f_K;
        private Func<SpaceAngle, SpaceAngle> f_Rotator;

        public string Name { get => f_Name; set => Set(ref f_Name, value); }

        /// <summary>Антенный элемент</summary>
        public Antenna Element
        {
            get => f_Element;
            set
            {
                if(Equals(f_Element, value)) return;
                {
                    if (f_Element is INotifyPropertyChanged property_changed_obj)
                        property_changed_obj.PropertyChanged -= OnElementPropertyChanged;
                }   
                f_Element = value;
                {
                    if (value is INotifyPropertyChanged property_changed_obj)
                        property_changed_obj.PropertyChanged += OnElementPropertyChanged;
                }
                OnPropertyChanged();
            }
        }

        private void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args) => OnPropertyChanged(nameof(Element)); 

        /// <summary>Вектор расположения антеннойго элемента решётки относительно её фазового центра</summary>
        public Vector3D Location { get => f_Location; set => Set(ref f_Location, value); }

        public double LocationX { get => f_Location.X; set => Location = new Vector3D(value, LocationY, LocationZ); }

        public double LocationY { get => f_Location.Y; set => Location = new Vector3D(LocationX, value, LocationZ); }

        public double LocationZ { get => f_Location.Z; set => Location = new Vector3D(LocationX, LocationY, value); }

        /// <summary>Пространственный угол поворота антенного элемента относительно фазового центра антенного элемента</summary>
        public SpaceAngle Direction
        {
            get => f_Direction;
            set
            {
                if (f_Direction.Equals(value)) return;
                f_Direction = value;
                f_Rotator = value.Theta.Equals(0d) && value.Phi.Equals(0d)
                    ? null
                    : value.GetRotatorPhiTheta();
                OnPropertyChanged();
            }
        }

        public double Theta { get => f_Direction.InRad.Theta; set => Direction = new SpaceAngle(value, Phi); }

        public double ThetaDeg { get => Theta / c_ToRad; set => Theta = value * c_ToRad; }

        public double Phi { get => f_Direction.InRad.Phi; set => Direction = new SpaceAngle(Theta, value); }

        public double PhiDeg { get => Phi / c_ToRad; set => Phi = value * c_ToRad; }

        /// <summary>Комплексный коэффициент передачи антенного эелмента</summary>
        public Complex K { get => f_K; set => Set(ref f_K, value); }

        public double absK { get => K.Abs; set => K = Complex.Exp(value, argK); }
        public double argK { get => K.Abs; set => K = Complex.Exp(absK, value); }
        public double reK { get => K.Re; set => K = new Complex(value, imK); }
        public double imK { get => K.Im; set => K = new Complex(reK, value); }

        public AntennaItem() : this(new UniformAntenna(), Vector3D.Empty, SpaceAngle.k, Complex.Real) { }

        /// <summary>Инициализация нового антеннойго элемента антенной решётки</summary>
        /// <param name="a">Антенный элемент</param>
        /// <param name="r">Вектор размещения</param>
        /// <param name="angle">Угол поворота</param>
        /// <param name="k">Комплексный коэффициент передачи</param>
        public AntennaItem(Antenna a, Vector3D r, SpaceAngle angle, Complex k)
        {
            Contract.Requires(a != null);
            Contract.Ensures(Element != null);

            PropertyDependence_Add(nameof(Location), nameof(LocationX), nameof(LocationY), nameof(LocationZ));
            PropertyDependence_Add(nameof(Direction), nameof(Theta), nameof(ThetaDeg), nameof(Phi), nameof(PhiDeg));
            PropertyDependence_Add(nameof(K), nameof(absK), nameof(argK), nameof(reK), nameof(imK));

            f_Element = a;
            f_Location = r;
            Direction = angle;
            K = k;
        }

        /// <summary>Диаграмма направленности элемента относительно фазового центра решётки</summary>
        /// <param name="a">Направление</param>
        /// <param name="f">Частота</param>
        /// <returns>Комплексное значение диаграммы направленности в указанном направлении на указанной частоте</returns>
        public override Complex Pattern(SpaceAngle a, double f)
        {
            if (f_Rotator != null) a = f_Rotator(a);
            return f_K
                * f_Element.Pattern(a, f)
                * Complex.Exp(-f_Location.GetProjectionTo(a) * pi_c * f);
        }

        #region Overrides of Antenna

        private static BinaryExpression M(Expression a, Expression b) => Expression.Multiply(a, b);
        private static BinaryExpression M(Expression a, Expression b, Expression c) => M(a, M(b, c));
        private static Expression Const(object value) => Expression.Constant(value);
        private static MethodCallExpression Call(Delegate d, Expression arg) => Expression.Call(d.Method, arg);
        private static MethodCallExpression Call<T>(T obj, MethodInfo method, params Expression[] arg) => Expression.Call(Const(obj), method, arg);
        private static MethodCallExpression Call<T>(T obj, Delegate d, params Expression[] arg) => Expression.Call(Const(obj), d.Method, arg);

        public override Expression GetPatternExpressionBody(Expression a, Expression f)
        {
            if (f_Rotator != null) a = Expression.Invoke(f_Rotator.ToExpression(), a);
            var kl = f.Multiply(-pi_c);
            var projection_info = typeof(Vector3D).GetMethod(nameof(Vector3D.GetProjectionTo), new[] { typeof(SpaceAngle) }, null);
            var projection = Call(Location, projection_info, a);
            var kr = kl.Multiply(projection);
            var exp = ((Func<double, Complex>)Complex.Exp).GetCallExpression(kr);
            return f_Element.GetPatternExpressionBody(a, f).Multiply(f_K.ToExpression().Multiply(exp));
        }

        #endregion

        public override string ToString()
        {
            var result = new StringBuilder();
            var empty = true;
            var r = f_Location;
            if (r != 0)
            {
                empty = false;
                result.AppendFormat("[loc:{0}", r);
            }

            var a = f_Direction;
            var angle_empty = true;
            if (!a.IsZero)
            {
                if (!empty) result.Append(" - ");
                result.AppendFormat(empty ? "[angle:{0}]" : "angle:{0}]", a);
                angle_empty = empty = false;
            }

            if (!empty && angle_empty) result.Append("]");

            var k = f_K;
            if (k == 0) return empty ? $"{{{Element}}}" : $"{{{Element}}}{result}";
            return $"{{{Element}}}{result} x {k}";
        }
    }
}