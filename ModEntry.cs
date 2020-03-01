using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ShowCookingRecipes {
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod {
        private readonly int cookingCollectionTabKey = 4;
        private int currentCollectionTabKey = 0;
        private int currentCollectionTabPage = 0;
        private bool eventsSubscribed = false;
        private string cookingObject;
        private int cookingObjectRawItemIndex;
        private CollectionsPage collectionsPage;

        public static CraftingRecipe cookingRecipe;

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
        /// Some cooking objects are abbreviated in the cookingRecipe collection, so their names have to be changed when creating a new CraftingRecipe.
        /// </summary>
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
        /// Generates a dictionary of item raw index and quantity pairs for a given object.
        /// </summary>
        private Dictionary<int, int> GetIngredientListOfCookingRecipe() {
            string _recipeData = CraftingRecipe.cookingRecipes[cookingRecipe.DisplayName];
            string[] _ingredientData = _recipeData.Split('/')[0].Split(' ');
            Dictionary<int, int> _ingredientKeyToQuantity = new Dictionary<int, int>();

            for (int ingredientIndex = 0; ingredientIndex < _ingredientData.Length; ingredientIndex += 2) {
                _ingredientKeyToQuantity.Add(
                    Convert.ToInt32(_ingredientData[ingredientIndex]),
                    Convert.ToInt32(_ingredientData[ingredientIndex + 1])
                );
            }

            return _ingredientKeyToQuantity;
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
                if (currentCollectionTabKey == cookingCollectionTabKey) {
                    return true;
                }
            }

            return false;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e) {
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
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e) {
            if (IsCollectionsMenuCookingTabOpen()) {
                UpdateHoveredCookingCollectionItem();
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e) {
            // Menu closed
            if (e.NewMenu == null) {
                if (eventsSubscribed) {
                    UnsubscribeEvents();
                }
            }
        }

        /// <summary>
        /// Custom handler of the Rendered event of Helper.Events.Display
        /// </summary>
        private void OnRendered(object sender, RenderedEventArgs e) {
            if (cookingRecipe != null) {
                int _timesCooked = Game1.player.recipesCooked.ContainsKey(cookingObjectRawItemIndex)
                    ? Game1.player.recipesCooked[cookingObjectRawItemIndex]
                    : 0;

                DrawUtil.DrawCookingCollectionItemTooltip(
                    name: cookingRecipe.getNameFromIndex(cookingObjectRawItemIndex),
                    description: GetDescriptionFromIndex(cookingObjectRawItemIndex),
                    _timesCooked,
                    price: GetPriceFromIndex(cookingObjectRawItemIndex),
                    ingredientKeyValuePairs: GetIngredientListOfCookingRecipe()
                );
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

            if (tabKey == cookingCollectionTabKey) {
                SubscribeEvents();
            } else {
                UnsubscribeEvents();
            }

            UpdateCurrentCollectionTabPage(0);
        }

        private void UpdateCurrentCollectionTabPage(int pageIndex, bool isIncrement = false) {
            if (isIncrement) {
                currentCollectionTabPage += pageIndex;
            } else {
                currentCollectionTabPage = pageIndex;
            }
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