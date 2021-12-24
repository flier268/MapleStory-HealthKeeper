using MapleStory_HealthKeeper.Model;

namespace MapleStory_HealthKeeper
{
    [AddINotifyPropertyChangedInterface]
    public class MainWindowViewModel
    {
        public const string MapleStoryNotFound = "MapleStory not found!";

        [JsonIgnore]
        public string Status { get; set; } = MapleStoryNotFound;

        [JsonIgnore]
        public string Slogan { get; set; } = "多喝水沒事，沒事多喝水";

        public string MapleStoryProcessName { get; set; } = "MapleStory";

        public int KeepHpOverThen { get; set; } = 50;
        public int KeepMpOverThen { get; set; } = 50;
        public FullKeyInfo HpKey { get; set; } = new() { Key = Keys.Delete, ScanCode = 83, IsExtented = true };
        public FullKeyInfo MpKey { get; set; } = new() { Key = Keys.PageDown, ScanCode = 81, IsExtented = true };
        public int Delay { get; set; } = 150;
    }
}