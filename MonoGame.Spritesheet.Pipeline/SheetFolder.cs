using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Newtonsoft.Json;

namespace MonoGame.Spritesheet.Pipeline
{
    public class SheetFolder
    {
        [JsonRequired]
        public string FolderPath { get; set; }
        [JsonRequired]
        public string Filter { get; set; }
        [JsonIgnore]
        public IReadOnlyList<TextureContent> Textures { get; set; }
    }
}
