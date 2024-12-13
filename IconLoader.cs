using System.Reflection;
using System.Drawing;

namespace DigitalCircularityToolkit
{

  public static class IconLoader
  {
    public static Bitmap AlignToObjectIcon { get; private set; }
    public static Bitmap AlignToPlaneIcon { get; private set; }
    public static Bitmap BoxObjectIcon { get; private set; }
    public static Bitmap BoxScoreIcon { get; private set; }
    public static Bitmap BoxSetIcon { get; private set; }
    public static Bitmap DistanceAsymmIcon { get; private set; }
    public static Bitmap DistanceSymmIcon { get; private set; }
    public static Bitmap FeatureVectorIcon { get; private set; }
    public static Bitmap HarmonicsComplexIcon { get; private set; }
    public static Bitmap HarmonicsRealIcon { get; private set; }
    public static Bitmap Hull2dIcon { get; private set; }
    public static Bitmap HungarianIcon { get; private set; }
    public static Bitmap KnollIcon { get; private set; }
    public static Bitmap LinearObjectIcon { get; private set; }
    public static Bitmap LinearSetIcon { get; private set; }
    public static Bitmap LineScoreIcon { get; private set; }
    public static Bitmap ManhattanIcon { get; private set; }
    public static Bitmap MatchLinesIcon { get; private set; }
    public static Bitmap NormalizeIcon { get; private set; }
    public static Bitmap ObjectIcon { get; private set; }
    public static Bitmap ObjectPropertiesIcon { get; private set; }
    public static Bitmap PlanarOutlineIcon { get; private set; }
    public static Bitmap OverridePCAIcon { get; private set; }
    public static Bitmap PairNormalizeIcon { get; private set; }
    public static Bitmap PlanarObjectIcon { get; private set; }
    public static Bitmap PlanarSetIcon { get; private set; }
    public static Bitmap PlaneScoreIcon { get; private set; }
    public static Bitmap RadialSignatureIcon { get; private set; }
    public static Bitmap RotatePCAIcon { get; private set; }
    public static Bitmap SphereScoreIcon { get; private set; }
    public static Bitmap SphereSetIcon { get; private set; }
    public static Bitmap SphericalObjectIcon { get; private set; }

    static IconLoader()
    {
        AlignToObjectIcon = LoadIcon("DigitalCircularityToolkit.Resources.ALIGNTOOBJECT.png");
        AlignToPlaneIcon = LoadIcon("DigitalCircularityToolkit.Resources.ALIGNTOPLANE.png");
        BoxObjectIcon = LoadIcon("DigitalCircularityToolkit.Resources.BOXOBJECT.png");
        BoxScoreIcon = LoadIcon("DigitalCircularityToolkit.Resources.BOXSCORE.png");
        BoxSetIcon = LoadIcon("DigitalCircularityToolkit.Resources.BOXSET.png");
        DistanceAsymmIcon = LoadIcon("DigitalCircularityToolkit.Resources.DISTANCEASYMM.png");
        DistanceSymmIcon = LoadIcon("DigitalCircularityToolkit.Resources.DISTANCESYMM.png");
        FeatureVectorIcon = LoadIcon("DigitalCircularityToolkit.Resources.FEATUREVEC.png");
        HarmonicsComplexIcon = LoadIcon("DigitalCircularityToolkit.Resources.HARMONICSCOMPLEX.png");
        HarmonicsRealIcon = LoadIcon("DigitalCircularityToolkit.Resources.HARMONICSREAL.png");
        Hull2dIcon = LoadIcon("DigitalCircularityToolkit.Resources.HULL.png");
        HungarianIcon = LoadIcon("DigitalCircularityToolkit.Resources.HUNGARIAN.png");
        KnollIcon = LoadIcon("DigitalCircularityToolkit.Resources.KNOLL.png");
        LinearObjectIcon = LoadIcon("DigitalCircularityToolkit.Resources.LINEAROBJECT.png");
        LinearSetIcon = LoadIcon("DigitalCircularityToolkit.Resources.LINEARSET.png");
        LineScoreIcon = LoadIcon("DigitalCircularityToolkit.Resources.LINESCORE.png");
        ManhattanIcon = LoadIcon("DigitalCircularityToolkit.Resources.MANHATTAN.png");
        MatchLinesIcon = LoadIcon("DigitalCircularityToolkit.Resources.MATCHLINES.png");
        NormalizeIcon = LoadIcon("DigitalCircularityToolkit.Resources.NORMALIZE.png");
        ObjectIcon = LoadIcon("DigitalCircularityToolkit.Resources.OBJECT.png");
        ObjectPropertiesIcon = LoadIcon("DigitalCircularityToolkit.Resources.OBJECTPROPERTIES.png");
        PlanarOutlineIcon = LoadIcon("DigitalCircularityToolkit.Resources.OUTLINE.png");
        OverridePCAIcon = LoadIcon("DigitalCircularityToolkit.Resources.OVERRIDEPCA.png");
        PairNormalizeIcon = LoadIcon("DigitalCircularityToolkit.Resources.PAIRNORMALIZE.png");
        PlanarObjectIcon = LoadIcon("DigitalCircularityToolkit.Resources.PLANAROBJECT.png");
        PlanarSetIcon = LoadIcon("DigitalCircularityToolkit.Resources.PLANARSET.png");
        PlaneScoreIcon = LoadIcon("DigitalCircularityToolkit.Resources.PLANESCORE.png");
        RadialSignatureIcon = LoadIcon("DigitalCircularityToolkit.Resources.RADIALSIG.png");
        RotatePCAIcon = LoadIcon("DigitalCircularityToolkit.Resources.ROTATEPCA.png");
        SphereScoreIcon = LoadIcon("DigitalCircularityToolkit.Resources.SPHERESCORE.png");
        SphereSetIcon = LoadIcon("DigitalCircularityToolkit.Resources.SPHERESET.png");
        SphericalObjectIcon = LoadIcon("DigitalCircularityToolkit.Resources.SPHERICALOBJECT.png");
    }

    private static Bitmap LoadIcon(string resourceName)
    {
      var assembly = Assembly.GetExecutingAssembly();
      var imageStream = assembly.GetManifestResourceStream(resourceName);

      return new Bitmap(imageStream);
    }

  }

}