using GTA.Native;

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

        // Overload to match older calls (name + percent + show flag)
        public static void Update(string petName, float pct, bool show)
        {
            if (show) Show(petName);
            Update(pct);
        }

        // Overload to accept raw health and max (common in some forks)
        public static void Update(string petName, int health, int maxHealth)
        {
            float pct = (maxHealth > 0) ? (float)health / (float)maxHealth : 0f;
            Show(petName);
            Update(pct);
        }

        public static void Draw()
        {
            if (!_visible) return;

            float cx = Utils.HudX;
            float cy = Utils.HudY;
            float w = 0.22f;
            float h = 0.035f;

            Function.Call(Hash.DRAW_RECT, cx, cy, w, h, 0, 0, 0, Utils.HudBackAlpha);

            float left = cx - w * 0.5f;
            float fillW = w * _pct;
            float fillX = left + fillW * 0.5f;
            Function.Call(Hash.DRAW_RECT, fillX, cy, fillW, h * 0.65f, 255, 255, 255, Utils.HudFillAlpha);

            // Center the label horizontally over the bar
            Utils.DrawText(_label, cx, cy - h * 0.55f, Utils.HudScale, Utils.HudTextR, Utils.HudTextG, Utils.HudTextB, Utils.HudTextA, true);
        }
    }
}
