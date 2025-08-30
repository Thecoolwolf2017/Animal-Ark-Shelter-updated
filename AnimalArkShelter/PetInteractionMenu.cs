using System;
using GTA;
using GTA.Native;
using GTA.Math;
using LemonUI;
using LemonUI.Menus;

namespace AnimalArkShelter
{
    public class PetInteractionMenu : Script
    {
        public static NativeMenu InteractionMenu;

        // Track "posed in vehicle" state so we can detach cleanly
        private static bool _vehPoseActive = false;
        private static int  _vehPoseVehHandle = 0;
        private static VehicleSeat _vehPoseSeat = VehicleSeat.Passenger;

        public PetInteractionMenu() { }

        public static void EnsureMenu()
        {
            if (InteractionMenu != null) return;

            InteractionMenu = new NativeMenu("", "PET INTERACTION", "");

            var follow    = new NativeItem("Follow", "Have your pet follow you");
            var stay      = new NativeItem("Stay", "Tell your pet to stay here");
            var come      = new NativeItem("Come Here", "Call your pet to your position");
            var sit       = new NativeItem("Sit", "Make your pet sit");
            var lie       = new NativeItem("Lay Down", "Make your pet lay down");
            var enterVeh  = new NativeItem("Enter Vehicle", "Put your pet into your vehicle");
            var exitVeh   = new NativeItem("Exit Vehicle", "Have your pet exit the vehicle");
            var heal      = new NativeItem("Treat (+10 HP)", "Feed a quick treat");
            var dismiss   = new NativeItem("Dismiss Pet", "Send your pet home");

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

                if (item == follow)        DoFollow();
                else if (item == stay)     DoStay();
                else if (item == come)     DoComeHere();
                else if (item == sit)      DoSit();
                else if (item == lie)      DoLay();
                else if (item == enterVeh) DoEnterVehicle();
                else if (item == exitVeh)  DoExitVehicle();
                else if (item == heal)     DoHeal();
                else if (item == dismiss)  DoDismiss();
            };
        }

        public static void Toggle()
        {
            if (InteractionMenu == null) EnsureMenu();
            InteractionMenu.Visible = !InteractionMenu.Visible;
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

                // Prefer natural pathing—teleport only if extremely far or stuck too long
                if (Utils.ComeWarpIfFar && dist > Utils.ComeWarpDistance)
                {
                    Main.Pet.Position = me.Position + me.ForwardVector * -1.0f;
                    Main.Pet.Task.ClearAll();
                }
                else
                {
                    Function.Call(Hash.TASK_GO_TO_ENTITY, Main.Pet.Handle, me.Handle, Utils.ComeStopRange, Utils.ComeRunSpeed, 0f, 0, 0);
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

        private static bool IsPlayingScenarioOrAnim(Ped p)
        {
            try
            {
                if (p == null || !p.Exists()) return false;
                if (Function.Call<bool>(Hash.IS_PED_ACTIVE_IN_SCENARIO, p.Handle)) return true;
                if (Function.Call<bool>(Hash.IS_PED_USING_ANY_SCENARIO, p.Handle)) return true;
            }
            catch {}
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
                string model = Function.Call<string>(Hash.GET_ENTITY_MODEL, Main.Pet.Handle).ToString();
                // Dogs: use scenario SIT or sit anims; Cats: use grooming as a "sit-in-place"
                if (Function.Call<bool>(Hash.IS_THIS_MODEL_A_DOG, Main.Pet.Model.Hash))
                {
                    Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, Main.Pet.Handle, "WORLD_DOG_SITTING", 0, true);
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
                if (Function.Call<bool>(Hash.IS_THIS_MODEL_A_DOG, Main.Pet.Model.Hash))
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

                // Task enter vehicle
                Function.Call(Hash.TASK_ENTER_VEHICLE, Main.Pet.Handle, veh.Handle, -1, (int)seat, 1.2f, 1, 0);

                // Auto-pose: once in, play a sit/lay loop depending on config
                Script.Wait(800);
                if (IsInAnyVehicle(Main.Pet))
                {
                    _vehPoseActive = true;
                    _vehPoseVehHandle = veh.Handle;
                    _vehPoseSeat = seat;

                    if (Utils.VehiclePose == 0) // sit
                    {
                        PlayAnim(Main.Pet, "creatures@rottweiler@incar@std@ds@", "sit", 1, -1);
                    }
                    else // lay
                    {
                        PlayAnim(Main.Pet, "creatures@rottweiler@incar@std@ds@", "sleep", 1, -1);
                    }
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

                // Clear pose state and nudge to the side
                _vehPoseActive = false; _vehPoseVehHandle = 0;
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
                if (IsInAnyVehicle(pet)) DoExitVehicle();

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
