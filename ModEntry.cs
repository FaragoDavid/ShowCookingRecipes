using System;
using System.Collections.Generic;
using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ShowCookingRecipes {
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod {
        /*********
         ** Properties
         *********/
        private readonly int COOKING_COLLECTION_SIDETAB_KEY = 4;

        private static string debugMessage = null;
        private int currentCollectionTabKey = 0;
        private int currentCollectionTabPage = 0;
        private bool eventsSubscribed = false;
        private string cookingObject;
        private int cookingObjectRawItemIndex;
        private CraftingRecipe cookingRecipe;
        private CollectionsPage collectionsPage;

        public float ZoomLevel => 1.0f; // SMAPI's draw call will handle zoom

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        /*********
        ** Private methods
        *********/

        /// <summary>
        /// Draws a tooltip box containing the name, description, number of times cooked, price and ingredient list of an item.
        /// </summary>
        private void DrawCookingCollectionItemTooltip() {

            // Local declarations
            int _mousePositionX = Game1.getOldMouseX(),
                _mousePositionY = Game1.getOldMouseY();

            cookingRecipe.getSpriteIndexFromRawIndex(cookingObjectRawItemIndex);
            // -- Part of the spritesheet containing the texture we want to draw
            Rectangle _menuTextureSourceRect = new Rectangle(0, 256, 60, 60);


            string name = cookingRecipe.getNameFromIndex(cookingObjectRawItemIndex);
            string description = GetDescriptionFromIndex(cookingObjectRawItemIndex);
            int timesCooked = cookingRecipe.timesCrafted;
            string price = GetPriceFromIndex(cookingObjectRawItemIndex);
            Dictionary<int, int> ingredientList = GetIngredientListOfCookingRecipe();

            string ingredientListText = "";
            foreach (int ingredientRawItemIndex in ingredientList.Keys) {
                int quantity = ingredientList[ingredientRawItemIndex];
                ingredientListText += (
                    Environment.NewLine
                    + quantity
                    + " "
                    + cookingRecipe.getNameFromIndex(ingredientRawItemIndex)
                );
            }


            string fullText = name + Environment.NewLine + Environment.NewLine
                + description + Environment.NewLine + Environment.NewLine;
            if (timesCooked > 0) {
                fullText += "Times cooked: " + timesCooked + Environment.NewLine;
            }
            fullText += price/* + Environment.NewLine + ingredientListText*/;


            Vector2 _stringLength = Game1.smallFont.MeasureString(fullText);
            int _textureBoxWidth = (int)_stringLength.X + Game1.tileSize / 2 + 40;
            int _textureBoxHeight = (int)_stringLength.Y + Game1.tileSize / 3 + 5;


            // Draw bounding box
            IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, _menuTextureSourceRect, color: Color.White, scale: this.ZoomLevel,
                x: _mousePositionX, y: _mousePositionY,
                width: _textureBoxWidth, height: _textureBoxHeight
            );
            // Draw text on box
            Utility.drawTextWithShadow(Game1.spriteBatch, font: Game1.smallFont, color: Game1.textColor,
                text: fullText,
                position: new Vector2(
                    _mousePositionX + Game1.tileSize / 4,
                    _mousePositionY + Game1.tileSize / 4
                )
            );
        }

        /// <summary>
        /// Some cooking objects are abbreviated in the cookingRecipe collection, so their names have to be changed when creating a new CraftingRecipe.
        /// </summary>
        /// <param name="cookingObjectName"></param>
        /// <returns></returns>
        private string GetCookingRecipeName(string cookingObjectName) {
            switch (cookingObjectName) {
                case "Cheese Cauliflower": return "Cheese Cauli.";
                case "Eggplant Parmesan": return "Eggplant Parm.";
                case "Vegetable Medley": return "Vegetable Stew";
                case "Cookie": return "Cookies";
                case "Cranberry Sauce": return "Cran. Sauce";
                case "Dish O' The Sea": return "Dish o' The Sea";
                default: return cookingObjectName;
            }
        }

        private string GetDescriptionFromIndex(int index) {
            if (index > 0) {
                return Game1.objectInformation[index].Split('/')[5];
            }

            return null;
        }

        /// <summary>
        /// Method generating a dictionary of item raw index and quantity pairs for a given object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Dictionary of index - quantity key-value pairs</returns>
        // TODO: Error handling?? What happens if the index if not of a cookable object?
        private Dictionary<int, int> GetIngredientListOfCookingRecipe() {
            try {
                // Local declarations

                LogDebugMessage(cookingRecipe.DisplayName);

                string recipeData = CraftingRecipe.cookingRecipes[cookingRecipe.DisplayName];
                string[] ingredientData = recipeData.Split('/')[0].Split(' ');

                // Result
                Dictionary<int, int> ingredientKeyToQuantity = new Dictionary<int, int>();

                for (int ingredientIndex = 0; ingredientIndex < ingredientData.Length; ingredientIndex += 2) {
                    ingredientKeyToQuantity.Add(
                        Convert.ToInt32(ingredientData[ingredientIndex]),
                        Convert.ToInt32(ingredientData[ingredientIndex + 1])
                    );
                }

                return ingredientKeyToQuantity;
            } catch (Exception e) {
                LogDebugMessage(cookingObject.Split('/')[4]);
                return new Dictionary<int, int>();
            }
        }


        /// <summary>
        /// Retrieves the sprite of an object
        /// </summary>
        /// <param name="index">Sprite index of object</param>
        /// <returns>Rectangle containint the sprite of the item.</returns>
        private Rectangle GetObjectSprite(int index) {
            return Game1.getSourceRectForStandardTileSheet(
                Game1.objectSpriteSheet,
                cookingRecipe.getSpriteIndexFromRawIndex(index),
                16,
                16
            );
        }

        private string GetPriceFromIndex(int index) {
            return Game1.objectInformation[index].Split('/')[1];
        }

        private bool IsCollectionsMenuOpen() {
            if (Game1.activeClickableMenu is GameMenu gameMenu) {
                if (gameMenu.pages[gameMenu.currentTab] is CollectionsPage) {
                    collectionsPage = (CollectionsPage)gameMenu.pages[gameMenu.currentTab];
                    return true;
                }

            }
            collectionsPage = null;

            return false;
        }

        private bool IsCollectionsMenuCookingTabOpen() {
            if (IsCollectionsMenuOpen()) {
                if (currentCollectionTabKey == COOKING_COLLECTION_SIDETAB_KEY) {
                    return true;
                }
            }

            return false;
        }

        private void LogDebugMessage(String message) {
            if (message != debugMessage) {
                Monitor.Log(message, LogLevel.Debug);
                debugMessage = message;
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e) {
            //LogDebugMessage("OnButtonPressed START");

            // Collections menu is open
            if (IsCollectionsMenuOpen()) {
                // Left mouse button pressed
                if (e.Button == SButton.MouseLeft) {
                    foreach (KeyValuePair<int, ClickableTextureComponent> _sideTab in collectionsPage.sideTabs) {
                        // Mouse is positioned on one of the tabs
                        if (_sideTab.Value.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())) {
                            UpdateCurrentCollectionTabKey(_sideTab.Key);
                        }
                    }

                    // Mouse is positioned on the back or forward arrows
                    if (collectionsPage.backButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())) {
                        UpdateCurrentCollectionTabPage(-1, true);
                    } else if (collectionsPage.forwardButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())) {
                        UpdateCurrentCollectionTabPage(1, true);
                    }
                }
            }

            // LogDebugMessage("OnButtonPressed END");
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e) {
            if (IsCollectionsMenuCookingTabOpen()) {
                UpdateHoveredCookingCollectionItem();
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e) {
            // menu closed
            if (e.NewMenu == null) {
                if (eventsSubscribed) {
                    UnsubscribeEvents();
                }
            }
        }

        /// <summary>
        /// Custom handler of the Rendered event of Helper.Events.Display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRendered(object sender, RenderedEventArgs e) {
            if (cookingRecipe != null) {
                DrawCookingCollectionItemTooltip();
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e) {
            foreach (KeyValuePair<string, string> v in CraftingRecipe.cookingRecipes) {
                LogDebugMessage(v.Key);
            }

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
        }

        /// <summary>
        /// Adds event handlers for the CursorMoved and Rendered events, when the cooking collections tab is open.
        /// </summary>
        private void SubscribeEvents() {
            eventsSubscribed = true;

            Helper.Events.Input.CursorMoved += OnCursorMoved;
            Helper.Events.Display.Rendered += OnRendered;
        }
        private void UnsubscribeEvents() {
            eventsSubscribed = false;

            Helper.Events.Input.CursorMoved -= OnCursorMoved;
            Helper.Events.Display.Rendered -= OnRendered;
        }

        /// <summary>
        /// Sets the current collection tab key and page properties, and (un)subscribes cooking tab events as needed.
        /// </summary>
        /// <param name="tabKey"></param>
        private void UpdateCurrentCollectionTabKey(int tabKey) {
            currentCollectionTabKey = tabKey;

            if (tabKey == COOKING_COLLECTION_SIDETAB_KEY) {
                SubscribeEvents();
            } else {
                UnsubscribeEvents();
            }

            LogDebugMessage("currentCollectionPageTab set to: " + currentCollectionTabKey);

            UpdateCurrentCollectionTabPage(0);
        }

        private void UpdateCurrentCollectionTabPage(int pageIndex, bool isIncrement = false) {
            if (isIncrement) {
                currentCollectionTabPage += pageIndex;
            } else {
                currentCollectionTabPage = pageIndex;
            }

            LogDebugMessage("currentCollectionTabPage set to " + currentCollectionTabPage);
        }

        /// <summary>
        /// Changes the hovered texture component reference to the current one, or null if the cursor is not above one.
        /// </summary>
        private void UpdateHoveredCookingCollectionItem() {
            // Local declaratons
            List<List<ClickableTextureComponent>> _cookingPages = collectionsPage.collections[CollectionsPage.cookingTab];

            // Reset hovered texture component to null
            cookingRecipe = null;

            foreach (ClickableTextureComponent textureComponent in _cookingPages[currentCollectionTabPage]) {
                if (textureComponent.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())) {
                    cookingObjectRawItemIndex = Convert.ToInt32(textureComponent.name.Split(' ')[0]);
                    cookingObject = Game1.objectInformation[cookingObjectRawItemIndex];
                    cookingRecipe = new CraftingRecipe(GetCookingRecipeName(cookingObject.Split('/')[4]), true);
                }
            }
        }
    }
}