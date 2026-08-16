namespace ProjectAT.Option
{
    public struct OptionSetting
    {
        public GraphicOptionSetting Graphic;
        public AudioOptionSetting Audio;
        public ControlKeySetting Control;

        public OptionSetting(GraphicOptionSetting graphic, AudioOptionSetting audio, ControlKeySetting control)
        {
            Graphic = graphic;
            Audio = audio;
            Control = control;
        }
        public static OptionSetting Default => new OptionSetting(GraphicOptionSetting.Default, AudioOptionSetting.Default, new ControlKeySetting());


    }
}