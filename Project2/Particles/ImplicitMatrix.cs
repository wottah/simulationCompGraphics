using System;
using System.Collections.Generic;
using micfort.GHL.Math2;

namespace Project2.Particles
{
	
	public struct ImplicitMatrix<T>
		where T : IComparable<T>
	{
		private struct MatrixItem
		{
			public int row, column;
			public CalculatingUnit<T> value;
		}

		private static BasicOperationsProvider<T> bop = BasicOperationsProvider<T>.GetBasicOperationsProvider();

		private int Columns, Rows;
		private List<List<MatrixItem>> mRf;
		private List<List<MatrixItem>> mCf;

		public ImplicitMatrix(int row, int columns)
		{
			Columns = columns;
			Rows = row;

			mRf = new List<List<MatrixItem>>();
			mCf = new List<List<MatrixItem>>();
			for (int i = 0; i < Rows; i++)
			{
				mRf.Add(new List<MatrixItem>());
			}
			for (int i = 0; i < columns; i++)
			{
				mCf.Add(new List<MatrixItem>());
			}
		}

		public T this[int row, int column]
		{
			get
			{
				if (mRf[row].Exists(x => x.column == column))
					return mRf[row].Find(x => x.column == column).value;
				else
					return bop.Zero();
			}
			set
			{
				mRf[row].RemoveAll(x => x.column == column);
				mCf[column].RemoveAll(x => x.row == row);
				if(bop.CompareTo(value, bop.Zero()) != 0)
				{
					MatrixItem item = new MatrixItem()
					{
						column = column,
						row = row,
						value = value
					};
					mRf[row].Add(item);
					mCf[column].Add(item);
				}
			}
		}

		public ImplicitMatrix<U> ConvertTo<U>() where U : IComparable<U>
		{
			ImplicitMatrix<U> result = new ImplicitMatrix<U>(this.Rows, this.Columns);
			foreach (List<MatrixItem> matrixItems in mRf)
			{
				foreach (MatrixItem matrixItem in matrixItems)
				{
					result[matrixItem.row, matrixItem.column] = bop.ConvertTo<U>(matrixItem.value);
				}
			}
			return result;
		}

		public ImplicitMatrix<T> Transpose()
		{
			ImplicitMatrix<T> result = new ImplicitMatrix<T>(Columns, Rows);
			foreach (List<MatrixItem> matrixItems in mRf)
			{
				foreach (MatrixItem matrixItem in matrixItems)
				{
					result[matrixItem.column, matrixItem.row] = matrixItem.value;
				}
			}
			return result;
		}

		public static ImplicitMatrix<T> operator *(ImplicitMatrix<T> m, T scaler)
		{
			ImplicitMatrix<T> result = new ImplicitMatrix<T>(m.Rows, m.Columns);
			foreach (List<MatrixItem> matrixItems in m.mRf)
			{
				foreach (MatrixItem matrixItem in matrixItems)
				{
					result[matrixItem.row, matrixItem.column] = matrixItem.value*scaler;
				}
			}
			return result;
		}

		public static ImplicitMatrix<T> operator *(T scaler, ImplicitMatrix<T> m)
		{
			return m * scaler;
		}

		public static ImplicitMatrix<T> operator *(ImplicitMatrix<T> m1, ImplicitMatrix<T> m2)
		{
			if (m1.Columns != m2.Rows)
			{
				throw new Exception("Matrix multiplication error, row of m1 isn't equal to the column count of m2");
			}

			ImplicitMatrix<T> output = new ImplicitMatrix<T>(m1.Rows, m2.Columns);
			for (int i = 0; i < output.Rows; i++)
			{
				for (int j = 0; j < output.Columns; j++)
				{
					CalculatingUnit<T> item = bop.Zero();
					foreach (MatrixItem mItem1 in m1.mRf[i])
					{
						if(m2.mCf[j].Exists(x => x.row == mItem1.column))
						{
							MatrixItem mItem2 = m2.mCf[j].Find(x => x.row == mItem1.column);
							item = bop.Add(item, bop.Multiply(mItem1.value, mItem2.value));
						}	
					}
					output[i, j] = item;
				}
			}
			return output;
		}

		public static HyperPoint<T> operator *(ImplicitMatrix<T> m, HyperPoint<T> p)
		{
			if (m.Columns != p.Dim)
			{
				throw new Exception("Matrix multiplication error, row of m1 isn't equal to the column count of m2");
			}

			HyperPoint<T> output = new HyperPoint<T>(m.Rows);
			for (int i = 0; i < output.Dim; i++)
			{
				CalculatingUnit<T> item = bop.Zero();
				foreach (MatrixItem mItem in m.mRf[i])
				{
					item = bop.Add(item, mItem.value*p[mItem.column]);
				}
				output[i] = item;
			}
			return output;
		}

		public static ImplicitMatrix<T> operator -(ImplicitMatrix<T> m1)
		{
			ImplicitMatrix<T> output = new ImplicitMatrix<T>(m1.Rows, m1.Columns);
			foreach (List<MatrixItem> matrixItems in m1.mCf)
			{
				foreach (MatrixItem matrixItem in matrixItems)
				{
					output[matrixItem.row, matrixItem.column] = bop.Negate(matrixItem.value);
				}
			}
			return output;
		}

		/// <summary>
		/// Creates an Identy matrix
		/// </summary>
		/// <param name="n"></param>
		/// <returns>an nxn matrix</returns>
		public static ImplicitMatrix<T> Identity(int n = 4)
		{
			ImplicitMatrix<T> m = new ImplicitMatrix<T>(n, n);
			for (int i = 0; i < n; i++)
			{
				m[i, i] = bop.One();
			}
			return m;
		}
	}
}
