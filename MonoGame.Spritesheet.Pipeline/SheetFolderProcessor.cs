using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Spritesheet.Pipeline.Packing;
using Newtonsoft.Json;
using MonoGame.Spritesheet.Pipeline.Utils;

namespace MonoGame.Spritesheet.Pipeline
{
    [ContentProcessor(DisplayName = "SheetFolder - Spritesheet")]
    public class SheetFolderProcessor : ContentProcessor<SheetFolder, SheetContent>
    {
        [DefaultValue(typeof(Color), "255,0,255,255")]
        public Color ColorKeyColor { get; set; } = Color.Magenta;
        [DefaultValue(true)]
        public bool ColorKeyEnabled { get; set; } = true;
        //public bool GenerateMipmaps { get; set; }
        [DefaultValue(true)]
        public bool PremultiplyAlpha { get; set; } = true;
        public bool ResizeToPowerOfTwo { get; set; }
        public bool MakeSquare { get; set; }
        public TextureProcessorOutputFormat TextureFormat { get; set; } = TextureProcessorOutputFormat.Color;

        [DefaultValue(0)]
        public int Padding { get; set; } = 0;


        public override SheetContent Process(SheetFolder input, ContentProcessorContext context)
        {
            var sprites = input.Textures.ToDictionary(i => i.Name, i => i.Faces.Single().Single().GetBounds());

            var sources = new Rectangle[sprites.Count];
            var names = new Dictionary<string, int>(sources.Length);
            {
                int i = 0;
                foreach (var s in sprites)
                {
                    names.Add(s.Key, i);
                    sources[i] = s.Value;
                    i++;
                }
            }

            var origins = TrimSources(ref sources, input.Textures, names, ColorKeyEnabled ? ColorKeyColor : Color.TransparentBlack, Padding);
            var destinations = Packer.Pack(sources);
            //Deflate
            for (int i = 0; i < destinations.Length; i++)
            {
                ref Rectangle dst = ref destinations[i];
                dst.Inflate(-Padding, -Padding);
            }

            var tmp = PackTexture(sources, destinations, input.Textures, names, Padding);

            var texture = context.Convert<TextureContent, Texture2DContent>(tmp, nameof(TextureProcessor), context.Parameters);
            var result = new SheetContent
            {
                Texture = texture,
                Names = names,
                Sources = destinations,
                Origins = origins
            };

            context.Logger.LogMessage($"Fillrate: {(double)result.Sources.GetArea() / result.Sources.GetUnionArea()}");
            return result;
        }

        static IReadOnlyList<Vector2> TrimSources(ref Rectangle[] sources, IEnumerable<TextureContent> textures, Dictionary<string, int> names, Color colorKey, int padding)
        {
            var origins = new List<Vector2>(sources.Length);
            foreach (var texture in textures)
            {
                var sourceBitmap = texture.Faces.Single().Single();
                var destinationBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
                BitmapContent.Copy(sourceBitmap, destinationBitmap);

                ref Rectangle src = ref sources[names[texture.Name]];
                var offset = Cropping.TrimRect(ref src, destinationBitmap, colorKey);
                origins.Add(-offset);
                //Inflate
                src.Inflate(padding, padding);
            }
            return origins;
        }

        static Texture2DContent PackTexture(Rectangle[] sources, Rectangle[] destinations, IEnumerable<TextureContent> textures, Dictionary<string, int> names, int padding)
        {
            if (sources.Length != destinations.Length)
                throw new ArgumentException("Array mismatch");

            var destBitmap = new PixelBitmapContent<Color>(destinations.Max(r => r.Right), destinations.Max(r => r.Bottom));
            foreach (var texture in textures)
            {

                var face = texture.Faces.Single();

                var bitmap = face.Single();
                var i = names[texture.Name];
                var src = sources[i];

                //Deflate
                src.Inflate(-padding, -padding);
                //Blit
                BitmapContent.Copy(bitmap, src, destBitmap, destinations[i]);
            }

            var result = new Texture2DContent();
            result.Mipmaps.Add(destBitmap);

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
