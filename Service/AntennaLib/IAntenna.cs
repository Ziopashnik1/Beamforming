using System.Diagnostics.Contracts;
using MathCore;
using MathCore.Vectors;

namespace Antennas
{
    /// <summary>������ �� ��������� ������� �� �������</summary>
    [ContractClass(typeof(AntennaContract))]
    public interface IAntenna
    {
        /// <summary>��������� �������������� �������</summary>
        /// <param name="Direction">���������������� ����</param>
        /// <param name="f">�������</param>
        /// <returns>����������� �������� ��������� ��������������</returns>
        Complex Pattern(SpaceAngle Direction, double f);
    }

    /// <summary>�����-�������� ��� ���������� �������</summary>
    [ContractClassFor(typeof(IAntenna))]
    abstract class AntennaContract : IAntenna
    {
        /// <summary>��������� �������������� ����� ���� ��������� ���� ��� ������������� �������� �������</summary>
        /// <param name="Direction">���������������� ���� ����� ��������� ����� ��������</param>
        /// <param name="f">�������� ������� ������ ���� ������ ����, �� ����� NAN � �������������</param>
        /// <returns></returns>
        public Complex Pattern(SpaceAngle Direction, double f)
        {
            Contract.Requires(!double.IsNaN(f));
            Contract.Requires(f > 0);
            Contract.Requires(!double.IsInfinity(f));
            return default(Complex);
        }
    }
}