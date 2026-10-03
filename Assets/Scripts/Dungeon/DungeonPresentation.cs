namespace LunaEclipse.Dungeon
{
    // Design pixels, not world units. Width stays nine tiles on every device.
    public static class DungeonPresentation
    {
        public const float Width = 1170, Height = 2532;
        public const float HeaderHeight = 240, ControlsTop = 2130;
        public const float ViewHeight = ControlsTop - HeaderHeight;
        public const float TilesAcross = 9;
        public const float LunaHeight = 1.02f; // Original .85-cell body enlarged 20%; foot anchor is unchanged.
        // Opaque idle/down body measured from original 320x240 PNG, not its padding.
        public const float LunaBodyCanvasRatio = 125f / 240f;
        public const float LunaFootOffset = -.40f;
    }
}
