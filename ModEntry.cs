using System;
using System.Collections.Generic;
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

        private int currentCollectionTabKey = 0;
        private int currentCollectionTabPage = 0;
        private bool eventsSubscribed = false;
        private string cookingObject;
        private int cookingObjectRawItemIndex;
        private CraftingRecipe cookingRecipe;
        private CollectionsPage collectionsPage;

        public static float ZoomLevel => 1.0f; // SMAPI's draw call will handle zoom

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

            // Local declarations
            string _name = cookingRecipe.getNameFromIndex(cookingObjectRawItemIndex);
            string _description = GetDescriptionFromIndex(cookingObjectRawItemIndex);
            int _timesCooked = Game1.player.recipesCooked.ContainsKey(cookingObjectRawItemIndex) ? Game1.player.recipesCooked[cookingObjectRawItemIndex] : 0;
            string _price = GetPriceFromIndex(cookingObjectRawItemIndex);

            // Draw bounding box
            // TODO: Fix width + height
            string _fullText = _name + Environment.NewLine
                + _description + Environment.NewLine;
            if (_timesCooked > 0) {
                _fullText += "Times cooked: " + _timesCooked + Environment.NewLine;
            }
            _fullText += _price/* + Environment.NewLine + ingredientListText*/;
            DrawUtil.DrawBoundingBox(_mousePositionX, _mousePositionY, Game1.smallFont.MeasureString(_fullText));

            // Draw name text
            DrawUtil.DrawName(_name, _mousePositionX, _mousePositionY);

            // Draw description text
            int _descriptionYOffset = (int) Game1.smallFont.MeasureString(_name + Environment.NewLine).Y,
                _descriptionY = _mousePositionY + _descriptionYOffset;
            DrawUtil.DrawDescription(_description, new Vector2(_mousePositionX, _descriptionY));

            // Draw times cooked text if it had ever been cooked
            int _timesCookedY = _descriptionY;
            if (_timesCooked > 0) {
                int _timesCookedYOffset = (int)Game1.smallFont.MeasureString(_timesCooked.ToString() + Environment.NewLine).Y;
                _timesCookedY += _timesCookedYOffset;
                DrawUtil.DrawTimesCooked(_timesCooked, new Vector2(_mousePositionX, _timesCookedY));
            }

            // Draw price
            int _priceYOffset = (int)Game1.smallFont.MeasureString(_description + Environment.NewLine).Y;
            if(_timesCooked > 0) {
                _priceYOffset += (int)Game1.smallFont.MeasureString(_timesCooked.ToString() + Environment.NewLine).Y;
            }
            int _priceY = _timesCookedY + _priceYOffset;
            DrawUtil.DrawPrice(_price, new Vector2(_mousePositionX, _priceY));

            // Draw ingredients
            //DrawIngredients();
        }

        /// <summary>
        /// Some cooking objects are abbreviated in the cookingRecipe collection, so their names have to be changed when creating a new CraftingRecipe.
        /// </summary>
        /// <param name="cookingObjectName"></param>
        /// <returns></returns>
        private void SetCookingRecipe(string cookingObjectName) {
            switch (cookingObjectName) {
                case "Cheese Cauliflower": cookingRecipe = new CraftingRecipe("Cheese Cauli.", true); break;
                case "Eggplant Parmesan": cookingRecipe = new CraftingRecipe("Eggplant Parm.", true); break;
                case "Vegetable Medley": cookingRecipe = new CraftingRecipe("Vegetable Stew", true); break;
                case "Cookie": cookingRecipe = new CraftingRecipe("Cookies", true); break;
                case "Cranberry Sauce": cookingRecipe = new CraftingRecipe("Cran. Sauce", true); break;
                case "Dish O' The Sea": cookingRecipe = new CraftingRecipe("Dish o' The Sea", true); break;
                default: cookingRecipe = new CraftingRecipe(cookingObjectName, true); break;
            }
        }

        private string GetDescriptionFromIndex(int index) {
            if (index > 0) {
                return Game1.parseText(Game1.objectInformation[index].Split('/')[5], Game1.smallFont, 256);
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

                Monitor.Log(cookingRecipe.DisplayName);

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
                Monitor.Log(cookingObject.Split('/')[4]);
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

            Monitor.Log("currentCollectionPageTab set to: " + currentCollectionTabKey);

            UpdateCurrentCollectionTabPage(0);
        }

        private void UpdateCurrentCollectionTabPage(int pageIndex, bool isIncrement = false) {
            if (isIncrement) {
                currentCollectionTabPage += pageIndex;
            } else {
                currentCollectionTabPage = pageIndex;
            }

            Monitor.Log("currentCollectionTabPage set to " + currentCollectionTabPage);
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
                    SetCookingRecipe(cookingObject.Split('/')[4]);
                }
            }
        }
    }
}