using System;

namespace ProjectAT.Option
{
    [Serializable]
    public struct AudioOptionSetting
    {
        public float Bgm;
        public float Sfx;
        public float Ui;

        public AudioOptionSetting(float bgm, float sfx, float ui)
        {
            Bgm = bgm;
            Sfx = sfx;
            Ui = ui;
        }

        public static AudioOptionSetting Default => new AudioOptionSetting(50f, 50f, 50f);
    }
    
}
