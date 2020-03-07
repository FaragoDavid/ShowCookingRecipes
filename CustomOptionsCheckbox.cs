using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowCookingRecipes {
    class CustomOptionsCheckbox : StardewValley.Menus.OptionsCheckbox {
        private readonly ModEntry mod;

        public CustomOptionsCheckbox(string label, int whichOption, ModEntry mod) : base(label, whichOption) {
            this.mod = mod;

            SetCheckBoxToProperValue(whichOption);
        }

        /// <summary>
        /// Handles changing the value of the appropriate property of the config class.
        /// </summary>
        private void ChangeCheckBoxOption(int whichOption, bool isChecked) {
            switch (whichOption) {
                case 0:
                    mod.config.ShowUnknownRecipes = isChecked; 
                    break;
            }

            mod.Helper.WriteConfig(mod.config);
        }

        /// <summary>
        /// Event handler of the mouse left click event. 
        /// Invokes 'base.receiveLeftClick' to handle changing the state of the 'isChecked' field.
        /// </summary>
        public override void receiveLeftClick(int x, int y) {
            if (!greyedOut) {
                Game1.playSound("drumkit6");

                base.receiveLeftClick(x, y);

                ChangeCheckBoxOption(whichOption, isChecked);
            }
        }

        /// <summary>
        /// Changes the value of the checkbox to that of the appropriate property of the config class.
        /// </summary>
        private void SetCheckBoxToProperValue(int whichOption) {
            switch (whichOption) {
                case 0:
                    isChecked = mod.config.ShowUnknownRecipes;
                    break;
            }
        }
    }
}
