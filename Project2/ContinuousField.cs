using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using micfort.GHL.Math2;

namespace Project2
{
	class ContinuousField<T> where T : System.IComparable<T>
	{
		public CalculatingUnit<T>[] _data;
		public HyperPoint<int> _size;

		public ContinuousField(HyperPoint<int> size):this(size.p) { } 

		public ContinuousField(params int[] size)
		{
			int totalSize = 1;
			for (int i = 0; i < size.Length; i++)
			{
				totalSize *= size[i];
			}
			_data = new CalculatingUnit<T>[totalSize];
			_size = size;
		}
		
		/// <summary>
		/// A copy will be made of the array
		/// </summary>
		/// <param name="size"></param>
		/// <param name="data"></param>
		public ContinuousField(HyperPoint<int> size, T[] data)
		{
			_size = size;
			for (int i = 0; i < data.Length; i++)
			{
				_data[i] = data[i];
			}
		}

		public T GetValue(HyperPoint<float> position)
		{
			return GetValue(position.p);
		}

		public T GetValue(float[] position)
		{
			BasicOperationsProvider<float> bop = BasicOperationsProvider<float>.GetBasicOperationsProvider();
			BasicOperationsProvider<T> bop2 = BasicOperationsProvider<T>.GetBasicOperationsProvider();

			CalculatingUnit<int> i0, j0, i1, j1;
			CalculatingUnit<float> x, y;
			CalculatingUnit<T> s0, t0, s1, t1;

			CalculatingUnit<float> half = 0.5f;

			x = position[0];
			y = position[1];
			if (x < half) x = half;
			if (x > _size.X + half) x = _size.X + half;
			i0 = bop.ConvertTo<int>(x);
			i1 = i0 + 1;

			if (y < half) y = half;
			if (y > _size.Y + half) y = _size.Y + half;
			j0 = bop.ConvertTo<int>(y);
			j1 = j0 + 1;

			s1 = bop.ConvertTo<T>(x - i0);
			s0 = bop2.One() - s1;
			t1 = bop.ConvertTo<T>(y - j0);
			t0 = bop2.One() - t1;
			return s0 * (t0 * GetCU(i0, j0) + t1 * GetCU(i0, j1)) +
				   s1 * (t0 * GetCU(i1, j0) + t1 * GetCU(i1, j1));
		}

		public T this[HyperPoint<int> position]
		{
			get { return this[position.p]; }
			set { this[position.p] = value; }
		}

		public T this[params int[] position]
		{
			get { return _data[CalculateIndex(position)]; }
			set { _data[CalculateIndex(position)] = value; }
		}

		public T this[HyperPoint<float> position]
		{
			get { return GetValue(position); }
		}

		public T this[params float[] position]
		{
			get { return GetValue(position); }
		}

		public CalculatingUnit<T> GetCU(HyperPoint<int> position)
		{
			return _data[CalculateIndex(position)];
		}

		public CalculatingUnit<T> GetCU(params int[] position)
		{
			return _data[CalculateIndex(position)];
		}

		public void SetCU(HyperPoint<int> position, CalculatingUnit<T> value)
		{
			_data[CalculateIndex(position)] = value;
		}

		public void SetCU(CalculatingUnit<T> value, params int[] position)
		{
			_data[CalculateIndex(position)] = value;
		}
		
		public int CalculateIndex(HyperPoint<int> position)
		{
			return CalculateIndex(position.p);
		}

		public int CalculateIndex(params int[] position)
		{
			int index = 0;
			int dimSize = 1;
			for (int i = 0; i < position.Length; i++)
			{
				index += position[i] * dimSize;
				dimSize *= _size[i];
			}
			return index;
		}
	}
}
