using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MatrixOperations
{
	public class MtrxOps
	{
		public static float[,] GetTransp(float[,] mtrx)
		{
			float[,] outMtrx = new float[mtrx.GetLength(1), mtrx.GetLength(0)];
			
			Parallel.For (0, mtrx.GetLength(0), delegate(int i) {
				for (int j = 0; j < mtrx.GetLength(1); j++)
					outMtrx[j, i] = mtrx[i, j];
			});
			
			return outMtrx;
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		
		static sbyte find_swap(float[,] mtrx, int i, ref sbyte sign)
		{
			int j;
			float buf;
			
			for (j = i + 1; j < mtrx.GetLength(0) && mtrx[j, i] == 0; j++) 
				{	}
			if (j == mtrx.GetLength(0)) 
				return 0;
				
			for (int p = 0; p < mtrx.GetLength(1); p++)
			{
				buf = mtrx[i, p];
				mtrx[i, p] = mtrx[j, p];
				mtrx[j, p] = buf;
			}
			sign *= -1;
			
			return sign;
		}
		
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		static sbyte find_swap(float[,] mtrx, float[,] add, int i)
		{
			int j;
			float buf;
			
			for (j = i + 1; j < mtrx.GetLength(0) && mtrx[j, i] == 0; j++) 
				{	}
			if (j == mtrx.GetLength(0)) 
				return 0;
				
			for (int p = 0; p < mtrx.GetLength(1); p++)
			{
				buf = mtrx[i, p];
				mtrx[i, p] = mtrx[j, p];
				mtrx[j, p] = buf;
				
				buf = add[i, p];
				add[i, p] = add[j, p];
				add[j, p] = buf;
			}
			
			return 1;
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		static Tuple<sbyte, float[,]> GetUpTriang(float[,] mtrx)
		{
			sbyte sign = 1;
			
			for (int i = 0; i < mtrx.GetLength(0); i++)
			{
				if (mtrx[i, i] == 0 && find_swap(mtrx, i, ref sign) == 0) 
					return new Tuple<sbyte, float[,]> (0, null);
				Parallel.For(i + 1, mtrx.GetLength(1), delegate(int j) {
					float coef = mtrx[j, i] / mtrx[i, i];
					for (int p = i; p < mtrx.GetLength(1); p++) 
						mtrx[j, p] -= coef * mtrx[i, p];
				});
			}
			
			return Tuple.Create(sign, mtrx);
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		static Tuple<float[,], float[,]> GetUpTriang(float[,] mtrx, float[,] add)
		{
			for (int i = 0; i < mtrx.GetLength(0); i++)
			{
				if (mtrx[i, i] == 0 && find_swap(mtrx, add, i) == 0) 
					return new Tuple<float[,], float[,]> (null, null);
				Parallel.For(i + 1, mtrx.GetLength(1), delegate(int j) 
				{
					float coef = mtrx[j, i] / mtrx[i, i];
					for (int p = 0; p < mtrx.GetLength(1); p++)
					{
						mtrx[j, p] -= coef * mtrx[i, p];
						add[j, p] -= coef * add[i, p];
					}
				});
			}
			
			return Tuple.Create(mtrx, add);
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		static Tuple<float[,], float[,]> GetDownTriang(float[,] mtrx, float[,] add)
		{
			for (int i = mtrx.GetLength(0) - 1; i >= 0; i--)
			{
				if (mtrx[i, i] == 0 && find_swap(mtrx, add, i) == 0)
					return new Tuple<float[,], float[,]> (null, null);
				Parallel.For(0, i, delegate(int j) 
				{
					float coef = mtrx[j, i] / mtrx[i, i];
					for (int p = 0; p < mtrx.GetLength(1); p++)
					{
						mtrx[j, p] -= coef * mtrx[i, p];
						add[j, p] -= coef * add[i, p];
					}
				});
			}
			
			return Tuple.Create(mtrx, add);
		}

		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		public static float GetDet(float[,] mtrx)
		{
			if (mtrx.GetLength(0) != mtrx.GetLength(1))
				throw new Exception("Wrong size of matrix, it mush be square matrix");
		
			float res = 1;
			float[,] arr = new float[mtrx.GetLength(0), mtrx.GetLength(1)];
			Array.Copy(mtrx, arr, mtrx.Length);
			
			Tuple<sbyte, float[,]> t = GetUpTriang(arr);
			if (t.Item1 == 0)
				return 0;
				
			for (int i = 0; i < mtrx.GetLength(0); i++)
				res *= t.Item2[i, i];
				
			return res * t.Item1;
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//

		public static float[,] GetReverse(float[,] mtrx)
		{
			if (GetDet(mtrx) == 0)
				throw new Exception("Determinant of matrix is 0, this matrix has not a reverse matrix");
		
			float[,] arr = new float[mtrx.GetLength(0), mtrx.GetLength(1)];
			Array.Copy(mtrx, arr, mtrx.Length);
			
			float [,] e = new float[mtrx.GetLength(0), mtrx.GetLength(1)];
			for (int i = 0; i < e.GetLength(0); i++)
				e[i, i] = 1;
			
			GetUpTriang(arr, e);
			GetDownTriang(arr, e);
			
			Parallel.For(0, e.GetLength(0), delegate(int i) {
				for (int j = 0; j < e.GetLength(1); j++)
					e[i, j] /= arr[i, i];
			});
			
			return e;
		}
		
		//-------------------------------------------------------------------------------------------------------------------------------------------------------------------//
		
		public static float[,] GetMult(float[,] a, float[,] b)
		{
			if (a.GetLength(0) != b.GetLength(1))
				throw new Exception("Wrong sizes of matrices, they can't be multiplied");
		
			float[,] res = new float[a.GetLength(0), b.GetLength(1)];
			
			Parallel.For(0, a.GetLength(0), delegate(int i) {
				for (int j = 0; j < b.GetLength(1); j++)
				{
					float sum = 0;
					for (int p = 0; p < a.GetLength(1); p++)
							sum += a[i, p] * b[p, j];
					res[i, j] = sum;
				}
			});
			
			return res;
		}
	}
}

