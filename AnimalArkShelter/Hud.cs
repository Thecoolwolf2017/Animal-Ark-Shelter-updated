using GTA.Native;
using GTA.Math;

namespace AnimalArkShelter
{
    public static class Hud
    {
        private static bool _visible = false;
        private static string _label = "Pet Health";
        private static float _pct = 1.0f;

        public static void Show(string petName)
        {
            _label = (petName ?? "Pet") + " Health";
            _visible = true;
        }

        public static void Hide() { _visible = false; }

        public static void Update(float pct)
        {
            if (pct < 0f) pct = 0f;
            if (pct > 1f) pct = 1f;
            _pct = pct;
        }

        public static void Draw()
        {
            if (!_visible) return;

            float cx = Utils.HudX;
            float cy = Utils.HudY;
            float w = 0.22f;
            float h = 0.035f;

            // Background
            Function.Call(Hash.DRAW_RECT, cx, cy, w, h, 0, 0, 0, Utils.HudBackAlpha);

            // Fill
            float left = cx - w * 0.5f;
            float fillW = w * _pct;
            float fillX = left + fillW * 0.5f;
            Function.Call(Hash.DRAW_RECT, fillX, cy, fillW, h * 0.65f, 255, 255, 255, Utils.HudFillAlpha);

            // Text (outlined by Utils settings)
            Utils.DrawText(_label, cx - w * 0.48f, cy - h * 0.55f, Utils.HudScale, Utils.HudTextR, Utils.HudTextG, Utils.HudTextB, Utils.HudTextA);
        }
    }
}
