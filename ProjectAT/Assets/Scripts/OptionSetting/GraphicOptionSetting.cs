using System;

namespace ProjectAT.Option
{
    public enum AntiAliasingOption
    {
        Off = 0,
        FXAA = 1,
        TAA = 2
    }
    [Serializable]
    public struct GraphicOptionSetting
    {
        public int Width;
        public int Height;
        public bool VSync;

        public int FrameLimit; //-1 = unlimited

        public AntiAliasingOption AntiAliasing;

        public GraphicOptionSetting(int width, int height, bool vSync, int frameLimit, AntiAliasingOption aliasing)
        {
            Width = width;
            Height = height;
            VSync = vSync;
            FrameLimit = frameLimit;
            AntiAliasing = aliasing;
        }

        public static GraphicOptionSetting Default => new GraphicOptionSetting(1920, 1080, false, -1, AntiAliasingOption.FXAA);
    }
    
}

