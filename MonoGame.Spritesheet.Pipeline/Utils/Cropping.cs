using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGame.Spritesheet.Pipeline.Utils
{
    static class Cropping
    {
        public static Vector2 TrimRect(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
        {
            var offsetX = GetLeft(ref rect, bitmap, colorKey);
            rect.X += offsetX;
            rect.Width -= offsetX;

            var offsetY = GetTop(ref rect, bitmap, colorKey);
            rect.Y += offsetY;
            rect.Height -= offsetY;

            int delta;
            delta = GetRight(ref rect, bitmap, colorKey);
            rect.Width -= delta;

            delta = GetBottom(ref rect, bitmap, colorKey);
            rect.Height -= delta;

            return new Vector2(offsetX, offsetY);
        }

        static int GetLeft(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
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
        static int GetRight(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
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

        static int GetTop(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
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
        static int GetBottom(ref Rectangle rect, PixelBitmapContent<Color> bitmap, Color colorKey)
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
