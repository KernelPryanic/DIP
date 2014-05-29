using System;
using System.IO;
using MatrixOperations;
using System.Threading.Tasks;

namespace AlgorithmsTesting
{
	class MainClass
	{
		static byte[] DimUp(byte[] v, int fn)
		{
			if (fn < v.Length)
				throw new Exception("Ok, but I'll not do this :) New dimension is lower than native.");
			if (fn > Math.Pow(2, v.Length))
				throw new Exception("Can't get so high dimension :)");
			return DoDimUp(v, ref fn);
		}
		
		static byte[] DoDimUp(byte[] v, ref int n)
		{
			int fild = 0;
			byte[] res = new byte[Math.Min((int)(Math.Pow(2, v.Length) - 1), n)];
			Array.Copy(v, res, v.Length);
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
	
		public static void Main (string[] args)
		{
			using (StreamReader sr = new StreamReader("input.txt"))
			{
				string s;
				string[] sArr = sr.ReadLine().Split(' ');
				int n = int.Parse(sArr[0]), m = int.Parse(sArr[1]);
				float[,] mtrx = new float[n, m];
				
				for (int i = 0; i < n; i++)
				{
					s = sr.ReadLine();
					sArr = s.Split(' ');
					for (int j = 0; j < sArr.Length; j++)
						mtrx[i, j] = float.Parse(sArr[j]);
				}
				
				//mtrx = MatrixOperations.MtrxOps.GetTransp(mtrx);
				float[,] mtrx1 = MatrixOperations.MtrxOps.GetMult(mtrx, mtrx);
				
				for (int i = 0; i < mtrx1.GetLength(0); i++)
				{
					for (int j = 0; j < mtrx1.GetLength(1); j++)
						Console.Write(mtrx1[i, j] + " ");
					Console.Write('\n');
				}
				
				Console.Write(MatrixOperations.MtrxOps.GetDet(mtrx) + "\n");
				float[,] arr = MatrixOperations.MtrxOps.GetReverse(mtrx);
				
				byte[] arr1 = new byte[]{1, 0, 1};
				byte[] arr2 = DimUp(arr1, 4);
				
				for (int i = 0; i < arr2.Length; i++)
					Console.Write(arr2[i] + " ");
				Console.WriteLine();
				
				for (int i = 0; i < n; i++)
				{
					for (int j = 0; j < m; j++)
						Console.Write(arr[i, j] + " ");
					Console.Write('\n');
				}
			}
			
		}
	}
}
