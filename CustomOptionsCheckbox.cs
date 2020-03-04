using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowCookingRecipes {
    class CustomOptionsCheckbox : StardewValley.Menus.OptionsCheckbox {
        public new bool isChecked;

        public CustomOptionsCheckbox(string label, int whichOption) : base(label, whichOption) {
            SetCheckBoxToProperValue(whichOption);
        }

        private void ChangeCheckBoxOption(int whichOption, bool isChecked) {
            ModEntry.Mod.Monitor.Log("WhichOption: " + whichOption + ", isChecked: " + isChecked, LogLevel.Debug);

            switch (whichOption) {
                case 0: ModConfig.ShowUnknownRecipes = isChecked; break;
            }
        }

        public override void receiveLeftClick(int x, int y) {
            if (!greyedOut) {
                Game1.playSound("drumkit6");

                base.receiveLeftClick(x, y);
                isChecked = !isChecked;

                ChangeCheckBoxOption(whichOption, isChecked);
            }
        }

        private void SetCheckBoxToProperValue(int whichOption) {
            switch (whichOption) {
                case 0: isChecked = ModConfig.ShowUnknownRecipes; break;
            }
        }
    }
}
