using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using StardewModdingAPI;

namespace ShowCookingRecipes {

    static class DrawUtil {
        public static readonly int globalOffsetX = 32;
        public static readonly int globalOffsetY = 32;
        public static readonly int smallFontUpperCaseHeight = 34;
        public static readonly int smallFontLowerCaseHeight = 26;
        public static readonly int spriteOffsetX = 20;
        public static readonly int textOffsetX = 16;
        public static readonly int textOffsetY = 16;

        /// <summary>
        /// Draws the bounding box of the cooking object hover tooltip.
        /// </summary>
        public static void DrawBoundingBox(Vector2 position, int width, int height) {
            // -- Part of the spritesheet containing the box texture
            Rectangle _menuTextureSourceRect = new Rectangle(0, 256, 60, 60);

            IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, 
                sourceRect: _menuTextureSourceRect, 
                color: Color.White, 
                scale: ModEntry.ZoomLevel,
                x: (int)position.X, 
                y: (int)position.Y,
                width: width + 20, 
                height: height
            );
        }

        /// <summary>
        /// Draws the description part of the cooking object hover tooltip.
        /// </summary>
        public static void DrawDescription(string cookingRecipeDescription, Vector2 textPosition) {
            Utility.drawTextWithShadow(
                b: Game1.spriteBatch,
                text: cookingRecipeDescription,
                font: Game1.smallFont,
                position: new Vector2(textPosition.X, textPosition.Y),
                color: Game1.textColor
            );
        }

        /// <summary>
        /// Draws the name part of the cooking object hover tooltip.
        /// </summary>
        // TODO: change cooking recipe name to cooking object name
        public static void DrawName(string cookingRecipeName, Vector2 textPosition) {
            Utility.drawTextWithShadow(
                b: Game1.spriteBatch,
                text: cookingRecipeName,
                font: Game1.smallFont,
                position: new Vector2(textPosition.X, textPosition.Y),
                color: Game1.textColor
            );
        }
        /// <summary>
        /// Draws the ingredient of the cooking object hover tooltip.
        /// </summary>
        public static void DrawIngredients(Dictionary<int, int> ingredientKeyValuePairs, Vector2 position) {
            foreach (KeyValuePair<int, int> _ingredient in ingredientKeyValuePairs) {
                int _ingredientRawIndex = _ingredient.Key,
                    _quantity = _ingredient.Value;
                string _ingredientName = ModEntry.cookingRecipe.getNameFromIndex(_ingredientRawIndex);

                Vector2 _textPosition = position + new Vector2(textOffsetX + spriteOffsetX, 0);

                DrawIngredientText(_ingredientName, _textPosition);
                DrawIngredientSprite(_ingredient.Key, position);
                position.Y += smallFontUpperCaseHeight;
            }
        }

        private static void DrawIngredientSprite(int ingredientIndex, Vector2 position) {
            Rectangle _ingredientSprite = ModEntry.GetObjectSprite(ingredientIndex);

            Game1.spriteBatch.Draw(
                texture: Game1.objectSpriteSheet,
                position: new Vector2(position.X, position.Y),
                sourceRectangle: _ingredientSprite,
                color: Color.White,
                rotation: 0.0f, 
                origin: Vector2.Zero, 
                scale: 2f, 
                effects: SpriteEffects.None, 
                layerDepth: 0.86f
            );
        }

        private static void DrawIngredientText(string ingredientName, Vector2 textPosition) {
            Utility.drawTextWithShadow(
               b: Game1.spriteBatch,
               text: ingredientName,
               font: Game1.smallFont,
               position: new Vector2(textPosition.X, textPosition.Y),
               color: Game1.textColor
           );
        }

        /// <summary>
        /// Draws the price line of the cooking object hover tooltip.
        /// </summary>
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
                    textPosition.X + Game1.smallFont.MeasureString(cookingRecipePrice).X + spriteOffsetX,
                    textPosition.Y + 12
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
        private static void DrawPriceText(string cookingRecipePrice, Vector2 textPosition) {
            Utility.drawTextWithShadow(
                b: Game1.spriteBatch,
                text: cookingRecipePrice,
                font: Game1.smallFont,
                position: new Vector2(textPosition.X, textPosition.Y),
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
                text: "Times Cooked: " + cookingRecipeTimesCooked,
                font: Game1.smallFont,
                position: new Vector2(textPosition.X, textPosition.Y),
                color: Game1.textColor
            );
        }
    }
}
