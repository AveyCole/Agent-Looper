using System;
namespace LoopPedal {static class FxOrder {
 public static readonly int[][] Orders=new int[][]{new[]{2,1,0},new[]{2,0,1},new[]{1,2,0},new[]{1,0,2},new[]{0,2,1},new[]{0,1,2}};
 public static int[] Items(int code){return Orders[Math.Max(0,Math.Min(5,code))];}
 public static int Move(int code,int from,int to){int[] a=(int[])Items(code).Clone();int moved=a[from];if(from<to)for(int i=from;i<to;i++)a[i]=a[i+1];else for(int i=from;i>to;i--)a[i]=a[i-1];a[to]=moved;for(int j=0;j<6;j++)if(a[0]==Orders[j][0]&&a[1]==Orders[j][1]&&a[2]==Orders[j][2])return j;return 0;}
}}
