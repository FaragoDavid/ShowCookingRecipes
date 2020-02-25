using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;

namespace ShowCookingRecipes {

    class DrawUtil {
        private static readonly int globalOffsetX = 32;
        private static readonly int globalOffsetY = 32;
        private static readonly int textOffsetX = 16;
        private static readonly int textOffsetY = 16;

        /// <summary>
        /// Draws the bounding box of the cooking object hover tooltip.
        /// </summary>
        /// <param name="boxPositionX">Position of the bounding box on the X (horizontal) axis</param>
        /// <param name="boxPositionY">Position of the bounding box on the Y (vertical) axis</param>
        /// <param name="boxDimensions">Width and height of the bounding box</param>
        public static void DrawBoundingBox(int boxPositionX, int boxPositionY, Vector2 boxDimensions) {
            // -- Part of the spritesheet containing the texture we want to draw
            Rectangle _menuTextureSourceRect = new Rectangle(0, 256, 60, 60);

            IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, _menuTextureSourceRect, color: Color.White, scale: ModEntry.ZoomLevel,
                x: globalOffsetX + boxPositionX, 
                y: globalOffsetY + boxPositionY,
                width: (int)boxDimensions.X + 20, 
                height: (int)boxDimensions.Y
            );
        }

        /// <summary>
        /// Draws the description part of the cooking object hover tooltip.
        /// </summary>
        /// <param name="cookingRecipeTimesCooked">The description string to draw</param>
        /// <param name="textPositionX">Position of the text on the X (horizontal) axis</param>
        /// <param name="textPositionY">Position of the text on the Y (vertical) axis</param>
        public static void DrawDescription(string cookingRecipeDescription, Vector2 textPosition) {
            Utility.drawTextWithShadow(
                b: Game1.spriteBatch,
                text: cookingRecipeDescription,
                font: Game1.smallFont,
                position: new Vector2(
                    globalOffsetX + textOffsetX + textPosition.X,
                    globalOffsetY + textOffsetY + textPosition.Y),
                color: Game1.textColor
            );
        }

        /// <summary>
        /// Draws the name part of the cooking object hover tooltip.
        /// </summary>
        /// <param name="cookingRecipeName">The name string to draw</param>
        /// <param name="textPositionX">Position of the text on the X (horizontal) axis</param>
        /// <param name="textPositionY">Position of the text on the Y (vertical) axis</param>
        // TODO: change cooking recipe name to cooking object name
        public static void DrawName(string cookingRecipeName, int textPositionX, int textPositionY) {
            Utility.drawTextWithShadow(
                b: Game1.spriteBatch,
                text: cookingRecipeName,
                font: Game1.smallFont,
                position: new Vector2(
                    globalOffsetX + textOffsetX + textPositionX,
                    globalOffsetY + textOffsetY + textPositionY),
                color: Game1.textColor
            );
        }

        /// <summary>
        /// Draws the price line of the cooking object hover tooltip.
        /// </summary>
        /// <param name="cookingRecipePrice">The price string to draw</param>
        /// <param name="textPositionX">Position of the text on the X (horizontal) axis</param>
        /// <param name="textPositionY">Position of the text on the Y (vertical) axis</param>
        // TODO: Add coin icon before price -- offset price by coin icon width and some (32?)  
        public static void DrawPrice(string cookingRecipePrice, Vector2 textPosition) {
            DrawPriceCoin(cookingRecipePrice, textPosition);
            DrawPriceText(cookingRecipePrice, textPosition);
        }

        /// <summary>
        /// Draws the coin for the price line of the cooking object hover tooltip.
        /// </summary>
        /// <param name="textPositionX">Position of the text on the X (horizontal) axis</param>
        /// <param name="textPositionY">Position of the text on the Y (vertical) axis</param>
        private static void DrawPriceCoin(string cookingRecipePrice, Vector2 textPosition) {
            Rectangle _coinSpriteSourceRectangle = Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16);

            Game1.spriteBatch.Draw(
                texture: Game1.debrisSpriteSheet,
                position: new Vector2(
                    globalOffsetX + textOffsetX + textPosition.X + Game1.smallFont.MeasureString(cookingRecipePrice).X + 20,
                    globalOffsetY + textOffsetY + textPosition.Y + 12
                ),
                sourceRectangle: _coinSpriteSourceRectangle,
                color: Color.White,
                rotation: 0.0f,
                origin: new Vector2(8f, 8f),
                scale: 4f,
                effects: SpriteEffects.None,
                layerDepth: 0.95f
            );
        }

        /// <summary>
        /// Draws the text for the price line of the cooking object hover tooltip.
        /// </summary>
        /// <param name="cookingRecipePrice">The price string to draw</param>
        /// <param name="textPositionX">Position of the text on the X (horizontal) axis</param>
        /// <param name="textPositionY">Position of the text on the Y (vertical) axis</param>
        private static void DrawPriceText(string cookingRecipePrice, Vector2 textPosition) {
            Utility.drawTextWithShadow(
                b: Game1.spriteBatch,
                text: cookingRecipePrice,
                font: Game1.smallFont,
                position: new Vector2(
                    globalOffsetX + textOffsetX + textPosition.X,
                    globalOffsetY + textOffsetY + textPosition.Y),
                color: Game1.textColor
            );
        }

        /// <summary>
        /// Draws the description part of the cooking object hover tooltip.
        /// </summary>
        /// <param name="cookingRecipeTimesCooked">The number of times, the recipe was cooked</param>
        /// <param name="textPositionX">Position of the text on the X (horizontal) axis</param>
        /// <param name="textPositionY">Position of the text on the Y (vertical) axis</param>
        public static void DrawTimesCooked(int cookingRecipeTimesCooked, Vector2 textPosition) {
            Utility.drawTextWithShadow(
                b: Game1.spriteBatch,
                text: "Times cooked: " + cookingRecipeTimesCooked,
                font: Game1.smallFont,
                position: new Vector2(
                    globalOffsetX + textOffsetX + textPosition.X,
                    globalOffsetY + textOffsetY + textPosition.Y),
                color: Game1.textColor
            );
        }
    }
}
