using MapleStory_HealthKeeper.Helper;
using MapleStory_HealthKeeper.Model;

namespace MapleStory_HealthKeeper.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class HealProfile
    {
        public HealProfile()
        {
        }

        public static HealProfile CreateDefault(HealType healType)
        {
            switch (healType)
            {
                case HealType.HP:
                    return new() { Key = new() { Key = Keys.Delete, ScanCode = 83, IsExtented = true } };

                case HealType.MP:
                    return new() { Key = new() { Key = Keys.PageDown, ScanCode = 81, IsExtented = true } };

                default:
                    throw new NotImplementedException();
            }
        }

        public void OnKeepOverThenChanged(int oldValue, int newValue)
        {
            if (HealMode == HealMode.Percentage)
            {
                if (newValue > 100)
                    KeepOverThen = 100;
            }
        }

        [OnChangedMethod(nameof(OnKeepOverThenChanged))]
        public int KeepOverThen { get; set; } = 50;

        public HealMode HealMode { get; set; } = HealMode.Percentage;
        public FullKeyInfo Key { get; set; } = new() { Key = Keys.Delete, ScanCode = 83, IsExtented = true };
    }
}