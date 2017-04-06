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
using MonoGame.Spritesheet.Pipeline.Utils;
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
        //public bool GenerateMipmaps { get; set; }
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
            if (!string.IsNullOrWhiteSpace(SheetData))
                context.AddDependency(SheetData);

            var sprites = LoadSpritebounds(SheetData);

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

            var origins = TrimSources(ref sources, input, ColorKeyEnabled ? ColorKeyColor : Color.TransparentBlack, Padding);
            var destinations = Packer.Pack(sources);
            //Deflate
            for (int i = 0; i < destinations.Length; i++)
            {
                ref Rectangle dst = ref destinations[i];
                dst.Inflate(-Padding, -Padding);
            }

            PackTexture(sources, destinations, ref input, Padding);

            var texture = context.Convert<TextureContent, Texture2DContent>(input, nameof(TextureProcessor), context.Parameters);
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

        static IReadOnlyList<Vector2> TrimSources(ref Rectangle[] sources, TextureContent texture, Color colorKey, int padding)
        {
            var sourceBitmap = texture.Faces.Single().Single();
            var destinationBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
            BitmapContent.Copy(sourceBitmap, destinationBitmap);

            var origins = new Vector2[sources.Length];
            for (int i = 0; i < sources.Length; i++)
            {
                ref Rectangle src = ref sources[i];
                origins[i] = -Cropping.TrimRect(ref src, destinationBitmap, colorKey);
                //Inflate
                src.Inflate(padding, padding);
            }
            return origins;
        }

        static void PackTexture(Rectangle[] sources, Rectangle[] destinations, ref TextureContent texture, int padding)
        {
            if (sources.Length != destinations.Length)
                throw new ArgumentException("Array mismatch");
            var face = texture.Faces.Single();
            var bitmap = face.Single();
            var destBitmap = new PixelBitmapContent<Color>(destinations.Max(r => r.Right), destinations.Max(r => r.Bottom));
            for (int i = 0; i < sources.Length; i++)
            {
                var src = sources[i];

                //Deflate
                src.Inflate(-padding, -padding);
                //Blit
                BitmapContent.Copy(bitmap, src, destBitmap, destinations[i]);
            }


            face[0] = destBitmap;
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
