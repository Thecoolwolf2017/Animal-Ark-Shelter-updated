using GTA;
using GTA.Math;
using GTA.Native;
using LemonUI.Menus;

namespace AnimalArkShelter
{
    public class PetInteractionMenu : Script
    {
        public static NativeMenu InteractionMenu;

        // Track in-vehicle pose state (removed unused backing fields)

        public PetInteractionMenu() { }

        public static void EnsureMenu()
        {
            if (InteractionMenu != null) return;

            InteractionMenu = new NativeMenu("", "PET INTERACTION", "");
            // Ensure the menu is processed and drawn
            Main.UiPool.Add(InteractionMenu);

            var follow = new NativeItem("Follow", "Have your pet follow you");
            var stay = new NativeItem("Stay", "Tell your pet to stay here");
            var come = new NativeItem("Come Here", "Call your pet to your position");
            var sit = new NativeItem("Sit", "Make your pet sit");
            var lie = new NativeItem("Lay Down", "Make your pet lay down");
            var enterVeh = new NativeItem("Enter Vehicle", "Put your pet into your vehicle");
            var exitVeh = new NativeItem("Exit Vehicle", "Have your pet exit the vehicle");
            var heal = new NativeItem("Treat (+10 HP)", "Feed a quick treat");
            var dismiss = new NativeItem("Dismiss Pet", "Send your pet home");

            InteractionMenu.Add(follow);
            InteractionMenu.Add(stay);
            InteractionMenu.Add(come);
            InteractionMenu.Add(sit);
            InteractionMenu.Add(lie);
            InteractionMenu.Add(enterVeh);
            InteractionMenu.Add(exitVeh);
            InteractionMenu.Add(heal);
            InteractionMenu.Add(dismiss);

            InteractionMenu.ItemActivated += (s, e) =>
            {
                var item = e.Item;
                if (Main.Pet == null || !Main.Pet.Exists()) return;

                if (item == follow) DoFollow();
                else if (item == stay) DoStay();
                else if (item == come) DoComeHere();
                else if (item == sit) DoSit();
                else if (item == lie) DoLay();
                else if (item == enterVeh) DoEnterVehicle();
                else if (item == exitVeh) DoExitVehicle();
                else if (item == heal) DoHeal();
                else if (item == dismiss) DoDismiss();
            };
        }

        public static void Toggle()
        {
            if (InteractionMenu == null) EnsureMenu();
            InteractionMenu.Visible = !InteractionMenu.Visible;
        }

        public static void Hide()
        {
            if (InteractionMenu != null) InteractionMenu.Visible = false;
        }

        private static void DoFollow()
        {
            try
            {
                var me = Game.Player.Character;
                Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, Main.Pet.Handle, Function.Call<int>(Hash.GET_PLAYER_GROUP, Function.Call<int>(Hash.PLAYER_ID)));
                Function.Call(Hash.SET_PED_NEVER_LEAVES_GROUP, Main.Pet.Handle, true);
                Function.Call(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, Main.Pet.Handle, me.Handle, 0.0f, -1.2f, 0.0f, 2.2f, -1, 2.0f, true);
                Utils.Notify($"{Main.PetName} is following.");
            }
            catch { }
        }

        private static void DoStay()
        {
            try
            {
                Function.Call(Hash.CLEAR_PED_TASKS, Main.Pet.Handle);
                Function.Call(Hash.REMOVE_PED_FROM_GROUP, Main.Pet.Handle);
                Utils.Notify($"{Main.PetName} will stay.");
            }
            catch { }
        }

        private static void DoComeHere()
        {
            try
            {
                var me = Game.Player.Character;
                float dist = me.Position.DistanceTo(Main.Pet.Position);

                // Break out of any idle/scenario/group so the pet can move
                try { Function.Call(Hash.REMOVE_PED_FROM_GROUP, Main.Pet.Handle); } catch { }
                try { Function.Call(Hash.CLEAR_PED_TASKS, Main.Pet.Handle); } catch { }

                if (Utils.ComeWarpIfFar && dist > Utils.ComeWarpDistance)
                {
                    Main.Pet.Position = me.Position + me.ForwardVector * -1.0f;
                    Main.Pet.Task.ClearAll();
                }
                else
                {
                    // Proper argument order: duration, stopRange, speed, p5, p6
                    Function.Call(Hash.TASK_GO_TO_ENTITY, Main.Pet.Handle, me.Handle, -1, Utils.ComeStopRange, Utils.ComeRunSpeed, 0, 0);
                    int t = Game.GameTime + Utils.ComeStuckTeleportMs;
                    while (Game.GameTime < t)
                    {
                        Script.Wait(0);
                        if (me.Position.DistanceTo(Main.Pet.Position) <= Utils.ComeStopRange + 0.4f) break;
                        if (!Main.Pet.Exists() || Main.Pet.IsDead) break;
                    }
                }

                Function.Call(Hash.TASK_TURN_PED_TO_FACE_ENTITY, Main.Pet.Handle, me.Handle, -1);
                Utils.Notify($"{Main.PetName} is here.");
            }
            catch { }
        }

        private static bool IsInAnyVehicle(Ped p)
        {
            try { return Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, p.Handle, false); } catch { }
            return false;
        }

        private static void PlayAnim(Ped p, string dict, string name, int flags = 1, int duration = -1)
        {
            try
            {
                Function.Call(Hash.REQUEST_ANIM_DICT, dict);
                int t = Game.GameTime + 1500;
                while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict) && Game.GameTime < t) Script.Wait(0);
                Function.Call(Hash.TASK_PLAY_ANIM, p.Handle, dict, name, 8.0f, -8.0f, duration, flags, 0.0f, false, false, false);
            }
            catch { }
        }

        private static void DoSit()
        {
            if (IsInAnyVehicle(Main.Pet))
            {
                Utils.Notify("~o~Can't sit while in a vehicle.");
                return;
            }

            try
            {
                // Remove from group and clear tasks so animations stick
                Function.Call(Hash.REMOVE_PED_FROM_GROUP, Main.Pet.Handle);
                Function.Call(Hash.CLEAR_PED_TASKS, Main.Pet.Handle);
                Script.Wait(100);

                if (Utils.IsDogModel(Main.Pet.Model))
                {
                    // Use explicit sit anim for dogs (more reliable than scenario)
                    PlayAnim(Main.Pet, "creatures@rottweiler@amb@world_dog_sitting@base", "base", 1, -1);
                }
                else
                {
                    // Cat substitute
                    PlayAnim(Main.Pet, "creatures@cat@amb@world_cat_sleeping_ledge@base", "base", 1, -1);
                }
                Utils.Notify($"{Main.PetName} sits.");
            }
            catch { }
        }

        private static void DoLay()
        {
            if (IsInAnyVehicle(Main.Pet))
            {
                Utils.Notify("~o~Can't lay down while in a vehicle.");
                return;
            }

            try
            {
                if (Utils.IsDogModel(Main.Pet.Model))
                {
                    PlayAnim(Main.Pet, "creatures@rottweiler@amb@sleep_in_kennel@", "sleep_in_kennel", 1, -1);
                }
                else
                {
                    PlayAnim(Main.Pet, "creatures@cat@amb@world_cat_sleeping_ground@base", "base", 1, -1);
                }
                Utils.Notify($"{Main.PetName} lays down.");
            }
            catch { }
        }

        private static void DoEnterVehicle()
        {
            try
            {
                var me = Game.Player.Character;
                var veh = me.CurrentVehicle;
                if (veh == null || !veh.Exists())
                {
                    Utils.Notify("~o~No vehicle.");
                    return;
                }

                VehicleSeat seat = VehicleSeat.Any;
                try { seat = (VehicleSeat)Utils.VehicleSeatIndex; } catch { }

                Function.Call(Hash.TASK_ENTER_VEHICLE, Main.Pet.Handle, veh.Handle, -1, (int)seat, 1.2f, 1, 0);

                // Wait until actually inside, then apply pose
                int t = Game.GameTime + 3000;
                while (Game.GameTime < t && !IsInAnyVehicle(Main.Pet)) { Script.Wait(50); }
                if (IsInAnyVehicle(Main.Pet))
                {
                    // Clear idle to ensure anim sticks
                    Function.Call(Hash.CLEAR_PED_TASKS, Main.Pet.Handle);
                    if (Utils.VehiclePose == 0) PlayAnim(Main.Pet, "creatures@rottweiler@incar@std@ds@", "sit", 1, -1);
                    else /* lay */                    PlayAnim(Main.Pet, "creatures@rottweiler@incar@std@ds@", "sleep", 1, -1);
                }
            }
            catch { }
        }

        private static void DoExitVehicle()
        {
            try
            {
                if (!IsInAnyVehicle(Main.Pet)) { Utils.Notify("~o~Pet is not in a vehicle."); return; }

                Function.Call(Hash.TASK_LEAVE_ANY_VEHICLE, Main.Pet.Handle, 0, 0);
                Script.Wait(600);

                // cleared vehicle pose state (no stored state)

                // Nudge beside vehicle
                var veh = Game.Player.Character.CurrentVehicle;
                if (veh != null && veh.Exists())
                {
                    var side = veh.RightVector;
                    var pos = veh.Position + side * 1.2f;
                    if (Utils.TryGetGroundZ(pos, out var gz) && gz > 0f) pos = new Vector3(pos.X, pos.Y, gz + 0.05f);
                    Main.Pet.Position = pos;
                }

                Function.Call(Hash.TASK_STAND_STILL, Main.Pet.Handle, 600);
                Utils.Notify($"{Main.PetName} got out.");
            }
            catch { }
        }

        private static void DoHeal()
        {
            var pet = Main.Pet;
            try
            {
                int tgt = System.Math.Min(pet.MaxHealth, pet.Health + 10);
                pet.Health = tgt;
                Utils.Notify($"Healed {Main.PetName} to {tgt}/{pet.MaxHealth}.");
            }
            catch { }
        }

        private static void DoDismiss()
        {
            var pet = Main.Pet;
            try
            {
                if (pet == null || !pet.Exists()) return;
                if (Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, pet.Handle, false)) DoExitVehicle();

                if (Main.PetBlip != null && Main.PetBlip.Exists()) { Main.PetBlip.Delete(); Main.PetBlip = null; }
                Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, pet.Handle);
                Function.Call(Hash.REMOVE_PED_FROM_GROUP, pet.Handle);
                pet.MarkAsNoLongerNeeded();
                pet.Delete();
            }
            catch { }
            Main.Pet = null;
            Main.HasPet = false;
            BuyPetMenu.ClearHealthBar();
            Hud.Hide();
        }
    }
}
