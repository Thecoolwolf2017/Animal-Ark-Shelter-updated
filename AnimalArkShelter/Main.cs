using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AnimalArkShelter
{
    public class Main : Script
    {
        public Main()
        {
            try
            {
                _iniFile = ScriptSettings.Load(@"scripts\AnimalArkShelter.ini");
                _petInteractionMenuKey = _iniFile.GetValue("Keys", "InteractionMenuKey", Keys.J);
                float x = _iniFile.GetValue("Shop", "X", 597.0833f);
                float y = _iniFile.GetValue("Shop", "Y", 2800.7881f);
                float z = _iniFile.GetValue("Shop", "Z", 41.3537f);
                _shopPos = new Vector3(x, y, z);
            }
            catch
            {
                _petInteractionMenuKey = Keys.J;
                _shopPos = new Vector3(597.0833f, 2800.7881f, 41.3537f);
            }

            ShopBlip = World.CreateBlip(_shopPos);
            if (ShopBlip != null)
            {
                ShopBlip.Sprite = BlipSprite.Store;
                ShopBlip.Color = BlipColor.Yellow;
                ShopBlip.IsShortRange = true;
                ShopBlip.Name = "Animal Ark Pet Store";
            }

            BuyPetMenu.EnsureMenu();
            BuyPetStuffMenu.EnsureMenu();
            PetInteractionMenu.EnsureMenu();

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;
        }

        private void OnAborted(object sender, EventArgs e)
        {
            try
            {
                if (Pet != null && Pet.Exists())
                {
                    Pet.IsInvincible = false;
                    Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, Pet.Handle, false);
                }
            }
            catch { }

            try { Function.Call(Hash.RENDER_SCRIPT_CAMS, false, true, 500, true, false, 0); } catch { }
            try { BuyPetMenu.CleanupShowcase(); } catch { }
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                UiPool.Process();

                // Shop marker
                try
                {
                    World.DrawMarker(
                        MarkerType.Cone,
                        _shopPos,
                        Vector3.Zero,
                        Vector3.Zero,
                        new Vector3(0.6f, 0.6f, 0.6f),
                        Color.FromArgb(180, 255, 165, 0),
                        false, false, false,
                        null, null, false
                    );
                }
                catch { }

                // Shop prompt
                if (Game.Player.Character.Position.DistanceTo(_shopPos) <= 1.5f)
                {
                    int pid = Function.Call<int>(Hash.PLAYER_ID);
                    int wanted = Function.Call<int>(Hash.GET_PLAYER_WANTED_LEVEL, pid);
                    if (wanted > 0)
                    {
                        Utils.DisplayHelpTextThisFrame("Lose the cops before shopping.");
                    }
                    else
                    {
                        Utils.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to browse pets & supplies.");
                        if (Game.IsControlJustPressed(GTA.Control.Context))
                        {
                            BuyPetMenu.Init();
                            IsShopping = true;
                        }
                    }
                }

                // Pet HUD & lifecycle
                if (HasPet && Pet != null && Pet.Exists())
                {
                    Hud.Update(PetName, Pet.Health, Pet.MaxHealth);
                    Hud.Draw();

                    if (Pet.IsDead)
                    {
                        PetInteractionMenu.Hide();
                        Hud.Hide();
                        if (PetBlip != null && PetBlip.Exists()) { PetBlip.Delete(); PetBlip = null; }
                        Utils.Notify($"~r~{PetName} has died.");
                        HasPet = false;
                        Pet = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.Notify("~r~AnimalArkShelter OnTick: " + ex.Message);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == _petInteractionMenuKey && HasPet && Pet != null && Pet.Exists())
            {
                PetInteractionMenu.Toggle();
            }
        }

        // --- Shared state ---
        public static ObjectPool UiPool { get; } = [];

        public static bool IsShopping { get; set; } = false;
        public static bool HasPet { get; set; } = false;

        public static Ped Pet;
        public static string PetName = "Pet";

        public static Vector3 _shopPos;
        public static Blip ShopBlip;
        public static Blip PetBlip;

        private readonly Keys _petInteractionMenuKey;
        private readonly ScriptSettings _iniFile;
    }
}
