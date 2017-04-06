using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameSample
{
    static class SpriteBatchExtensions
    {
        public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? source, Color color, Vector2 origin)
            => spriteBatch.Draw(texture, position, source, color, 0f, origin, 1f, SpriteEffects.None, 0f);
    }
}
