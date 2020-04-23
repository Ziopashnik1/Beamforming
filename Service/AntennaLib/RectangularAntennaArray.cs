using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using MathCore;
using MathCore.Vectors;

namespace Antennas
{
    public delegate Complex Distribution(double x, double y);

    /// <summary>Прямоугольная плоская антенная решётка</summary>
    public class RectangularAntennaArray : AntennaArray
    {
        /// <summary>Метод инициализации элементов решётки</summary>
        /// <param name="Nx">Число элементов по оси X</param>
        /// <param name="Ny">Число элементов по оси Y</param>
        /// <param name="dx">Шаг между элементами по оси X</param>
        /// <param name="dy">Шаг между элемнтами по оси Y</param>
        /// <param name="Element">Антенный элемент</param>
        /// <param name="Distribution">Распределение</param>
        /// <returns>Перечисление антенных элементов решётки</returns>
        private static IEnumerable<AntennaItem> Initialize(int Nx, int Ny, double dx, double dy, Antenna Element, Distribution Distribution)
        {
            Contract.Requires(Nx > 0);
            Contract.Requires(Ny > 0);
            Contract.Requires(dx > 0);
            Contract.Requires(dy > 0);
            Contract.Requires(Element != null);
            Contract.Requires(Distribution != null);

            var Lx = (Nx - 1) * dx;
            var Ly = (Ny - 1) * dy;
            var x0 = Lx / 2;
            var y0 = Ly / 2;

            var angle = new SpaceAngle();
            for(var ix = 0; ix < Nx; ix++)
                for(var iy = 0; iy < Ny; iy++)
                {
                    var x = ix * dx - x0;
                    var y = iy * dy - y0;
                    var k = Distribution(x, y);
                    yield return new AntennaItem(Element, new Vector3D(x, y), angle, k);
                }
        }

        /// <summary>Число элементов по оси X</summary>
        private int f_Nx;
        /// <summary>Число элементов по оси Y</summary>
        private int f_Ny;
        /// <summary>Шаг между элементами по оси X</summary>
        private double f_dx;
        /// <summary>Шаг между элементами по оси Y</summary>
        private double f_dy;
        /// <summary>Антенный элемент</summary>
        private Antenna f_Element;
        /// <summary>Распределение</summary>
        private Distribution f_Distribution = (x, y) => 1;


        /// <summary>Число элементов по оси X</summary>
        public int Nx
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return f_Nx;
            }
            set
            {
                Contract.Requires(value > 0);
                Contract.Ensures(f_Nx > 0);
                if(value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "value <= 0");
                if(f_Nx == value) return;
                f_Nx = value;
                RefreshConstruction();
            }
        }

        /// <summary>Число элементов по оси Y</summary>
        public int Ny
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return f_Ny;
            }
            set
            {
                Contract.Requires(value > 0);
                Contract.Ensures(f_Ny > 0);
                if(value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "value <= 0");
                if(f_Ny == value) return;
                f_Ny = value;
                RefreshConstruction();
            }
        }

        /// <summary>Шаг между элементами по оси X</summary>
        public double dx
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() > 0);
                return f_dx;
            }
            set
            {
                Contract.Requires(value > 0);
                Contract.Ensures(f_dx > 0);
                if(value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "value <= 0");
                if(f_dx.Equals(value)) return;
                f_dx = value;
                RefreshGeometry();
            }
        }

        /// <summary>Шаг между элементами по оси Y</summary>
        public double dy
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() > 0);
                return f_dy;
            }
            set
            {
                Contract.Requires(value > 0);
                Contract.Ensures(f_dy > 0);
                if(value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "value <= 0");
                if(f_dy.Equals(value)) return;
                f_dy = value;
                RefreshGeometry();
            }
        }

        /// <summary>Антенный элемент</summary>
        public Antenna Element
        {
            get
            {
                Contract.Ensures(Contract.Result<Antenna>() != null);
                return f_Element;
            }
            set
            {
                Contract.Requires(value != null);
                Contract.Ensures(f_Element != null);
                if(ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
                if(ReferenceEquals(f_Element, value)) return;
                f_Element = value;
                for(var i = 0; i < Count; i++) this[i].Element = value;
            }
        }

        /// <summary>Распределение</summary>
        public Distribution Distribution
        {
            get
            {
                Contract.Ensures(Contract.Result<Distribution>() != null);
                return f_Distribution;
            }
            set
            {
                Contract.Requires(value != null);
                Contract.Ensures(f_Distribution != null);
                if(ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
                if(ReferenceEquals(f_Distribution, value)) return;
                f_Distribution = value;
                for(var i = 0; i < Count; i++)
                {
                    var a = this[i];
                    var location = a.Location;
                    a.K = f_Distribution(location.X, location.Y);
                }
            }
        }

        /// <summary>Инициализация прямоугольной плоской антенной решётки</summary>
        /// <param name="Nx"></param>
        /// <param name="Ny"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="Element"></param>
        /// <param name="Distribution"></param>
        public RectangularAntennaArray(int Nx, int Ny, double dx, double dy, Antenna Element, Distribution Distribution)
            : base(Initialize(Nx, Ny, dx, dy, Element, Distribution))
        {
            Contract.Requires(Nx > 0);
            Contract.Requires(Ny > 0);
            Contract.Requires(dx > 0);
            Contract.Requires(dy > 0);
            Contract.Requires(Element != null);
            Contract.Requires(Distribution != null);

            Contract.Ensures(f_Nx > 0);
            Contract.Ensures(f_Ny > 0);
            Contract.Ensures(f_dx > 0);
            Contract.Ensures(f_dy > 0);
            Contract.Ensures(f_Element != null);
            Contract.Ensures(f_Distribution != null);

            f_Nx = Nx;
            f_Ny = Ny;
            f_dx = dx;
            f_dy = dy;
            f_Element = Element;
            f_Distribution = Distribution;
        }

        private void RefreshConstruction()
        {
            Clear();
            AddRange(Initialize(f_Nx, f_Ny, f_dx, f_dy, f_Element, f_Distribution));
        }

        private void RefreshGeometry()
        {
            var Nx = f_Nx;
            var Ny = f_Ny;
            var dx = f_dx;
            var dy = f_dy;
            var A = f_Distribution;

            var Lx = (Nx - 1) * dx;
            var Ly = (Ny - 1) * dy;
            var x0 = Lx / 2;
            var y0 = Ly / 2;

            for(var ix = 0; ix < Nx; ix++)
                for(var iy = 0; iy < Ny; iy++)
                {
                    var i = Ny * ix + iy;
                    var a = this[i];
                    var x = ix * dx - x0;
                    var y = iy * dy - y0;
                    a.Location = new Vector3D(x, y);
                    a.K = A(x, y);
                }
        }

        [ContractInvariantMethod]
        private void ContractInvariantMethod()
        {
            Contract.Invariant(f_Nx > 0);
            Contract.Invariant(f_Ny > 0);
            Contract.Invariant(f_dx > 0);
            Contract.Invariant(f_dy > 0);
            Contract.Invariant(f_Element != null);
            Contract.Invariant(f_Distribution != null);
        }
    }
}