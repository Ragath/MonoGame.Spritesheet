﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Spritesheet
{
    public sealed class GridSheet
    {
        int _SpriteWidth;
        int _SpriteHeight;
        int _Padding;

        int ColumnSize { get; set; }
        int RowSize { get; set; }


        public Texture2D Texture { get; private set; }

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


        public Vector2 SpriteSize => new Vector2(SpriteWidth, SpriteHeight);

        public Rectangle this[int column, int row] => new Rectangle(column * ColumnSize + Padding, row * RowSize + Padding, SpriteWidth, SpriteHeight);


        static int GetCellSize(int spriteSize, int padding) => spriteSize + padding * 2;
    }
}
