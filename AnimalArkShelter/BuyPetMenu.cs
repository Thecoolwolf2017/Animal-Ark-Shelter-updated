using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI.Menus;
using System;

namespace AnimalArkShelter
{
    public class BuyPetMenu : Script
    {
        public static NativeMenu ShopMenu;
        public static Ped ShowcaseAnimal;
        public static Ped Shopkeeper;

        // Native camera (robust across SHVDN3 nightlies)
        private static int _shopCam = 0;

        private static NativeItem _suppliesItem;

        private class AnimalDef(string name, string model, int price)
        {
            public string Name = name;
            public string Model = model;
            public int Price = price;
        }

        private static readonly AnimalDef[] Animals =
        [
            new("Cat",        "a_c_cat_01",     500),
            new("Husky",      "a_c_husky",      1200),
            new("Westy",      "a_c_westy",      800),
            new("Poodle",     "a_c_poodle",     900),
            new("Rottweiler", "a_c_rottweiler", 1100),
            new("Retriever",  "a_c_retriever",  1100),
            new("Pug",        "a_c_pug",        850),
            new("Chop",       "a_c_chop",       1500),
        ];

        public BuyPetMenu()
        {
            Tick += OnTick;
            Aborted += (s, e) => CleanupCamera();
        }

        public static void EnsureMenu()
        {
            if (ShopMenu != null) return;

            ShopMenu = new NativeMenu("ANIMAL ARK", "Pet Store", "");
            Main.UiPool.Add(ShopMenu);

            // Add animals first
            for (int i = 0; i < Animals.Length; i++)
            {
                var a = Animals[i];
                var it = new NativeItem(a.Name, "Adopt this animal", "$" + a.Price);
                ShopMenu.Add(it);
            }

            // Supplies at the end (no separator to avoid visual gap); enable only if player has a pet
            _suppliesItem = new NativeItem("Supplies...", "Buy items for your pet")
            {
                Enabled = Main.HasPet
            };
            ShopMenu.Add(_suppliesItem);

            ShopMenu.ItemActivated += (s, e) =>
            {
                if (e.Item == _suppliesItem)
                {
                    if (!Main.HasPet) { Utils.Notify("~o~You need a pet first."); return; }
                    ShopMenu.Visible = false; // prevent overlap
                    try { BuyPetStuffMenu.Init(); } catch { }
                    return;
                }

                // Animal adoption
                int idx = ShopMenu.SelectedIndex;
                if (idx >= 0 && idx < Animals.Length)
                {
                    AdoptFromShowcase(idx);
                }
            };
        }

        public static void Init()
        {
            EnsureMenu();
            Main.IsShopping = true;

            if (ShowcaseAnimal == null || !ShowcaseAnimal.Exists())
            {
                SpawnOrSwapShowcase(Animals[0].Model);
                _lastIndexShown = 0;
            }

            EnsureShopkeeper();

            SetupShopCamera();
            ShopMenu.Visible = true;
        }

        public static void CleanupShowcase()
        {
            try
            {
                if (ShowcaseAnimal != null && ShowcaseAnimal.Exists())
                {
                    ShowcaseAnimal.MarkAsNoLongerNeeded();
                    ShowcaseAnimal.Delete();
                }
            }
            catch { }
            ShowcaseAnimal = null;

            try
            {
                if (Shopkeeper != null && Shopkeeper.Exists())
                {
                    Shopkeeper.MarkAsNoLongerNeeded();
                    Shopkeeper.Delete();
                }
            }
            catch { }
            Shopkeeper = null;
        }

        public static void ClearHealthBar() { try { Hud.Hide(); } catch { } }

        private static void CleanupCamera()
        {
            try
            {
                if (_shopCam != 0 && Function.Call<bool>(Hash.DOES_CAM_EXIST, _shopCam))
                {
                    Function.Call(Hash.RENDER_SCRIPT_CAMS, false, true, Utils.ShopEaseTimeMs, true, false, 0);
                    Function.Call(Hash.DESTROY_CAM, _shopCam, false);
                }
            }
            catch { }
            _shopCam = 0;
        }

        private static int _lastIndexShown = -1;

        // Fixed world offsets from the shop anchor (not player-facing)
        private static Vector3 ShowcasePos => Main._shopPos + Utils.ShowcaseOffset;
        private static Vector3 CameraPos => Main._shopPos + Utils.CameraOffset;

        private void OnTick(object sender, EventArgs e)
        {
            // Keep supplies enabled state synced; no “gap” items used
            if (_suppliesItem != null) _suppliesItem.Enabled = Main.HasPet;

            if (ShopMenu != null && ShopMenu.Visible)
            {
                int idx = ShopMenu.SelectedIndex;
                if (idx != _lastIndexShown && idx >= 0 && idx < Animals.Length)
                {
                    SpawnOrSwapShowcase(Animals[idx].Model);
                    _lastIndexShown = idx;
                }

                if (Shopkeeper == null || !Shopkeeper.Exists()) EnsureShopkeeper();
            }
            else
            {
                if (_shopCam != 0) CleanupCamera();
            }
        }

        private static void SetupShopCamera()
        {
            try
            {
                CleanupCamera();

                _shopCam = Function.Call<int>(Hash.CREATE_CAM, "DEFAULT_SCRIPTED_CAMERA", true);
                if (_shopCam != 0 && Function.Call<bool>(Hash.DOES_CAM_EXIST, _shopCam))
                {
                    // Keep camera safely above ground
                    var camPos = CameraPos;
                    try { if (Utils.TryGetGroundZ(camPos, out var gz) && camPos.Z < gz + 0.6f) camPos = new Vector3(camPos.X, camPos.Y, gz + 0.6f); } catch { }
                    Function.Call(Hash.SET_CAM_COORD, _shopCam, camPos.X, camPos.Y, camPos.Z);
                    Function.Call(Hash.SET_CAM_ROT, _shopCam, 0.0f, 0.0f, 0.0f, 2);
                    Function.Call(Hash.SET_CAM_FOV, _shopCam, Utils.ShopFov);
                    // Aim at the actual ped if available, otherwise fallback; slight Z lift for framing
                    var look = (ShowcaseAnimal != null && ShowcaseAnimal.Exists()) ? ShowcaseAnimal.Position : ShowcasePos;
                    Function.Call(Hash.POINT_CAM_AT_COORD, _shopCam, look.X, look.Y, look.Z + 0.3f);
                    Function.Call(Hash.SET_CAM_ACTIVE, _shopCam, true);
                    Function.Call(Hash.RENDER_SCRIPT_CAMS, true, true, Utils.ShopEaseTimeMs, true, false, 0);
                }
            }
            catch { }
        }

        private static void SpawnOrSwapShowcase(string modelName)
        {
            try
            {
                if (ShowcaseAnimal != null && ShowcaseAnimal.Exists())
                {
                    ShowcaseAnimal.MarkAsNoLongerNeeded();
                    ShowcaseAnimal.Delete();
                }
            }
            catch { }

            var model = new Model(modelName);
            model.Request(500);
            if (!model.IsInCdImage || !model.IsValid) return;

            var pos = ShowcasePos;
            if (Utils.TryGetGroundZ(pos, out var groundZ) && groundZ > 0f)
                pos = new Vector3(pos.X, pos.Y, groundZ + 0.05f);

            ShowcaseAnimal = World.CreatePed(model, pos);
            if (ShowcaseAnimal != null && ShowcaseAnimal.Exists())
            {
                // Face the camera direction
                var toCam = CameraPos - ShowcaseAnimal.Position; toCam.Z = 0f;
                if (toCam.Length() > 0.001f)
                {
                    float heading = (float)(System.Math.Atan2(toCam.Y, toCam.X) * 180.0 / System.Math.PI);
                    ShowcaseAnimal.Heading = heading;
                }
                ShowcaseAnimal.IsPersistent = true;
                Function.Call(Hash.SET_PED_CAN_BE_TARGETTED, ShowcaseAnimal.Handle, false);
                Function.Call(Hash.SET_PED_CAN_RAGDOLL, ShowcaseAnimal.Handle, false);
                Function.Call(Hash.TASK_STAND_STILL, ShowcaseAnimal.Handle, -1);
            }
        }

        private static void EnsureShopkeeper()
        {
            try
            {
                if (Shopkeeper != null && Shopkeeper.Exists()) return;
                var model = new Model(Utils.ShopkeeperModel);
                model.Request(500);
                if (!model.IsInCdImage || !model.IsValid) return;

                var pos = Main._shopPos + Utils.ShopkeeperOffset;
                if (Utils.TryGetGroundZ(pos, out var gz) && gz > 0f) pos = new Vector3(pos.X, pos.Y, gz + 0.05f);

                Shopkeeper = World.CreatePed(model, pos);
                if (Shopkeeper != null && Shopkeeper.Exists())
                {
                    var look = (ShowcaseAnimal != null && ShowcaseAnimal.Exists()) ? ShowcaseAnimal.Position : ShowcasePos;
                    var dir = look - Shopkeeper.Position; dir.Z = 0f;
                    if (dir.Length() > 0.001f)
                    {
                        float heading = (float)(System.Math.Atan2(dir.Y, dir.X) * 180.0 / System.Math.PI);
                        Shopkeeper.Heading = heading;
                    }
                    Shopkeeper.IsPersistent = true;
                    Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, Shopkeeper.Handle, true);
                    Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, Shopkeeper.Handle, "WORLD_HUMAN_STAND_IMPATIENT", 0, true);
                }
            }
            catch { }
        }

        private static void AdoptFromShowcase(int index)
        {
            if (Main.HasPet && Main.Pet != null && Main.Pet.Exists())
            {
                Utils.Notify("~o~You already have a pet.");
                return;
            }
            if (ShowcaseAnimal == null || !ShowcaseAnimal.Exists()) return;

            // Convert showcase ped into the adopted pet (prevents duplication)
            Main.Pet = ShowcaseAnimal;
            ShowcaseAnimal = null;

            Main.PetName = Animals[index].Name;
            Main.HasPet = true;

            // Prompt for a custom name
            try
            {
                string typed = Utils.GetUserTextInput("Enter Pet Name", Main.PetName, 20);
                if (!string.IsNullOrWhiteSpace(typed)) Main.PetName = typed.Trim();
            }
            catch { }

            try
            {
                int playerGroup = Function.Call<int>(Hash.GET_HASH_KEY, "PLAYER");
                int petGroup = Function.Call<int>(Hash.GET_HASH_KEY, "PET_ANIMAL");
                if (petGroup != 0)
                {
                    Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, 0, playerGroup, petGroup);
                    Function.Call(Hash.SET_RELATIONSHIP_BETWEEN_GROUPS, 0, petGroup, playerGroup);
                    Function.Call(Hash.SET_PED_RELATIONSHIP_GROUP_HASH, Main.Pet.Handle, petGroup);
                }
                Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, Main.Pet.Handle, true);
                Function.Call(Hash.SET_CAN_ATTACK_FRIENDLY, Main.Pet.Handle, false, false);
                Main.Pet.IsPersistent = true;
                Main.Pet.KeepTaskWhenMarkedAsNoLongerNeeded = true;
            }
            catch { }

            // Optional showcase walk-off beat
            try
            {
                if (Utils.EnableWalkOffAnim && _shopCam != 0)
                {
                    var ahead = ShowcasePos + Game.Player.Character.ForwardVector * Utils.WalkOffDistance;
                    Function.Call(Hash.TASK_GO_STRAIGHT_TO_COORD, Main.Pet.Handle, ahead.X, ahead.Y, ahead.Z, 1.2f, 2500, 0f, 0f);
                    Script.Wait(350);
                }
            }
            catch { }

            // Reposition near player safely
            var me = Game.Player.Character;
            if (!Utils.TryFindSafeCoordNear(me.Position, 6.0f, out var spawn))
            {
                spawn = me.Position + me.ForwardVector * 1.2f;
                if (Utils.TryGetGroundZ(spawn, out var gz) && gz > 0f) spawn = new Vector3(spawn.X, spawn.Y, gz + 0.1f);
            }
            Main.Pet.Position = spawn;
            Main.Pet.Heading = me.Heading;

            // Blip
            try
            {
                if (Main.PetBlip != null && Main.PetBlip.Exists()) Main.PetBlip.Delete();
                Main.PetBlip = Main.Pet.AddBlip();
                Main.PetBlip.Color = BlipColor.Yellow;
                Main.PetBlip.Name = Main.PetName;
            }
            catch { }

            // HUD
            Hud.Show(Main.PetName);

            ShopMenu.Visible = false;
            Main.IsShopping = false;
            CleanupCamera();

            // Auto-follow
            try
            {
                Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, Main.Pet.Handle, Function.Call<int>(Hash.GET_PLAYER_GROUP, Function.Call<int>(Hash.PLAYER_ID)));
                Function.Call(Hash.SET_PED_NEVER_LEAVES_GROUP, Main.Pet.Handle, true);
                Function.Call(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, Main.Pet.Handle, me.Handle, 0.0f, -1.2f, 0.0f, 2.2f, -1, 2.0f, true);
            }
            catch { }

            Utils.Notify($"~g~Adopted {Main.PetName}~s~.");
        }
    }
}
