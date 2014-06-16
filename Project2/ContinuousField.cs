using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using micfort.GHL.Math2;

namespace Project2
{
	class ContinuousField
	{
		public float[] _data;
		public HyperPoint<int> _size;

		public ContinuousField(HyperPoint<int> size):this(size.p) { } 

		public ContinuousField(params int[] size)
		{
			int totalSize = 1;
			for (int i = 0; i < size.Length; i++)
			{
				totalSize *= size[i];
			}
			_data = new float[totalSize];
			_size = new HyperPoint<int>(size);
		}
		
		/// <summary>
		/// No copy will be made of the data
		/// </summary>
		/// <param name="size"></param>
		/// <param name="data"></param>
		public ContinuousField(HyperPoint<int> size, float[] data)
		{
			_size = size;
			_data = data;
		}

		public float GetValue(HyperPoint<float> position)
		{
			return GetValue(position.p);
		}

		public float GetValue(float[] position)
		{
			CalculatingUnit<int> i0, j0, i1, j1;
			float x, y, s0, t0, s1, t1;

			x = position[0];
			y = position[1];
			if (x < 0.5f) x = 0.5f;
			if (x > _size.X + 0.5f) x = _size.X + 0.5f;
			i0 = (int)x;
			i1 = i0 + 1;

			if (y < 0.5f) y = 0.5f;
			if (y > _size.Y + 0.5f) y = _size.Y + 0.5f;
			j0 = (int)y;
			j1 = j0 + 1;

			s1 = x - i0;
			s0 = 1 - s1;
			t1 = y - j0;
			t0 = 1 - t1;
			return s0 * (t0 * this[i0, j0] + t1 * this[i0, j1]) +
				   s1 * (t0 * this[i1, j0] + t1 * this[i1, j1]);
		}

		public float this[HyperPoint<int> position]
		{
			get { return this[position.p]; }
			set { this[position.p] = value; }
		}

		public float this[params int[] position]
		{
			get { return _data[CalculateIndex(position)]; }
			set { _data[CalculateIndex(position)] = value; }
		}

		public float this[HyperPoint<float> position]
		{
			get { return GetValue(position); }
		}

		public float this[params float[] position]
		{
			get { return GetValue(position); }
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
