using System;
using System.IO;
using MatrixOperations;
using System.Threading.Tasks;
using BaseConstInit;

namespace AlgorithmsTesting
{
	public class KernelNet
	{
 		float[][] revCnts, multTr;
		byte[][] memImages;
		int N, fN;
	
		public KernelNet(byte[][] images, int n, int fn, int bitn, char chr)
		{
			N = n * n;
			fN = fn;
			byte[][] expImages = new byte[images.Length][];
			memImages = new byte[images.Length][];
			
			for (int i = 0; i < images.GetLength(0); i++)
			{
				memImages[i] = Sify(images[i], bitn);
				expImages[i] = DimUp(memImages[i], fN);
			}
				
			float[][] cnts = null;
			BCI.init<float>(ref cnts, images.Length, images.Length);
			
			for (int i = 0; i < images.Length; i++)
			{
				for (int j = 0; j < images.Length; j++)
				{
					cnts[i][j] = K(expImages[i], expImages[j]);
					Console.Write(cnts[i][j] + " ");
				}
				Console.WriteLine();
			}
			revCnts = MtrxOps.GetReverse(cnts);
			multTr = MtrxOps.GetTransp<float>(MtrxOps.GetMult(MtrxOps.GetTransp<byte>(memImages), revCnts));
		}
		
		public Tuple<float, char> Recognize(byte[] image, int bitn)
		{
			float[][] arrS = new float[5][];
			arrS[0] = new float[]{6, 3, 1, 3, 1};
			arrS[1] = new float[]{3, 3, 0, 1, 1};
			arrS[2] = new float[]{1, 0, 1, 1, 0};
			arrS[3] = new float[]{3, 1, 1, 3, 0};
			arrS[4] = new float[]{1, 1, 0, 0, 1};
		
			float[][] revS = MtrxOps.GetReverse(arrS);
		
			float[] z = new float[memImages.Length], x = ToFloat(Sify(image, bitn)), y = new float[N];
			
			x = new float[]{0.22f, 0.75f, 0.8f};
			for (int r = 0; r < 10; r++)
			{
				for (int i = 0; i < memImages.Length; i++)
					z[i] = K(memImages[i], x);
				y = MtrxOps.GetMult(z, multTr);
				x = SigmoidActFunc(y);
			}
			return Tuple.Create(0F, 'a');
		}
		
		byte[] DimUp(byte[] v, int fn)
		{
			if (fn < v.Length)
				throw new Exception("Ok, but I'll not do this :) New dimension is lower than native.");
			if (fn >= Math.Pow(2, v.Length))
				throw new Exception("Can't get so high dimension :)");
			return DoDimUp(v, ref fn);
		}
		
		byte[] DoDimUp(byte[] v, ref int n)
		{
			int fild = 0;
			byte[] res = new byte[Math.Min((int)(Math.Pow(2, v.Length) - 1), n)];
			v.CopyTo(res, 0);
			n -= v.Length;
			fild += v.Length;
			
			for (int i = 0; i < v.Length; i++)
			{
				byte[] buf = new byte[Math.Min(v.Length - i - 1, n)];
				for (int j = 0; j < buf.Length; j++)
					buf[j] = (byte)(v[i] * v[i + 1 + j]);
				byte[] rec = DoDimUp(buf, ref n);
				Array.Copy(rec, 0, res, fild, rec.Length);
				fild += rec.Length;
				if (n == 0)
					return res;
			}
			return res;
		}
		
		float[] SigmoidActFunc(float[] v)
		{
			float[] res = new float[v.Length];
			float expa;
			
			for (int i = 0; i < v.Length; i++)
			{
				expa = (float)Math.Exp(10 * (v[i] - 0.5));
				res[i] = expa / (float)(1 + expa);
			}
			
			return res;	
		}
		
		float K(byte[] v1, byte[] v2)
		{
			if (v1.Length != v2.Length)
				throw new Exception("Kernel can't apply to vectors different dimensions");
			
			float res = 0;
			for (int i = 0; i < v1.Length; i++)
				res += (float)(v1[i] * v2[i]);
				
			return res;
		}
		
		float K(byte[] v1, float[] v2)
		{
			if (v1.Length != v2.Length)
				throw new Exception("Kernel can't apply to vectors different dimensions");
			
			float res = 0;
			for (int i = 0; i < v1.Length; i++)
				res += (float)(v1[i] * v2[i]);
				
			return res;
		}
		
		byte[] Sify(byte[] image, int bitn)
		{
			byte[] res = new byte[image.Length / bitn];
			for (int i = 0; i < res.Length; i++)
				res[i] = (byte)((image[i * bitn] == 0) ? 0 : 1);
			return res;
		}
		
		float[] ToFloat(byte[] image)
		{
			float[] res = new float[image.Length];
			for (int i = 0; i < image.Length; i++)
				res[i] = (float)image[i];
			return res;
		}
	}

	class MainClass
	{	
		public static void Main (string[] args)
		{
			using (StreamReader sr = new StreamReader("input.txt"))
			{
				string s = sr.ReadLine();
				string[] arr = s.Split(' ');
				int n1 = int.Parse(arr[0]), m1 = int.Parse(arr[1]);
				byte[][] images = new byte[n1][];
				for (int i = 0; i < n1; i++)
				{
					images[i] = new byte[m1];
					s = sr.ReadLine();
					arr = s.Split(' ');
					for (int j = 0; j < m1; j++)
						images[i][j] = byte.Parse(arr[j]);
				}
				
				KernelNet kn = new KernelNet(images, 2, 7, 3, 'a');
				kn.Recognize(new byte[]{0, 0, 0, 0, 0, 0, 255, 255, 255}, 3);
			}
			
		}
	}
	
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
			//Array.Copy(mtrx, arr, mtrx.LongLength);
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
