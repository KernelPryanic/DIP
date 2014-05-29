using System;
using System.IO;
using MatrixOperations;
using System.Threading.Tasks;

namespace AlgorithmsTesting
{
	class MainClass
	{
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
