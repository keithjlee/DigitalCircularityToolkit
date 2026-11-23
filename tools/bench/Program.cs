using System.Diagnostics;
using DigitalCircularityToolkit.Numerics;
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
  // Writes rectangular integer cost matrices (1-D lengths, quantized, many ties) with the LapjvCSharp and
  // LinearAssignment answers, for an independent optimality check (scipy): tools/bench/rectcheck.py.
  static void RectCheck(string dir){
    Directory.CreateDirectory(dir);
    var rng=new Random(5);
    int k=0;
    foreach(var (n,m) in new[]{(30,128),(40,25),(200,600),(1000,3000)}){
      for(int t=0;t<3;t++,k++){
        var d=Enumerable.Range(0,n).Select(_=>1000+rng.NextDouble()*3000).ToArray(); var s=Enumerable.Range(0,m).Select(_=>1500+rng.NextDouble()*4500).ToArray();
        var C=new double[n,m]; var v=new double[n*m];
        for(int i=0;i<n;i++)for(int j=0;j<m;j++){ double x=s[j]-d[i]; double c=Math.Ceiling(x>=0?x:x*x); C[i,j]=c; v[i*m+j]=c; }
        var lap=new Lapjv().lapjvCsharp(C,true,double.PositiveInfinity).Item1;
        var la=LinearAssignment.Solve(new CostMatrix(n,m,v)).Assignment;
        File.WriteAllLines(Path.Combine(dir,$"m{k}.csv"), Enumerable.Range(0,n).Select(i=>string.Join(",",Enumerable.Range(0,m).Select(j=>C[i,j].ToString(System.Globalization.CultureInfo.InvariantCulture)))));
        File.WriteAllText(Path.Combine(dir,$"m{k}_lapjv.txt"), string.Join(",",lap));
        File.WriteAllText(Path.Combine(dir,$"m{k}_ours.txt"), string.Join(",",la));
      }
    }
  }
  static double T(Action a,int reps=3){ a(); var sw=Stopwatch.StartNew(); for(int r=0;r<reps;r++) a(); return sw.Elapsed.TotalMilliseconds/reps; }

  static void Main(string[] args){
    if (args.Length > 0 && args[0] == "pca") { PcaBench.Run(); return; }
    if (args.Length > 0 && args[0] == "pcasign") { PcaSignProbe.Run(); return; }
    if (args.Length > 0 && args[0] == "eigcheck") { EigenCheck.Run(); return; }
    if (args.Length > 1 && args[0] == "rectcheck") { RectCheck(args[1]); return; }
    if (args.Length > 0 && args[0] == "geo") { // Euclidean feature-space matrices (the Assign workload), Release timing
      foreach(var (n,m,k) in new[]{(2000,2000,8),(1000,3000,2),(2000,2000,2)}){
        var r=new Random(1);
        double[][] D=Enumerable.Range(0,n).Select(_=>Enumerable.Range(0,k).Select(__=>1000+r.NextDouble()*3000).ToArray()).ToArray();
        double[][] S=Enumerable.Range(0,m).Select(_=>Enumerable.Range(0,k).Select(__=>1500+r.NextDouble()*4500).ToArray()).ToArray();
        var sw=Stopwatch.StartNew(); var C=CostFunctions.EuclideanMatrix(D,S,1); var _v=C[0,0]; double build=sw.Elapsed.TotalMilliseconds;
        sw.Restart(); var res=LinearAssignment.Solve(C); double solve=sw.Elapsed.TotalMilliseconds;
        var C2=new double[n,m]; for(int i=0;i<n;i++)for(int j=0;j<m;j++) C2[i,j]=C[i,j];
        sw.Restart(); new Lapjv().lapjvCsharp(C2,true,double.PositiveInfinity); double lap=sw.Elapsed.TotalMilliseconds;
        Console.WriteLine($"geo {n}x{m} k={k}: build {build:F1} ms | LinearAssignment {solve:F0} ms | LapjvCSharp {lap:F0} ms");
      }
      return;
    }
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
    static CostMatrix M(double[,] C){ int n=C.GetLength(0), m=C.GetLength(1); var v=new double[n*m]; for(int i=0;i<n;i++)for(int j=0;j<m;j++) v[i*m+j]=C[i,j]; return new CostMatrix(n,m,v); }
    foreach(var n in new[]{500,1000,2000}){
      var C=new double[n,n]; for(int i=0;i<n;i++)for(int j=0;j<n;j++) C[i,j]=Math.Ceiling(rng.NextDouble()*1000);
      int[] x1=null; var cm=M(C); AssignmentResult r=null;
      double l=T(()=>{ x1=new Lapjv().lapjvCsharp(C,true,double.PositiveInfinity).Item1; },1);
      double s=T(()=>{ r=LinearAssignment.Solve(cm); },1);
      double c1=0; for(int i=0;i<n;i++) c1+=C[i,x1[i]];
      Console.WriteLine($"LAP {n}x{n}: LapjvCSharp {l:F0} ms (cost {c1}) | LinearAssignment {s:F0} ms (cost {r.Cost})");
    }
    foreach(var n in new[]{1000,2000,4000}){ // 1-D length matching (truss case): many ties in the quantized matrix
      var d=Enumerable.Range(0,n).Select(_=>3.0+rng.NextDouble()*2).ToArray(); var s=Enumerable.Range(0,n).Select(_=>3.0+rng.NextDouble()*2).ToArray();
      var C=new double[n,n]; for(int i=0;i<n;i++)for(int j=0;j<n;j++) C[i,j]=Math.Ceiling(Math.Abs(d[i]-s[j])*100);
      var cm=M(C);
      var dd=d.Select(x=>new[]{x}).ToArray(); var ss=s.Select(x=>new[]{x}).ToArray();
      double l=T(()=>new Lapjv().lapjvCsharp(C,true,double.PositiveInfinity),1), sp=T(()=>LinearAssignment.Solve(cm),1);
      double mono=T(()=>LinearAssignment.Solve(CostFunctions.EuclideanMatrix(dd,ss,1)),1);
      var dr=dd.Take(n*3/4).ToArray(); double monoRect=T(()=>LinearAssignment.Solve(CostFunctions.AsymmetricEuclideanMatrix(dr,ss,1,2,true)),1);
      Console.WriteLine($"LAP 1D-lengths {n}x{n}: LapjvCSharp {l:F0} ms | LinearAssignment general {sp:F0} ms | monotone square {mono:F1} ms | monotone {dr.Length}x{n} forbid-shortfall {monoRect:F1} ms");
    }
    // rectangular, demand > supply: what does Match.cs see?
    var R=new double[5,3]; for(int i=0;i<5;i++)for(int j=0;j<3;j++) R[i,j]=rng.Next(1,10);
    var (rows,_)=new Lapjv().lapjvCsharp(R,true,double.PositiveInfinity);
    Console.WriteLine("5 demand x 3 supply -> assignments: ["+string.Join(",",rows)+"]");
    try { double tc=0; for(int i=0;i<5;i++) tc+=R[i,rows[i]]; Console.WriteLine("total cost ok "+tc);} catch(Exception e){ Console.WriteLine("Match.cs total-cost loop would throw: "+e.GetType().Name); }
  }
}
