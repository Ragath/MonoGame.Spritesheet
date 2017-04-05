using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Newtonsoft.Json;

namespace MonoGame.Spritesheet.Pipeline
{
    [ContentImporter(".json", DefaultProcessor = nameof(SheetFolderProcessor), DisplayName = "SheetFolder Importer - Spritesheet")]
    public class SheetFolderImporter : ContentImporter<SheetFolder>
    {
        public override SheetFolder Import(string filename, ContentImporterContext context)
        {
            var data = JsonConvert.DeserializeObject<SheetFolder>(File.ReadAllText(filename));
            if (!Directory.Exists(data.FolderPath))
                throw new DirectoryNotFoundException(data.FolderPath);
            var files = Directory.GetFiles(data.FolderPath, data.Filter);


            var texImporter = new TextureImporter();
            var textures = new List<TextureContent>(files.Length);
            foreach (var f in files)
            {
                context.AddDependency(f);
                var texture = texImporter.Import(f, context);
                texture.Name = Path.GetFileNameWithoutExtension(f);
                textures.Add(texture);
            }
            data.Textures = textures.ToArray();

            return data;
        }
    }
}
