using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGame.Spritesheet.Pipeline
{
    static class Cropping
    {
        public static void TrimRect(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
        {
            int delta;

            delta = TrimLeft(ref rect, bitmap, colorKey);
            rect.X += delta;
            rect.Width -= delta;

            delta = TrimRight(ref rect, bitmap, colorKey);
            rect.Width -= delta;

            delta = TrimTop(ref rect, bitmap, colorKey);
            rect.Y += delta;
            rect.Height -= delta;
            delta = TrimBottom(ref rect, bitmap, colorKey);
            rect.Height -= delta;
        }

        static int TrimLeft(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
        {
            for (int x = rect.Left, i = 0; x < rect.Right; x++, i++)
            {
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.A != 0 && color != colorKey)
                        return i;
                }
            }
            return 0;
        }
        static int TrimRight(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
        {
            for (int x = rect.Right - 1, i = 0; x >= rect.Left; x--, i++)
            {
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.A != 0 && color != colorKey)
                        return i;
                }
            }
            return 0;
        }

        static int TrimTop(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
        {
            for (int y = rect.Top, i = 0; y < rect.Bottom; y++, i++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.A != 0 && color != colorKey)
                        return i;
                }
            }
            return 0;
        }
        static int TrimBottom(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
        {
            for (int y = rect.Bottom - 1, i = 0; y >= rect.Top; y--, i++)
            {
                for (int x = rect.Left; x < rect.Right; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.A != 0 && color != colorKey)
                        return i;
                }
            }
            return 0;
        }
    }
}
