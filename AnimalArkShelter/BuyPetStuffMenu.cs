using GTA;
using GTA.Native;
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
            ShopMenu.Add(new NativeItem("Water (+5 HP)", "Small sip to recover a bit"));
            ShopMenu.Add(new NativeItem("Pet Food (+25 HP)", "Hearty meal for your pet"));
            ShopMenu.Add(new NativeItem("Bandage (+50 HP)", "Patch up some wounds"));
            ShopMenu.Add(new NativeItem("Medkit (+100 HP)", "Full heal"));
            ShopMenu.Add(new NativeItem("Whistle (Call Here)", "Calls your pet to you"));
            ShopMenu.Add(new NativeItem("Leash (Toggle Follow/Stay)", "Switch between follow and stay"));
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
                    else if (title.StartsWith("Water"))
                    {
                        Main.Pet.Health = System.Math.Min(Main.Pet.MaxHealth, Main.Pet.Health + 5);
                        Utils.Notify($"Gave water. {Main.PetName}: {Main.Pet.Health}/{Main.Pet.MaxHealth}");
                    }
                    else if (title.StartsWith("Pet Food"))
                    {
                        Main.Pet.Health = System.Math.Min(Main.Pet.MaxHealth, Main.Pet.Health + 25);
                        Utils.Notify($"Fed {Main.PetName}. {Main.Pet.Health}/{Main.Pet.MaxHealth}");
                    }
                    else if (title.StartsWith("Bandage"))
                    {
                        Main.Pet.Health = System.Math.Min(Main.Pet.MaxHealth, Main.Pet.Health + 50);
                        Utils.Notify($"Bandaged {Main.PetName}. {Main.Pet.Health}/{Main.Pet.MaxHealth}");
                    }
                    else if (title.StartsWith("Medkit"))
                    {
                        Main.Pet.Health = Main.Pet.MaxHealth;
                        Utils.Notify($"Healed {Main.PetName} fully.");
                    }
                    else if (title.StartsWith("Whistle"))
                    {
                        // Light-weight come here
                        var me = Game.Player.Character;
                        Function.Call(Hash.REMOVE_PED_FROM_GROUP, Main.Pet.Handle);
                        Function.Call(Hash.CLEAR_PED_TASKS, Main.Pet.Handle);
                        Function.Call(Hash.TASK_GO_TO_ENTITY, Main.Pet.Handle, me.Handle, -1, Utils.ComeStopRange, Utils.ComeRunSpeed, 0, 0);
                        Utils.Notify($"Whistled for {Main.PetName}.");
                    }
                    else if (title.StartsWith("Leash"))
                    {
                        // Toggle follow/stay
                        if (Game.Player.Character.Position.DistanceTo(Main.Pet.Position) > 2.0f)
                        {
                            Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, Main.Pet.Handle, Function.Call<int>(Hash.GET_PLAYER_GROUP, Function.Call<int>(Hash.PLAYER_ID)));
                            Function.Call(Hash.SET_PED_NEVER_LEAVES_GROUP, Main.Pet.Handle, true);
                            Function.Call(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, Main.Pet.Handle, Game.Player.Character.Handle, 0.0f, -1.2f, 0.0f, 2.2f, -1, 2.0f, true);
                            Utils.Notify($"{Main.PetName} is following.");
                        }
                        else
                        {
                            Function.Call(Hash.CLEAR_PED_TASKS, Main.Pet.Handle);
                            Function.Call(Hash.REMOVE_PED_FROM_GROUP, Main.Pet.Handle);
                            Utils.Notify($"{Main.PetName} will stay.");
                        }
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
