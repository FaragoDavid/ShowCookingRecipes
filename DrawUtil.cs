using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System;

namespace ShowCookingRecipes {

    static class DrawUtil {
        public static readonly int smallFontUpperCaseHeight = 34;
        public static readonly int smallFontLowerCaseHeight = 26;
        public static readonly int spriteOffsetX = 20;
        public static readonly int textOffset = 16;
        public static readonly int tooltipOffset = 32;

        /// <summary>
        /// Draws the bounding box of the cooking object hover tooltip.
        /// </summary>
        public static void DrawBoundingBox(Vector2 position, int width, int height) {
            // Part of the spritesheet containing the box texture
            Rectangle _menuTextureSourceRect = new Rectangle(0, 256, 60, 60);

            IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, 
                sourceRect: _menuTextureSourceRect, 
                color: Color.White, 
                scale: 1f,
                x: (int)position.X, 
                y: (int)position.Y,
                width: width, 
                height: height
            );
        }

        /// <summary>
        /// Draws a tooltip box containing the name, description, number of times cooked, price and ingredient list of an item.
        /// </summary>
        public static void DrawCookingCollectionItemTooltip(string name, string description, int timesCooked, string price, Dictionary<int, int> ingredientKeyValuePairs) {
            Vector2 _boundingBoxPosition = new Vector2(
                Game1.getOldMouseX() + tooltipOffset,
                Game1.getOldMouseY() + tooltipOffset
            );

            int _boundingBoxHeight = (int)Game1.smallFont.MeasureString(name).Y
                + (int)Game1.smallFont.MeasureString(description).Y
                + (int)Game1.smallFont.MeasureString(price).Y
                + ingredientKeyValuePairs.Count * smallFontUpperCaseHeight
                + (2 * smallFontLowerCaseHeight);
            int _boundingBoxWidth = Math.Max(
                (int)Game1.smallFont.MeasureString(name).X,
                (int)Game1.smallFont.MeasureString(description).X
            );
            foreach (KeyValuePair<int, int> keyValuePair in ingredientKeyValuePairs) {
                int _ingredientNameWidth = (int)Game1.smallFont.MeasureString(ModEntry.cookingRecipe.getNameFromIndex(keyValuePair.Key)).X,
                    _ingredientLineWidth = spriteOffsetX + textOffset + _ingredientNameWidth;
                if (_boundingBoxWidth < _ingredientLineWidth) {
                    _boundingBoxWidth = _ingredientLineWidth;
                }
            }

            if (timesCooked > 0) {
                _boundingBoxHeight += (int)Game1.smallFont.MeasureString(timesCooked.ToString()).Y + smallFontLowerCaseHeight / 2;
                _boundingBoxWidth = Math.Max(
                    _boundingBoxWidth,
                    (int)Game1.smallFont.MeasureString("Times Cooked: " + timesCooked.ToString()).X
                );
            }

            // Add padding
            _boundingBoxWidth += (2 * textOffset);
            _boundingBoxHeight += (2 * textOffset);

            // Reposition if the tooltip is outside the visible screen
            if (Utility.getSafeArea().Right < _boundingBoxPosition.X + _boundingBoxWidth) {
                _boundingBoxPosition.X -= ((_boundingBoxPosition.X + _boundingBoxWidth) - Utility.getSafeArea().Right);
            }
            if (Utility.getSafeArea().Bottom < _boundingBoxPosition.Y + _boundingBoxHeight) {
                _boundingBoxPosition.Y -= ((_boundingBoxPosition.Y + _boundingBoxHeight) - Utility.getSafeArea().Bottom);
            }

            DrawBoundingBox(_boundingBoxPosition, _boundingBoxWidth, _boundingBoxHeight);

            Vector2 _textPosition = _boundingBoxPosition + new Vector2(textOffset, textOffset);
            DrawName(name, _textPosition);
            _textPosition.Y += Game1.smallFont.MeasureString(name).Y + smallFontLowerCaseHeight / 2;

            DrawDescription(description, _textPosition);
            _textPosition.Y += Game1.smallFont.MeasureString(description).Y + smallFontLowerCaseHeight / 2;

            if (timesCooked > 0) {
                DrawTimesCooked(timesCooked, _textPosition);
                _textPosition.Y += Game1.smallFont.MeasureString(timesCooked.ToString()).Y + smallFontLowerCaseHeight / 2;
            }

            DrawPrice(price, _textPosition);
            _textPosition.Y += Game1.smallFont.MeasureString(price).Y + smallFontLowerCaseHeight / 2;

            DrawIngredients(ingredientKeyValuePairs, _textPosition);
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

                Vector2 _textPosition = position + new Vector2(textOffset + spriteOffsetX, 0);

                DrawIngredientText(_ingredientName, _textPosition);
                DrawIngredientSprite(_ingredientRawIndex, position);
                DrawIngredientQuantity(_quantity, position);
                position.Y += smallFontUpperCaseHeight;
            }
        }

        /// <summary>
        /// Draws an ingredient quantity on the cooking object hover tooltip.
        /// </summary>
        private static void DrawIngredientQuantity(int quantity, Vector2 position) {
            float _quantityWidth = Game1.tinyFont.MeasureString(quantity.ToString()).X;

            // Why 18 though
            position += new Vector2(32 - _quantityWidth, 18);

            Utility.drawTinyDigits(
                toDraw: quantity,
                b: Game1.spriteBatch,
                position: position,
                scale: 2f,
                layerDepth: 0.87f,
                c: Color.AntiqueWhite
            );
        }

        /// <summary>
        /// Draws an ingredient sprite on the cooking object hover tooltip.
        /// </summary>
        private static void DrawIngredientSprite(int ingredientIndex, Vector2 position) {
            Rectangle _ingredientSprite = GetObjectSprite(ingredientIndex);

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

        /// <summary>
        /// Draws an ingredient name on the cooking object hover tooltip.
        /// </summary>
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
        public static void DrawTimesCooked(int cookingRecipeTimesCooked, Vector2 textPosition) {
            Utility.drawTextWithShadow(
                b: Game1.spriteBatch,
                text: "Times Cooked: " + cookingRecipeTimesCooked,
                font: Game1.smallFont,
                position: new Vector2(textPosition.X, textPosition.Y),
                color: Game1.textColor
            );
        }

        /// <summary>
        /// Retrieves the sprite of an object
        /// </summary>
        /// <param name="index">Sprite index of object</param>
        /// <returns>Rectangle containint the sprite of the item.</returns>
        public static Rectangle GetObjectSprite(int index) {
            return Game1.getSourceRectForStandardTileSheet(
                Game1.objectSpriteSheet,
                ModEntry.cookingRecipe.getSpriteIndexFromRawIndex(index),
                16,
                16
            );
        }
    }
}
