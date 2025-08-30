using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace AnimalArkShelter
{
    public static class Utils
    {
        // --- INI-backed config (loaded once) ---
        static Utils()
        {
            try
            {
                var ini = ScriptSettings.Load(@"scripts\\AnimalArkShelter.ini");

                // Shop/camera
                float camX = ini.GetValue("Shop", "CameraOffsetX", 2.2f);
                float camY = ini.GetValue("Shop", "CameraOffsetY", 2.6f);
                float camZ = ini.GetValue("Shop", "CameraOffsetZ", 1.2f);
                CameraOffset = new Vector3(camX, camY, camZ);

                float showX = ini.GetValue("Shop", "ShowcaseOffsetX", 1.6f);
                float showY = ini.GetValue("Shop", "ShowcaseOffsetY", 1.2f);
                float showZ = ini.GetValue("Shop", "ShowcaseOffsetZ", 0.0f);
                ShowcaseOffset = new Vector3(showX, showY, showZ);

                ShopFov = ini.GetValue("Shop", "FOV", 50.0f);
                ShopEaseTimeMs = ini.GetValue("Shop", "EaseTimeMs", 350);
                EnableWalkOffAnim = ini.GetValue("Shop", "EnableWalkOffAnim", true);
                WalkOffDistance = ini.GetValue("Shop", "WalkOffDistance", 1.5f);

                // Come Here
                ComeWarpIfFar = ini.GetValue("ComeHere", "WarpIfFar", true);
                ComeWarpDistance = ini.GetValue("ComeHere", "WarpDistance", 120.0f);
                ComeRunSpeed = ini.GetValue("ComeHere", "RunSpeed", 3.0f);
                ComeStopRange = ini.GetValue("ComeHere", "StopRange", 1.2f);
                ComeStuckTeleportMs = ini.GetValue("ComeHere", "TeleportIfStuckMs", 4500);

                // Vehicle
                VehicleSeatIndex = ini.GetValue("Vehicle", "SeatIndex", 0); // 0=Passenger
                VehiclePose = ini.GetValue("Vehicle", "Pose", 0); // 0=sit,1=lay

                // HUD
                HudScale = ini.GetValue("HUD", "Scale", 0.35f);
                HudX = ini.GetValue("HUD", "X", 0.88f);
                HudY = ini.GetValue("HUD", "Y", 0.88f);
                HudUseOutline = ini.GetValue("HUD", "UseOutline", true);
                HudUseShadow = ini.GetValue("HUD", "UseShadow", false);
                HudTextR = ini.GetValue("HUD", "TextR", 255);
                HudTextG = ini.GetValue("HUD", "TextG", 255);
                HudTextB = ini.GetValue("HUD", "TextB", 255);
                HudTextA = ini.GetValue("HUD", "TextA", 255);
                HudFillAlpha = ini.GetValue("HUD", "FillAlpha", 220);
                HudBackAlpha = ini.GetValue("HUD", "BackAlpha", 160);
            }
            catch { }
        }

        public static Vector3 CameraOffset { get; private set; } = new Vector3(2.2f, 2.6f, 1.2f);
        public static Vector3 ShowcaseOffset { get; private set; } = new Vector3(1.6f, 1.2f, 0.0f);
        public static float ShopFov { get; private set; } = 50.0f;
        public static int ShopEaseTimeMs { get; private set; } = 350;
        public static bool EnableWalkOffAnim { get; private set; } = true;
        public static float WalkOffDistance { get; private set; } = 1.5f;

        public static bool ComeWarpIfFar { get; private set; } = true;
        public static float ComeWarpDistance { get; private set; } = 120.0f;
        public static float ComeRunSpeed { get; private set; } = 3.0f;
        public static float ComeStopRange { get; private set; } = 1.2f;
        public static int   ComeStuckTeleportMs { get; private set; } = 4500;

        public static int VehicleSeatIndex { get; private set; } = 0;
        // 0 = sit, 1 = lay
        public static int VehiclePose { get; private set; } = 0;

        public static float HudScale { get; private set; } = 0.35f;
        public static float HudX { get; private set; } = 0.88f;
        public static float HudY { get; private set; } = 0.88f;
        public static bool HudUseOutline { get; private set; } = true;
        public static bool HudUseShadow { get; private set; } = false;
        public static int HudTextR { get; private set; } = 255;
        public static int HudTextG { get; private set; } = 255;
        public static int HudTextB { get; private set; } = 255;
        public static int HudTextA { get; private set; } = 255;
        public static int HudFillAlpha { get; private set; } = 220;
        public static int HudBackAlpha { get; private set; } = 160;

        // --- UI helpers ---
        public static void Notify(string msg)
        {
            try { GTA.UI.Notification.PostTicker(msg, false, false); } catch {}
        }

        public static void ShowTicker(string msg)
        {
            try { GTA.UI.Notification.PostTicker(msg, false, true); } catch {}
        }

        public static void DisplayHelpTextThisFrame(string text)
        {
            try
            {
                Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, "STRING");
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
                Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, false, true, -1);
            }
            catch { }
        }

        public static void DrawText(string text, float x, float y, float scale, int r, int g, int b, int a)
        {
            try
            {
                Function.Call(Hash.SET_TEXT_FONT, 0);
                Function.Call(Hash.SET_TEXT_SCALE, scale, scale);
                Function.Call(Hash.SET_TEXT_COLOUR, r, g, b, a);
                if (HudUseOutline) Function.Call(Hash.SET_TEXT_OUTLINE);
                if (HudUseShadow)  Function.Call(Hash.SET_TEXT_DROP_SHADOW);

                Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text);
                Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, x, y);
            }
            catch { }
        }

        // --- Grounding & placement helpers ---
        public static bool TryGetGroundZ(Vector3 pos, out float groundZ)
        {
            groundZ = 0f;
            try
            {
                unsafe
                {
                    float gz;
                    bool ok = Function.Call<bool>(Hash.GET_GROUND_Z_FOR_3D_COORD, pos.X, pos.Y, pos.Z + 10f, &gz, false);
                    if (ok) { groundZ = gz; return true; }
                }
            }
            catch { }
            return false;
        }

        public static bool TryFindSafeCoordNear(Vector3 origin, float radius, out Vector3 result)
        {
            result = origin;
            try
            {
                // First, use safe coord native
                unsafe
                {
                    float outX=0, outY=0, outZ=0;
                    bool ok = Function.Call<bool>(Hash.GET_SAFE_COORD_FOR_PED, origin.X, origin.Y, origin.Z, true, &outX, &outY, &outZ, 16);
                    if (ok) { result = new Vector3(outX, outY, outZ); return true; }
                }
            }
            catch { }

            try
            {
                // Fallback: try around in a small circle
                for (int i = 0; i < 12; i++)
                {
                    float ang = i * (float)(Math.PI / 6.0);
                    var p = origin + new Vector3((float)Math.Cos(ang), (float)Math.Sin(ang), 0f) * radius;
                    if (TryGetGroundZ(p, out var gz)) { result = new Vector3(p.X, p.Y, gz + 0.05f); return true; }
                }
            }
            catch { }
            return false;
        }

        // Simple on-screen keyboard
        public static string GetUserTextInput(string title, string defaultText = "", int maxLen = 20)
        {
            try
            {
                Function.Call(Hash.DISPLAY_ONSCREEN_KEYBOARD, 1, "FMMC_KEY_TIP8", "", defaultText ?? "", "", "", "", maxLen);
                int upd = 0;
                while ((upd = Function.Call<int>(Hash.UPDATE_ONSCREEN_KEYBOARD)) == 0) { Script.Wait(0); }
                if (upd == 1)
                {
                    string res = Function.Call<string>(Hash.GET_ONSCREEN_KEYBOARD_RESULT);
                    if (!string.IsNullOrEmpty(res)) return res;
                }
            }
            catch { }
            return defaultText;
        }
    }
}
