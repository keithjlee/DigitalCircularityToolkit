using System.Diagnostics;
using DigitalCircularityToolkit.Matching;
using LapjvCSharp;

class Num { public double Value; public Num(double v){Value=v;} }   // stand-in for GH_Number
class Int { public int Value; public Int(int v){Value=v;} }          // stand-in for GH_Integer

static class P {
  static double Dist(IEnumerable<Num> a, IEnumerable<Num> b){ int n=a.Count(); double s=0; for(int i=0;i<n;i++) s+=Math.Pow(a.ElementAt(i).Value-b.ElementAt(i).Value,2); return Math.Sqrt(s);}
  // current pattern: tree of boxed numbers in, tree of boxed ints out
  static List<List<Int>> Current(List<List<Num>> D, List<List<Num>> S, int p){
    var tree=new List<List<Int>>();
    for(int i=0;i<D.Count;i++){ var d=D[i].Cast<Num>(); var row=new List<Int>(); tree.Add(row);
      for(int j=0;j<S.Count;j++){ var s=S[j].Cast<Num>(); row.Add(new Int((int)Math.Ceiling(Math.Pow(Dist(d,s),p)))); } }
    return tree;
  }
  // flat row-major double[] in/out
  static double[] Flat(double[] D,double[] S,int n,int m,int k,bool par){
    var C=new double[n*m];
    void Row(int i){ var di=D.AsSpan(i*k,k); var ci=C.AsSpan(i*m,m);
      for(int j=0;j<m;j++){ var sj=S.AsSpan(j*k,k); double s=0; for(int t=0;t<k;t++){ double x=di[t]-sj[t]; s+=x*x;} ci[j]=Math.Sqrt(s);} }
    if(par) Parallel.For(0,n,Row); else for(int i=0;i<n;i++) Row(i);
    return C;
  }
  static double T(Action a,int reps=3){ a(); var sw=Stopwatch.StartNew(); for(int r=0;r<reps;r++) a(); return sw.Elapsed.TotalMilliseconds/reps; }

  static void Main(string[] args){
    if (args.Length > 0 && args[0] == "pca") { PcaBench.Run(); return; }
    if (args.Length > 0 && args[0] == "pcasign") { PcaSignProbe.Run(); return; }
    if (args.Length > 0 && args[0] == "eigcheck") { EigenCheck.Run(); return; }
    var rng=new Random(1);
    Console.WriteLine($"cores={Environment.ProcessorCount}");
    foreach(var (n,m,k) in new[]{(500,500,8),(2000,2000,8),(2000,2000,64)}){
      var Df=Enumerable.Range(0,n*k).Select(_=>rng.NextDouble()*100).ToArray();
      var Sf=Enumerable.Range(0,m*k).Select(_=>rng.NextDouble()*100).ToArray();
      var Dt=Enumerable.Range(0,n).Select(i=>Enumerable.Range(0,k).Select(t=>new Num(Df[i*k+t])).ToList()).ToList();
      var St=Enumerable.Range(0,m).Select(j=>Enumerable.Range(0,k).Select(t=>new Num(Sf[j*k+t])).ToList()).ToList();
      double a=T(()=>Current(Dt,St,1),1), b=T(()=>Flat(Df,Sf,n,m,k,false)), c=T(()=>Flat(Df,Sf,n,m,k,true));
      Console.WriteLine($"dist {n}x{m} k={k}: current {a:F0} ms | flat {b:F1} ms ({a/b:F0}x) | flat+parallel {c:F1} ms ({a/c:F0}x)");
    }
    foreach(var n in new[]{500,1000,2000}){
      var C=new double[n,n]; for(int i=0;i<n;i++)for(int j=0;j<n;j++) C[i,j]=Math.Ceiling(rng.NextDouble()*1000);
      int[] x1=null,x2=null;
      double l=T(()=>{ x1=new Lapjv().lapjvCsharp(C,true,double.PositiveInfinity).Item1; },1);
      double s=T(()=>{ x2=new ShortestPathSolver().Solve(C); },1);
      double c1=0,c2=0; for(int i=0;i<n;i++){c1+=C[i,x1[i]];c2+=C[i,x2[i]];}
      Console.WriteLine($"LAP {n}x{n}: LapjvCSharp {l:F0} ms (cost {c1}) | ShortestPathSolver {s:F0} ms (cost {c2})");
    }
    foreach(var n in new[]{1000,2000,4000}){ // 1-D length matching w/ integer-quantized costs (truss case): many ties
      var d=Enumerable.Range(0,n).Select(_=>3.0+rng.NextDouble()*2).ToArray(); var s=Enumerable.Range(0,n).Select(_=>3.0+rng.NextDouble()*2).ToArray();
      var C=new double[n,n]; for(int i=0;i<n;i++)for(int j=0;j<n;j++) C[i,j]=Math.Ceiling(Math.Abs(d[i]-s[j])*100);
      double l=T(()=>new Lapjv().lapjvCsharp(C,true,double.PositiveInfinity),1), sp=T(()=>new ShortestPathSolver().Solve(C),1);
      Console.WriteLine($"LAP 1D-lengths {n}x{n} (int-quantized): LapjvCSharp {l:F0} ms | ShortestPathSolver {sp:F0} ms");
    }
    // rectangular, demand > supply: what does Match.cs see?
    var R=new double[5,3]; for(int i=0;i<5;i++)for(int j=0;j<3;j++) R[i,j]=rng.Next(1,10);
    var (rows,_)=new Lapjv().lapjvCsharp(R,true,double.PositiveInfinity);
    Console.WriteLine("5 demand x 3 supply -> assignments: ["+string.Join(",",rows)+"]");
    try { double tc=0; for(int i=0;i<5;i++) tc+=R[i,rows[i]]; Console.WriteLine("total cost ok "+tc);} catch(Exception e){ Console.WriteLine("Match.cs total-cost loop would throw: "+e.GetType().Name); }
  }
}
