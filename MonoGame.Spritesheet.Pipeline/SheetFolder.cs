using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace MonoGame.Spritesheet.Pipeline;

public class SheetFolder
{
    [Required]
    public string FolderPath { get; set; }
    [Required]
    public string Filter { get; set; }
    [JsonIgnore]
    public IReadOnlyList<TextureContent> Textures { get; set; }
}
