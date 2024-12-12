using System.Reflection;
using System.Drawing;

namespace DigitalCircularityToolkit
{

  public static class IconLoader
  {

    public static Bitmap EuclideanIcon { get; private set; }

    static IconLoader()
    {
      EuclideanIcon = LoadIcon("DigitalCircularityToolkit.Resources.DISTANCESYMM.png");
    }

    private static Bitmap LoadIcon(string resourceName)
    {
      var assembly = Assembly.GetExecutingAssembly();
      var imageStream = assembly.GetManifestResourceStream(resourceName);

      return new Bitmap(imageStream);
    }

  }

}