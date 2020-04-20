using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MathService;
using MathService.Annotations;
using MathService.Values;
using MathService.Vectors;

namespace Antennas
{
    /// <summary>Антенная решётка</summary>
    public class AntennaArray : Antenna, IList<AntennaItem>
    {
        /// <summary>Создать линейную антенную решётку</summary>
        /// <param name="antennas">Перечисление антенных элементов</param>
        /// <param name="d">Шаг между элементами</param>
        /// <returns><see cref="AntennaArray"/>></returns>
        [NotNull]
        public static AntennaArray CreateLinearArray([NotNull]IEnumerable<Antenna> antennas, double d)
        {
            Contract.Requires(antennas != null);
            Contract.Requires(d > 0);
            Contract.Ensures(Contract.Result<AntennaArray>() is LinearAntennaArray);
            Contract.Ensures(Contract.Result<AntennaArray>() != null);

            return new LinearAntennaArray(antennas, d);
        }

        /// <summary>Создать линейную антенную решётку</summary>
        /// <param name="antennas">Массив антенных элементов</param>
        /// <param name="step">Шаг размещения</param>
        /// <param name="A">Амплитудно-фазовое распределение</param>
        /// <returns>Антенная решётка с линейно размещёнными в пространстве антенными эламентами</returns>
        public static AntennaArray CreateLinearArray(Antenna[] antennas, double step, Func<Vector3D, Complex> A = null)
        {
            Contract.Requires(antennas != null);
            Contract.Ensures(Contract.Result<AntennaArray>() != null);
            Contract.Ensures(Contract.Result<AntennaArray>().Count == antennas.Length);
            var n = antennas.Length;
            var l05 = step * (n - 1) / 2;
            if(A == null) A = v => 1;
            var items = antennas.Select((a, i) => new { a, v = new Vector3D(i * step - l05), o = new SpaceAngle() })
                        .Select(a => new { a.a, a.v, a.o, A = A(a.v) })
                        .Select(a => new AntennaItem(a.a, a.v, a.o, a.A))
                        .ToArray();

            return new AntennaArray(items);
        }

        /// <summary>Создать плоскую антенную решётку</summary>
        /// <param name="antennas">Массив антенных элементов</param>
        /// <param name="dx">Шаг по оси OX</param>
        /// <param name="dy">ШАг по оси OY</param>
        /// <returns>Антенная решётка с размещеинем антеных элементов в плоскости XOY</returns>
        public static AntennaArray CreateFlatArray(Antenna[,] antennas, double dx, double dy)
        {
            Contract.Requires(antennas != null);
            Contract.Ensures(Contract.Result<AntennaArray>() != null);
            Contract.Ensures(Contract.Result<AntennaArray>().Count == antennas.Length);
            var nx = antennas.GetLength(0);
            var ny = antennas.GetLength(1);
            var lx05 = dx * (nx - 1);
            var ly05 = dy * (ny - 1);
            var items = new AntennaItem[nx * ny];
            for(int i = 0, ii = 0; i < nx; i++)
                for(var j = 0; j < ny; j++)
                    items[ii++] = new AntennaItem(antennas[i, j], new Vector3D(i * dx - lx05, j * dy - ly05), new SpaceAngle(), 1);
            return new AntennaArray(items);
        }

        /// <summary>Список антенных элементов</summary>
        [NotNull]
        private AntennaItem[] f_Items;

        /// <summary>Размер апертуры</summary>
        public Vector3D AperturaLenght
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Contract.Ensures(Contract.Result<Vector3D>().X >= 0);
                Contract.Ensures(Contract.Result<Vector3D>().Y >= 0);
                Contract.Ensures(Contract.Result<Vector3D>().Z >= 0);
                Contract.Ensures(Contract.Result<Vector3D>().R >= 0);
                return f_Items.Aggregate(new { X = new MinMaxValue(), Y = new MinMaxValue(), Z = new MinMaxValue() },
                    (R, i) =>
                    {
                        var r = i.Location;
                        R.X.AddValue(r.X);
                        R.Y.AddValue(r.Y);
                        R.Z.AddValue(r.Z);
                        return R;
                    },
                    R => new Vector3D(R.X.Interval.Length, R.Y.Interval.Length, R.Z.Interval.Length));
            }
        }

        /// <summary>Размер апертуры по оси OX</summary>
        public double L_x
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() >= 0);
                return f_Items.Aggregate(new MinMaxValue(), (I, i) =>
                {
                    I.AddValue(i.Location.X);
                    return I;
                }, I => I.Interval.Length);
            }
        }

        /// <summary>Размер апертуры по оси OY</summary>
        public double L_y
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() >= 0);
                return f_Items.Aggregate(new MinMaxValue(), (I, i) =>
                {
                    I.AddValue(i.Location.Y);
                    return I;
                }, I => I.Interval.Length);
            }
        }

        /// <summary>Размер апертуры по оси OZ</summary>
        public double L_z
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() >= 0);
                return f_Items.Aggregate(new MinMaxValue(), (I, i) =>
                {
                    I.AddValue(i.Location.Z);
                    return I;
                }, I => I.Interval.Length);
            }
        }

        /// <summary>нициализация новой антенной решётки</summary>
        /// <param name="items">Перечисление антенных элементов</param>
        public AntennaArray(IEnumerable<AntennaItem> items) => f_Items = items.ToArray();

        [ContractInvariantMethod]
        private void ObjectInvariant() => Contract.Invariant(f_Items != null);

        /// <summary>Диаграмма направленности антенной решётки</summary>
        /// <param name="Direction">Пространственный угол</param>
        /// <param name="f">Частота</param>
        /// <returns>Комплексное значение ДН</returns>
        public override Complex Pattern(SpaceAngle Direction, double f) => f_Items.Length == 0 ? Complex.NaN : f_Items.AsParallel().Sum(i => i.Pattern(Direction, f));

        public double GetBeamPatternWidthX_deg(double lamda) => 51 * lamda / L_x;
        public double GetBeamPatternWidthY_deg(double lamda) => 51 * lamda / L_y;
        public double GetBeamPatternWidthZ_deg(double lamda) => 51 * lamda / L_z;


        public void SaveToFile(string FileName)
        {
            var info = new FileInfo(FileName);
            using(var writer = info.CreateText())
            {
                var title = new[]
                {
                    "#",
                    "X",
                    "Y",
                    "Z",
                    "Thetta",
                    "Phi",
                    "A",
                    "Phase",
                    "Element"
                };
                writer.WriteLine(title.ToSeparatedStr("\t"));
                var elements = this as IEnumerable<AntennaItem>;
                elements.Select((e, i) => new { i, e.Location, Direction = e.Direction.InDeg, e.K, e.Element })
                    .Select(e => new
                    {
                        e.i,
                        e.Location.X,
                        e.Location.Y,
                        e.Location.Z,
                        e.Direction.Thetta,
                        e.Direction.Phi,
                        A = e.K.Abs.In_dB(),
                        Phase = e.K.Arg.ToDeg(),
                        Element = e.Element.GetType().Name
                    })
                    .Select(e => new object[] { e.i, e.X, e.Y, e.Z, e.Thetta, e.Phi, e.A, e.Phase, e.Element })
                    .Select(e => e.ToSeparatedStr("\t"))
                    .Foreach(writer.WriteLine);
            }
        }

        #region IList<AntennaArray> implementation
        /// <summary>Возвращает перечислитель, выполняющий итерацию в коллекции.</summary>
        /// <returns>
        /// Интерфейс <see cref="T:System.Collections.Generic.IEnumerator`1"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<AntennaItem> GetEnumerator() => f_Items.Cast<AntennaItem>().GetEnumerator();

        /// <summary>Возвращает перечислитель, который осуществляет перебор элементов коллекции.</summary>
        /// <returns>
        /// Объект <see cref="T:System.Collections.IEnumerator"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Добавляет элемент в интерфейс <see cref="T:System.Collections.Generic.ICollection`1"/>.</summary>
        /// <param name="item">Объект, добавляемый в интерфейс <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">Объект <see cref="T:System.Collections.Generic.ICollection`1"/> доступен только для чтения.</exception>
        public virtual void Add(AntennaItem item)
        {
            Contract.Requires(item?.Element != null);
            if(ReferenceEquals(item?.Element, this))
                throw new ArgumentException(@"Произведена попытка добавить саму антенную решётку к себе в список антенных элементов", nameof(item));
            var new_items = new AntennaItem[f_Items.Length + 1];
            Array.Copy(f_Items, new_items, f_Items.Length);
            new_items[new_items.Length - 1] = item;
            f_Items = new_items;
        }

        /// <summary>Удаляет все элементы из интерфейса <see cref="T:System.Collections.Generic.ICollection`1"/>.</summary>
        public virtual void Clear()
        {
            Contract.Ensures(Count == 0);
            f_Items = new AntennaItem[0];
        }

        /// <summary>Определяет, содержит ли интерфейс <see cref="T:System.Collections.Generic.ICollection`1"/> указанное значение.</summary>
        /// <returns>
        /// Значение true, если объект <paramref name="item"/> найден в <see cref="T:System.Collections.Generic.ICollection`1"/>; в противном случае — значение false.
        /// </returns>
        /// <param name="item">Объект, который требуется найти в <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        [System.Diagnostics.Contracts.Pure]
        public virtual bool Contains(AntennaItem item) => item != null && f_Items.Contains(item);

        /// <summary>
        /// Копирует элементы <see cref="T:System.Collections.Generic.ICollection`1"/> в массив <see cref="T:System.Array"/>, начиная с указанного индекса <see cref="T:System.Array"/>.
        /// </summary>
        /// <param name="array">Одномерный массив <see cref="T:System.Array"/>, в который копируются элементы из интерфейса <see cref="T:System.Collections.Generic.ICollection`1"/>.Индексация в массиве <see cref="T:System.Array"/> должна начинаться с нуля.</param><param name="index">Отсчитываемый от нуля индекс в массиве <paramref name="array"/>, с которого начинается копирование.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> имеет значение null;</exception><exception cref="T:System.ArgumentOutOfRangeException">Значение параметра <paramref name="index"/> меньше 0.</exception><exception cref="T:System.ArgumentException">Параметр <paramref name="array"/> является многомерным— или —Количество элементов в исходной коллекции <see cref="T:System.Collections.Generic.ICollection`1"/> превышает доступное место, начиная с индекса <paramref name="index"/> до конца массива назначения <paramref name="array"/>.— или —Тип <paramref name="T"/> не может быть автоматически приведен к типу массива назначения <paramref name="array"/>.</exception>
        public virtual void CopyTo(AntennaItem[] array, int index) => f_Items.CopyTo(array, index);

        /// <summary>Удаляет первое вхождение указанного объекта из интерфейса <see cref="T:System.Collections.Generic.ICollection`1"/>.</summary>
        /// <returns>
        /// Значение true, если объект <paramref name="item"/> успешно удален из <see cref="T:System.Collections.Generic.ICollection`1"/>, в противном случае — значение false.Этот метод также возвращает значение false, если параметр <paramref name="item"/> не найден в исходном интерфейсе <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        /// <param name="item">Объект, который необходимо удалить из интерфейса <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">Объект <see cref="T:System.Collections.Generic.ICollection`1"/> доступен только для чтения.</exception>
        public virtual bool Remove(AntennaItem item)
        {
            Contract.Ensures(Contract.Result<bool>() && !Contains(item));
            var new_items = f_Items.ToList();
            var result = new_items.Remove(item);
            if (result) f_Items = new_items.ToArray();
            return result;
        }

        /// <summary>Получает число элементов, содержащихся в интерфейсе <see cref="T:System.Collections.Generic.ICollection`1"/>.</summary>
        /// <returns>Число элементов, содержащихся в интерфейсе <see cref="T:System.Collections.Generic.ICollection`1"/>.</returns>
        public virtual int Count => f_Items.Length;

        /// <summary>
        /// Получает значение, указывающее, доступен ли интерфейс <see cref="T:System.Collections.Generic.ICollection`1"/> только для чтения.
        /// </summary>
        /// <returns>
        /// Значение true, если интерфейс <see cref="T:System.Collections.Generic.ICollection`1"/> доступен только для чтения, в противном случае — значение false.
        /// </returns>
        bool ICollection<AntennaItem>.IsReadOnly => false;

        /// <summary>
        /// Определяет индекс заданного элемента коллекции <see cref="T:System.Collections.Generic.IList`1"/>.
        /// </summary>
        /// <returns>Индекс <paramref name="item"/> если он найден в списке; в противном случае его значение равно -1.</returns>
        /// <param name="item">Объект, который требуется найти в <see cref="T:System.Collections.Generic.IList`1"/>.</param>
        public virtual int IndexOf(AntennaItem item)
        {
            Contract.Requires(item != null);
            return Array.IndexOf(f_Items, item);
        }

        /// <summary>Вставляет элемент в список <see cref="T:System.Collections.Generic.IList`1"/> по указанному индексу.</summary>
        /// <param name="index">Индекс (с нуля), по которому следует вставить параметр <paramref name="item"/>.</param><param name="item">Объект, вставляемый в <see cref="T:System.Collections.Generic.IList`1"/>.</param><exception cref="T:System.ArgumentOutOfRangeException">Значение параметра <paramref name="index"/> не является допустимым индексом в <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">Объект <see cref="T:System.Collections.Generic.IList`1"/> доступен только для чтения.</exception>
        public void Insert(int index, AntennaItem item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contains(item));
            Contract.Ensures(IndexOf(item) == index);
            if(ReferenceEquals(item.Element, this) || Array.Exists(f_Items, i => ReferenceEquals(i, item)))
                throw new InvalidOperationException();
            var items = f_Items.ToList();
            items.Insert(index, item);
            f_Items = items.ToArray();
        }

        /// <summary>Удаляет элемент <see cref="T:System.Collections.Generic.IList`1"/> по указанному индексу.</summary>
        /// <param name="index">Индекс (с нуля) удаляемого элемента.</param><exception cref="T:System.ArgumentOutOfRangeException">Значение параметра <paramref name="index"/> не является допустимым индексом в <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">Объект <see cref="T:System.Collections.Generic.IList`1"/> доступен только для чтения.</exception>
        public virtual void RemoveAt(int index)
        {
            var new_items = f_Items.ToList();
            new_items.RemoveAt(index);
            f_Items = new_items.ToArray();
        }

        /// <summary>Получает или задает элемент по указанному индексу.</summary>
        /// <returns>Элемент с указанным индексом.</returns>
        /// <param name="index">Индекс (с нуля) элемента, который необходимо получить или задать.</param><exception cref="T:System.ArgumentOutOfRangeException">Значение параметра <paramref name="index"/> не является допустимым индексом в <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">Свойство задано, и объект <see cref="T:System.Collections.Generic.IList`1"/> доступен только для чтения.</exception>
        [NotNull]
        public virtual AntennaItem this[int index]
        {
            get
            {
                Contract.Requires(index >= 0);
                Contract.Requires(index < Count);
                Contract.Ensures(Contract.Result<AntennaItem>() != null);
                return f_Items[index];
            }
            set
            {
                Contract.Requires(index >= 0);
                Contract.Requires(index < Count);
                f_Items[index] = value;
            }
        }
        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddRange([NotNull]IEnumerable<AntennaItem> collection)
        {
            collection = collection.ForeachLazy(i =>
            {
                if(ReferenceEquals(i.Element, this))
                    throw new ArgumentException(@"Произведена попытка добавить саму антенную решётку к себе в список антенных элементов", nameof(collection));
            });
            var to_add = collection.ToArray();
            var new_items = new AntennaItem[f_Items.Length + to_add.Length];
            Array.Copy(f_Items, new_items, f_Items.Length);
            Array.Copy(to_add, 0, new_items, f_Items.Length, new_items.Length);
            f_Items = new_items;
        }

        #region Overrides of Antenna

        [MethodImpl(MethodImplOptions.AggressiveInlining), NotNull]
        private static BinaryExpression Add([NotNull]Expression a, [NotNull]Expression b)
        {
            Contract.Requires(a != null);
            Contract.Requires(b != null);
            Contract.Ensures(Contract.Result<BinaryExpression>() != null);
            return Expression.Add(a, b);
        }

        public override Expression GetPatternExpressionBody(Expression a, Expression f)
        {
            Contract.Ensures(Contract.Result<Expression>() != null);
            return this
                .Select(i => i.GetPatternExpressionBody(a, f))
                .Aggregate((x, y) => x == null ? y : Add(x, y));
        }

        #endregion
    }
}