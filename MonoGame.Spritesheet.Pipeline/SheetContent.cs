using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGame.Spritesheet.Pipeline
{
    [ContentSerializerRuntimeType("MonoGame.Spritesheet.Sheet, MonoGame.Spritesheet")]
    public class SheetContent
    {
        public Texture2DContent Texture { get; set; }
        public IReadOnlyDictionary<string, int> Names { get; set; }
        public IReadOnlyList<Rectangle> Sources { get; set; }
        public IReadOnlyList<Vector2> Origins { get; set; }
    }
}
