using System;
using System.IO;
using MatrixOperations;
using System.Threading.Tasks;
using BaseConstInit;

namespace AlgorithmsTesting
{
	class MainClass
	{
		public static float[][] GetMult(float[][] a, float[][] b)
		{
			if (a.Length != b[0].Length)
				throw new Exception("Wrong sizes of matrices, they can't be multiplied");
		
			float[][] res = null;
			BCI<float>.init(ref res, a.Length, b[0].Length);
			
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
	
		static Tuple<sbyte, float[][]> GetUpTriang(float[][] mtrx)
		{
			sbyte sign = 1;
			
			for (int i = 0; i < mtrx.Length; i++)
			{
				if (mtrx[i][i] == 0 && find_swap(mtrx, i, ref sign) == 0) 
					return new Tuple<sbyte, float[][]> (0, null);
				Parallel.For(i + 1, mtrx[0].Length, delegate(int j) {
					float coef = mtrx[j][i] / mtrx[i][i];
					for (int p = i; p < mtrx[0].Length; p++) 
						mtrx[j][p] -= coef * mtrx[i][p];
				});
			}
			
			return Tuple.Create(sign, mtrx);
		}
	
		public static float GetDet(float[][] mtrx)
		{
			if (mtrx.Length != mtrx[0].Length)
				throw new Exception("Wrong size of matrix, it mush be square matrix");
		
			float res = 1;
			float[][] arr = null;
			BCI<float>.init(ref arr, mtrx.Length, mtrx[0].Length);
			Array.Copy(mtrx, arr, mtrx.Length * mtrx[0].Length);
			
			Tuple<sbyte, float[][]> t = GetUpTriang(arr);
			if (t.Item1 == 0)
				return 0;
				
			for (int i = 0; i < mtrx.Length; i++)
				res *= t.Item2[i][i];
				
			return res * t.Item1;
		}
	
		public static void Main (string[] args)
		{
			using (StreamReader sr = new StreamReader("input.txt"))
			{
				string s;
				string[] sArr = sr.ReadLine().Split(' ');
				int n = int.Parse(sArr[0]), m = int.Parse(sArr[1]);
				float[][] mtrx = null;
				BCI<float>.init(ref mtrx, n, m);
				
				for (int i = 0; i < n; i++)
				{
					s = sr.ReadLine();
					sArr = s.Split(' ');
					for (int j = 0; j < sArr.Length; j++)
						mtrx[i][j] = float.Parse(sArr[j]);
				}
				
				//mtrx = MatrixOperations.MtrxOps.GetTransp(mtrx);
				float[][] mtrx1 = MtrxOps.GetMult(mtrx, mtrx);
				
				for (int i = 0; i < mtrx1.Length; i++)
				{
					for (int j = 0; j < mtrx1[0].Length; j++)
						Console.Write(mtrx1[i][j] + " ");
					Console.Write('\n');
				}
				
				Console.Write(MtrxOps.GetDet(mtrx) + "\n");
				float[][] arr = MtrxOps.GetReverse(mtrx);
				
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < m; j++)
						Console.Write(arr[i][j] + " ");
					Console.Write('\n');
				}
			}
			
		}
	}
}
