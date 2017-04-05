using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Spritesheet;

namespace GameSample
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        GridSheet OverworldSheet { get; set; }

        Sheet EnemySheet { get; set; }
        Sheet CharacterSheet { get; set; }
        Rectangle[] WalkingAnimation { get; set; }


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            OverworldSheet = Content.Load<GridSheet>("Overworld");

            CharacterSheet = Content.Load<Sheet>("character");
            EnemySheet = Content.Load<Sheet>("Enemy");

            WalkingAnimation = new[]
            {
                CharacterSheet["Walking0"],
                CharacterSheet["Walking4"],
                CharacterSheet["Walking8"],
                CharacterSheet["Walking12"]
            };
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            {
                //Draw terrain
                var tileSize = OverworldSheet.SpriteSize;
                for (int y = 0; y < 10; y++)
                    for (int x = 0; x < 10; x++)
                        spriteBatch.Draw(OverworldSheet.Texture, new Vector2(x, y) * tileSize, OverworldSheet[0, 0], Color.White);

                //Draw log
                spriteBatch.Draw(OverworldSheet.Texture, Vector2.UnitX * 0 * tileSize, OverworldSheet[3, 5], Color.White);
                for (int x = 1; x < 4; x++)
                    spriteBatch.Draw(OverworldSheet.Texture, Vector2.UnitX * x * tileSize, OverworldSheet[4, 5], Color.White);
                spriteBatch.Draw(OverworldSheet.Texture, Vector2.UnitX * 4 * tileSize, OverworldSheet[5, 5], Color.White);

                //Draw character sprites
                spriteBatch.Draw(CharacterSheet.Texture, Vector2.UnitX * 16, CharacterSheet[0], Color.White);
                spriteBatch.Draw(CharacterSheet.Texture, Vector2.UnitX * 48 + Vector2.UnitY * 16, GetFrameSource(WalkingAnimation, 0.7, gameTime), Color.White);

                //Draw enemy
                spriteBatch.Draw(EnemySheet.Texture, Vector2.UnitX * 32 + Vector2.UnitY * 48, EnemySheet[0], Color.White);

            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        static Rectangle GetFrameSource(Rectangle[] animation, double duration, GameTime gameTime)
        {
            var i = (int)(gameTime.TotalGameTime.TotalSeconds * animation.Length / duration % animation.Length);
            return animation[i];
        }
    }
}
