using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGame.Spritesheet.Pipeline
{
    [ContentSerializerRuntimeType("MonoGame.Spritesheet.GridSheet, MonoGame.Spritesheet")]
    public class GridSheetContent
    {
        public Texture2DContent Texture { get; set; }

        public int SpriteWidth { get; set; }
        public int SpriteHeight { get; set; }
        public int Padding { get; set; }        
    }
}
