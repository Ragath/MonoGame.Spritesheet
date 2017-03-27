using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Newtonsoft.Json;

namespace MonoGame.Spritesheet.Pipeline
{
    [ContentProcessor(DisplayName = "Sheet - Spritesheet")]
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

        [DefaultValue(0)]
        public int Padding { get; set; } = 0;

        [DefaultValue("SheetData.json")]
        public string SheetData { get; set; } = "SheetData.json";


        public override SheetContent Process(TextureContent input, ContentProcessorContext context)
        {
            var texture = context.Convert<TextureContent, Texture2DContent>(input, nameof(TextureProcessor), context.Parameters);

            if (!string.IsNullOrWhiteSpace(SheetData))
                context.AddDependency(SheetData);

            var sprites = LoadSpritebounds(SheetData);

            var sources = new Rectangle[sprites.Count];
            var names = new Dictionary<string, int>(sources.Length);
            int i = 0;
            foreach (var s in sprites)
            {
                names.Add(s.Key, i);
                sources[i] = s.Value;
                i++;
            }

            var result = new SheetContent
            {
                Texture = texture,
                Names = names,
                Sources = sources
            };
            return result;
        }

        static Dictionary<string, Rectangle> LoadSpritebounds(string path)
        {
            if (File.Exists(path))
                return JsonConvert.DeserializeObject<Dictionary<string, Rectangle>>(File.ReadAllText(path));
            else
                return new Dictionary<string, Rectangle>();
        }
    }
}
