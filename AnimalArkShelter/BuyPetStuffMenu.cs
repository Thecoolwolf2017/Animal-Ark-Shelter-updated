using GTA;
using LemonUI.Menus;

namespace AnimalArkShelter
{
    public class BuyPetStuffMenu : Script
    {
        public static NativeMenu ShopMenu;

        public static void EnsureMenu()
        {
            if (ShopMenu != null) return;
            ShopMenu = new NativeMenu("ANIMAL ARK", "Supplies", "");
            Main.UiPool.Add(ShopMenu);
            ShopMenu.Add(new NativeItem("Treats", "Quick +10 HP treat"));
            ShopMenu.Add(new NativeItem("Basic Collar", "A simple collar"));
        }

        public static void Init()
        {
            if (!Main.HasPet) { Utils.Notify("~o~You need a pet to buy supplies."); return; }
            EnsureMenu();
            ShopMenu.Visible = true;
        }
    }
}
