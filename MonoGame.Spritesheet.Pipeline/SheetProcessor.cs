using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace MonoGame.Spritesheet.Pipeline
{
    [ContentProcessor(DisplayName = "Spritesheet processor")]
    public class SheetProcessor : ContentProcessor<TextureContent, SheetContent>
    {
        [DefaultValue(typeof(Color), "255,0,255,255")]
        public Color ColorKeyColor { get; set; } = Color.Magenta;
        [DefaultValue(true)]
        public bool ColorKeyEnabled { get; set; } = true;
        public bool GenerateMipmaps { get; set; }
        [DefaultValue(true)]
        public bool PremultiplyAlpha { get; set; } = true;
        public bool ResizeToPowerOfTwo { get; set; }
        public bool MakeSquare { get; set; }
        public TextureProcessorOutputFormat TextureFormat { get; set; } = TextureProcessorOutputFormat.Color;

        [DefaultValue(16)]
        public int SpriteWidth { get; set; } = 16;
        [DefaultValue(16)]
        public int SpriteHeight { get; set; } = 16;
        [DefaultValue(0)]
        public int Padding { get; set; } = 0;

        public override SheetContent Process(TextureContent input, ContentProcessorContext context)
        {
            var texture = context.Convert<TextureContent, Texture2DContent>(input, nameof(TextureProcessor), context.Parameters);

            var result = new SheetContent
            {
                Texture = texture,
                SpriteWidth = SpriteWidth,
                SpriteHeight = SpriteHeight,
                Padding = Padding
            };
            return result;
        }
    }
}
