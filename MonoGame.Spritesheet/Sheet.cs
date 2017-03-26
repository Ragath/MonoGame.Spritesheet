using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Spritesheet
{
    public sealed class Sheet
    {
        int _SpriteWidth;
        int _SpriteHeight;
        int _Padding;

        int ColumnSize { get; set; }
        int RowSize { get; set; }

        public int SpriteWidth
        {
            get => _SpriteWidth;
            private set
            {
                if (_SpriteWidth != value)
                {
                    _SpriteWidth = value;
                    ColumnSize = GetCellSize(SpriteWidth, Padding);
                }
            }
        }
        public int SpriteHeight
        {
            get => _SpriteHeight;
            private set
            {
                if (_SpriteHeight != value)
                {
                    _SpriteHeight = value;
                    RowSize = GetCellSize(SpriteHeight, Padding);
                }
            }
        }
        public int Padding
        {
            get => _Padding;
            private set
            {
                if (_Padding != value)
                {
                    _Padding = value;
                    ColumnSize = GetCellSize(SpriteWidth, Padding);
                    RowSize = GetCellSize(SpriteHeight, Padding);
                }
            }
        }

        public Texture2D Texture { get; set; }
        //public IReadOnlyDictionary<string, Rectangle> Sources { get; set; }

        public string Name => Texture.Name;
        
        public Rectangle GetSource(int column, int row)
            => new Rectangle(column * ColumnSize + Padding, row * RowSize + Padding, SpriteWidth, SpriteHeight);

        static int GetCellSize(int spriteSize, int padding) => spriteSize + padding * 2;
    }
}
