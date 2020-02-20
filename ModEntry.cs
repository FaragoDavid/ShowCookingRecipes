using System;
using System.Collections.Generic;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ShowCookingRecipes {
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod {
        private static string debugMessage = null;
        private int currentCollectionPageTab = 0;
        private int currentCollectionSidetabPage = 0;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            helper.Events.Input.CursorMoved += MouseMovedOverCookingolletionItem;
            helper.Events.Input.ButtonPressed += CollectionsMenuSidetabClicked;
            helper.Events.Input.ButtonPressed += CollectionsMenuPageTurned;
        }


        /*********
        ** Private methods
        *********/

        private void CollectionsMenuPageTurned(object sender, ButtonPressedEventArgs e) {
            if (Game1.activeClickableMenu is GameMenu gameMenu) {
                if (gameMenu.pages[gameMenu.currentTab] is CollectionsPage collectionsPage) {
                    if (e.Button == SButton.MouseLeft) {
                        int newX = (int)e.Cursor.ScreenPixels.X;
                        int newY = (int)e.Cursor.ScreenPixels.Y;

                        if (collectionsPage.backButton.containsPoint(newX, newY)) {
                            currentCollectionSidetabPage -= 1;

                            // Log
                            Monitor.Log("currentCollectionPageTab set to " + currentCollectionSidetabPage);
                        } else if (collectionsPage.forwardButton.containsPoint(newX, newY)) {
                            currentCollectionSidetabPage += 1;

                            // Log
                            Monitor.Log("currentCollectionPageTab set to " + currentCollectionSidetabPage);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Caches the private currentTab property of the CollectionsPage, as it is private.
        /// </summary>
        private void CollectionsMenuSidetabClicked(object sender, ButtonPressedEventArgs e) {
            if (Game1.activeClickableMenu is GameMenu gameMenu) {
                if (gameMenu.pages[gameMenu.currentTab] is CollectionsPage collectionsPage) {
                    if (e.Button == SButton.MouseLeft) {
                        int newX = (int)e.Cursor.ScreenPixels.X;
                        int newY = (int)e.Cursor.ScreenPixels.Y;

                        foreach (KeyValuePair<int, ClickableTextureComponent> sideTab in collectionsPage.sideTabs) {
                            if (sideTab.Value.containsPoint(newX, newY)) {
                                if (currentCollectionPageTab != sideTab.Key) {
                                    // Set current page of the collection sidetab to 0
                                    currentCollectionSidetabPage = 0;

                                    // Log
                                    Monitor.Log("currentCollectionPageTab set to " + currentCollectionSidetabPage);

                                    // Set the current selected colection sidetab
                                    currentCollectionPageTab = sideTab.Key;

                                    // Log
                                    Monitor.Log("currentCollectionPageTab set to " + sideTab.Key);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void MouseMovedOverCookingolletionItem(object sender, CursorMovedEventArgs e) {
            int newX = (int)e.NewPosition.ScreenPixels.X;
            int newY = (int)e.NewPosition.ScreenPixels.Y;

            if (Game1.activeClickableMenu is GameMenu gameMenu) {
                if (gameMenu.pages[gameMenu.currentTab] is CollectionsPage collectionsPage) {
                    if (currentCollectionPageTab == CollectionsPage.cookingTab) {
                        List<List<ClickableTextureComponent>> cookingPages = collectionsPage.collections[CollectionsPage.cookingTab];
                        foreach (ClickableTextureComponent textureComponent in cookingPages[currentCollectionSidetabPage]) {

                            if (textureComponent.containsPoint(newX, newY)) {
                                //textureComponent.scale = Math.Min(textureComponent.scale + 0.02f, textureComponent.baseScale + 0.1f);
                                //textureComponent.hoverText = CreateCookingCollectionObjectDescription(Convert.ToInt32(textureComponent.name.Split(' ')[0]));
                                DrawHoverTextBox(
                                    CreateCookingCollectionObjectDescription(Convert.ToInt32(textureComponent.name.Split(' ')[0])),
                                    newX, newY
                                );
                            }
                        }

                    }
                }
            }
        }

        private string CreateCookingCollectionObjectDescription(int index) {
            string[] cookingObjectParts = Game1.objectInformation[index].Split('/');


            string displayName = cookingObjectParts[4];
            string description = cookingObjectParts[5];
            string recipe = "unknown recipe";
            if (Game1.player.recipesCooked.ContainsKey(index)) {
                recipe = Game1.content.LoadString(
                    "Strings\\UI:Collections_Description_RecipesCooked",
                    (object)Game1.player.recipesCooked[index]
                );
            }

            // debug
            LogDebugMessage("Cooking oject hovered over:");
            LogDebugMessage("  name: " + displayName + ", recipe: " + recipe);

            return displayName
                + Environment.NewLine
                + Environment.NewLine
                + Game1.parseText(description, Game1.smallFont, 256)
                + Environment.NewLine
                + Environment.NewLine
                + recipe;
        }

        private void DrawHoverTextBox(string description, int x, int y) {
                Game1.spriteBatch.End();

            Vector2 stringLength = Game1.smallFont.MeasureString(description);
            int width = (int)stringLength.X + Game1.tileSize / 2 + 40;
            int height = (int)stringLength.Y + Game1.tileSize / 3 + 5;

            /*int x = Math.Max((int)(Mouse.GetState().X / Game1.options.zoomLevel) - Game1.tileSize / 2 - width, 0);
            int y = (int)(Mouse.GetState().Y / Game1.options.zoomLevel) + Game1.tileSize / 2;

            if (y + height > Game1.graphics.GraphicsDevice.Viewport.Height) {
                y = Game1.graphics.GraphicsDevice.Viewport.Height - height;
            }*/


            LogDebugMessage( "\n" +
                "\tx: " + x + ",\n" +
                "\ty: " + y + ",\n" +
                "\twidth: " + width + ",\n" +
                "\theight: " + height
            );

            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            IClickableMenu.drawTextureBox(
                Game1.spriteBatch,
                Game1.menuTexture,
                new Rectangle(0, 256, 60, 60),
                x, y,
                width, height,
                Color.White
            );
            Game1.spriteBatch.End();
            /*Utility.drawTextWithShadow(
                Game1.spriteBatch,
                description,
                font,
                new Vector2(x + Game1.tileSize / 4, y + Game1.tileSize / 4),
                Game1.textColor
            );*/
        }

        private void LogDebugMessage(String message) {
            if (message != debugMessage) {
                Monitor.Log(message, LogLevel.Debug);
                debugMessage = message;
            }
        }
    }
}