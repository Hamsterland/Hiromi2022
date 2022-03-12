using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Hiromi.Extensions;

public static class ImageExtensions
{
    public static void Resize(this Image image, double percentage)
    {
        var width = (int) Math.Floor(image.Width * percentage);
        var height = (int) Math.Floor(image.Height * percentage);
        image.Mutate(ctx => ctx.Resize(width, height));
    }

    public static Image Overlay(this Image background, Image overlay, Point point)
    {
        return background.Clone(ctx => ctx.DrawImage(overlay, point, new GraphicsOptions()));
    }
    
    public static Image Overlay(this Image background, Image overlay, int x, int y)
    {
        return background.Clone(ctx => ctx.DrawImage(overlay, new Point(x, y), new GraphicsOptions()));
    }
}