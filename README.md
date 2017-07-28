# MonoGame.Spritesheet
[![Build status](https://ci.appveyor.com/api/projects/status/t4gruw3f5upjt8k7/branch/master?svg=true)](https://ci.appveyor.com/project/Ragath/monogame-spritesheet/branch/master)  
Everything you need to load and draw sprites from spritesheets in MonoGame!

## Nugets
- [MonoGame.Spritesheet](https://www.nuget.org/packages/MonoGame.Spritesheet/)
- [MonoGame.Spritesheet.Pipeline](https://www.nuget.org/packages/MonoGame.Spritesheet.Pipeline/)

## Packed sheet example
A packed **`Sheet`** is a spritesheet with it's sprites packed together to reduce texture size.
```
spriteBatch.Begin();
var sheet = Content.Load<Sheet>("MyPackedSheet");
spriteBatch.Draw(sheet.Texture, position: Vector2.Zero, source: sheet["SpriteName"], color: Color.White, origin: sheet.GetOrigin("SpriteName"));
spriteBatch.End();
```

## Grid-sheet example
A **`GridSheet`** is a spritesheet with a grid of sprites.
```
  spriteBatch.Begin();
  var sheet = Content.Load<GridSheet>("MyPackedSheet");
  spriteBatch.Draw(sheet.Texture, position: Vector2.Zero, source: sheet[column, row], color: Color.White);
  spriteBatch.End();
```
