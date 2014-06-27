using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BaseConstInit;

namespace MatrixOperations
{
	public class MtrxOps
	{
		public static T[][] GetTransp<T>(T[][] mtrx)
		{
			T[][] outMtrx = null;
			BCI.init<T>(ref outMtrx, mtrx[0].Length, mtrx.Length);
			
			Parallel.For (0, mtrx.Length, delegate(int i) {
				for (int j = 0; j < mtrx[0].Length; j++)
					outMtrx[j][i] = mtrx[i][j];
			});
			
			return outMtrx;
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		
		static sbyte find_swap(float[][] mtrx, int i, ref sbyte sign)
		{
			int j;
			float[] buf;
			
			for (j = i + 1; j < mtrx.Length && mtrx[j][i] == 0; j++) 
				{	}
			if (j == mtrx.Length) 
				return 0;
			
			
			buf = mtrx[i];
			mtrx[i] = mtrx[j];
			mtrx[j] = buf;
			
			sign *= -1;
			
			return sign;
		}
		
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		static sbyte find_swap(float[][] mtrx, float[][] add, int i)
		{
			int j;
			float[] buf;
			
			for (j = i + 1; j < mtrx.Length && mtrx[j][i] == 0; j++) 
				{	}
			if (j == mtrx.Length) 
				return 0;
				
			buf = mtrx[i];
			mtrx[i] = mtrx[j];
			mtrx[j] = buf;
				
			buf = add[i];
			add[i] = add[j];
			add[j] = buf;
			
			return 1;
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		static Tuple<sbyte, float[][]> GetUpTriang(float[][] mtrx)
		{
			sbyte sign = 1;
			
			for (int i = 0; i < mtrx.Length; i++)
			{
				if (mtrx[i][i] == 0 && find_swap(mtrx, i, ref sign) == 0) 
					return new Tuple<sbyte, float[][]> (0, null);
				Parallel.For(i + 1, mtrx.Length, delegate(int j) {
					float coef = mtrx[j][i] / mtrx[i][i];
					for (int p = i; p < mtrx[0].Length; p++) 
						mtrx[j][p] -= coef * mtrx[i][p];
				});
			}
			
			return Tuple.Create(sign, mtrx);
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		static Tuple<float[][], float[][]> GetUpTriang(float[][] mtrx, float[][] add)
		{
			for (int i = 0; i < mtrx.Length; i++)
			{
				if (mtrx[i][i] == 0 && find_swap(mtrx, add, i) == 0) 
					return new Tuple<float[][], float[][]> (null, null);
				Parallel.For(i + 1, mtrx.Length, delegate(int j) 
				{
					float coef = mtrx[j][i] / mtrx[i][i];
					for (int p = 0; p < mtrx[0].Length; p++)
					{
						mtrx[j][p] -= coef * mtrx[i][p];
						add[j][p] -= coef * add[i][p];
					}
				});
			}
			
			return Tuple.Create(mtrx, add);
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		static Tuple<float[][], float[][]> GetDownTriang(float[][] mtrx, float[][] add)
		{
			for (int i = mtrx.Length - 1; i >= 0; i--)
			{
				if (mtrx[i][i] == 0 && find_swap(mtrx, add, i) == 0)
					return new Tuple<float[][], float[][]> (null, null);
				Parallel.For(0, i, delegate(int j) 
				{
					float coef = mtrx[j][i] / mtrx[i][i];
					for (int p = 0; p < mtrx[0].Length; p++)
					{
						mtrx[j][p] -= coef * mtrx[i][p];
						add[j][p] -= coef * add[i][p];
					}
				});
			}
			
			return Tuple.Create(mtrx, add);
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		public static float GetDet(float[][] mtrx)
		{
			if (mtrx.Length != mtrx[0].Length)
				throw new Exception("Wrong size of matrix, it mush be square matrix");
		
			float res = 1;
			float[][] arr = null;
			BCI.init<float>(ref arr, mtrx.Length, mtrx[0].Length);
			mtrx.CopyTo(arr, 0);
			
			Tuple<sbyte, float[][]> t = GetUpTriang(arr);
			if (t.Item1 == 0)
				return 0;
				
			for (int i = 0; i < mtrx.Length; i++)
				res *= t.Item2[i][i];
				
			return res * t.Item1;
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		public static float[][] GetReverse(float[][] mtrx)
		{
			if (GetDet(mtrx) == 0)
				throw new Exception("Determinant of matrix is 0, this matrix has not a reverse matrix");
		
			float[][] arr = null;
			BCI.init<float>(ref arr, mtrx.Length, mtrx[0].Length);
			mtrx.CopyTo(arr, 0);
			
			float [][] e = null;
			BCI.init<float>(ref e, mtrx.Length, mtrx[0].Length);
			for (int i = 0; i < e.Length; i++)
				e[i][i] = 1;
			
			GetUpTriang(arr, e);
			GetDownTriang(arr, e);
			
			Parallel.For(0, e.Length, delegate(int i) {
				for (int j = 0; j < e[0].Length; j++)
					e[i][j] /= arr[i][i];
			});
			
			return e;
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		
		public static float[][] GetMult(float[][] a, float[][] b)
		{
			if (a[0].Length != b.Length)
				throw new Exception("Wrong sizes of matrices, they can't be multiplied");
		
			float[][] res = null;
			BCI.init<float>(ref res, a.Length, b[0].Length);
			
			Parallel.For(0, a.Length, delegate(int i) {
				for (int j = 0; j < b[0].Length; j++)
				{
					float sum = 0;
					for (int p = 0; p < a[0].Length; p++)
							sum += a[i][p] * b[p][j];
					res[i][j] = sum;
				}
			});
			
			return res;
		}
		
		public static float[][] GetMult(byte[][] a, float[][] b)
		{
			if (a[0].Length != b.Length)
				throw new Exception("Wrong sizes of matrices, they can't be multiplied");
		
			float[][] res = null;
			BCI.init<float>(ref res, a.Length, b[0].Length);
			
			Parallel.For(0, a.Length, delegate(int i) {
				for (int j = 0; j < b[0].Length; j++)
				{
					float sum = 0;
					for (int p = 0; p < a[0].Length; p++)
							sum += a[i][p] * b[p][j];
					res[i][j] = sum;
				}
			});
			
			return res;
		}
		
		public static float[] GetMult(float[] a, float[][] b)
		{
			if (a.Length != b.Length)
				throw new Exception("Wrong sizes of matrices, they can't be multiplied");
			
			float[] res = new float[b[0].Length];
			
			Parallel.For(0, b[0].Length, delegate(int i) {
				float sum = 0;
				for (int j = 0; j < a.Length; j++)
					sum += a[j] * b[j][i];
				res[i] = sum;
			});
			
			return res;
		}
	}
}

