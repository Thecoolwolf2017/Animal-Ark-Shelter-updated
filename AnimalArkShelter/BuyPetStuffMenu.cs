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
            ShopMenu = new NativeMenu("ANIMAL ARK", "Shelter Supplies", "");
            Main.UiPool.Add(ShopMenu);
            ShopMenu.Add(new NativeItem("Treat (+10 HP)", "Quick snack to heal a bit"));
            ShopMenu.Add(new NativeItem("Pet Food (+25 HP)", "Hearty meal for your pet"));
            ShopMenu.Add(new NativeItem("Medkit (+100 HP)", "Full heal"));
            ShopMenu.Add(new NativeItem("Rename Tag...", "Rename your pet"));

            ShopMenu.ItemActivated += (s, e) =>
            {
                if (!Main.HasPet || Main.Pet == null || !Main.Pet.Exists()) { Utils.Notify("~o~You need a pet."); return; }

                var title = (e.Item as NativeItem)?.Title ?? string.Empty;
                try
                {
                    if (title.StartsWith("Treat"))
                    {
                        Main.Pet.Health = System.Math.Min(Main.Pet.MaxHealth, Main.Pet.Health + 10);
                        Utils.Notify($"Gave a treat. {Main.PetName}: {Main.Pet.Health}/{Main.Pet.MaxHealth}");
                    }
                    else if (title.StartsWith("Pet Food"))
                    {
                        Main.Pet.Health = System.Math.Min(Main.Pet.MaxHealth, Main.Pet.Health + 25);
                        Utils.Notify($"Fed {Main.PetName}. {Main.Pet.Health}/{Main.Pet.MaxHealth}");
                    }
                    else if (title.StartsWith("Medkit"))
                    {
                        Main.Pet.Health = Main.Pet.MaxHealth;
                        Utils.Notify($"Healed {Main.PetName} fully.");
                    }
                    else if (title.StartsWith("Rename"))
                    {
                        string typed = Utils.GetUserTextInput("Enter Pet Name", Main.PetName, 20);
                        if (!string.IsNullOrWhiteSpace(typed)) Main.PetName = typed.Trim();
                        Utils.Notify($"Renamed to {Main.PetName}.");
                    }
                }
                catch { }
            };
        }

        public static void Init()
        {
            if (!Main.HasPet) { Utils.Notify("~o~You need a pet to buy supplies."); return; }
            EnsureMenu();
            ShopMenu.Visible = true;
        }
    }
}
