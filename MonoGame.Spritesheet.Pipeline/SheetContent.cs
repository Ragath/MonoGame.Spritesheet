using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGame.Spritesheet.Pipeline
{
    [ContentSerializerRuntimeType("MonoGame.Spritesheet.Sheet, MonoGame.Spritesheet")]
    public class SheetContent
    {
        public int SpriteWidth { get; set; }
        public int SpriteHeight { get; set; }
        public int Padding { get; set; }
        
        public Texture2DContent Texture { get; set; }
        //public IReadOnlyDictionary<string, Rectangle> Sources { get; set; }

        public string Name => Texture.Name;
    }
}
