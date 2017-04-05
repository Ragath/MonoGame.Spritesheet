using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGame.Spritesheet.Pipeline.Utils
{
    static class BitmapExtensions
    {
        public static Rectangle GetBounds(this BitmapContent bitmap) => new Rectangle(0, 0, bitmap.Width, bitmap.Height);
    }
}
