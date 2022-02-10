using CommunityToolkit.Mvvm.Input;
using MapleStory_HealthKeeper.ViewModels;
using Meziantou.Framework.WPF.Collections;

namespace MapleStory_HealthKeeper
{
    [AddINotifyPropertyChangedInterface]
    public class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            HpHealProfiles.Add(new());
            MpHealProfiles.Add(HealProfile.CreateDefault(Helper.HealType.MP));
            AddHpProfileCommand = new(() =>
            {
                if (SelectedHpHealProfile is null)
                    HpHealProfiles.Add(HealProfile.CreateDefault(Helper.HealType.HP));
                else
                    HpHealProfiles.Insert(HpHealProfiles.IndexOf(SelectedHpHealProfile), HealProfile.CreateDefault(Helper.HealType.HP));
            });
            RemoveHpProfileCommand = new(() =>
            {
                if (SelectedHpHealProfile is not null)
                {
                    int index = HpHealProfiles.IndexOf(SelectedHpHealProfile);
                    HpHealProfiles.Remove(SelectedHpHealProfile);
                    if (HpHealProfiles.Count > index)
                        SelectedHpHealProfile = HpHealProfiles[index];
                }
            });
            AddMpProfileCommand = new(() =>
            {
                if (SelectedMpHealProfile is null)
                    MpHealProfiles.Add(HealProfile.CreateDefault(Helper.HealType.MP));
                else
                    MpHealProfiles.Insert(MpHealProfiles.IndexOf(SelectedMpHealProfile), HealProfile.CreateDefault(Helper.HealType.MP));
            });
            RemoveMpProfileCommand = new(() =>
            {
                if (SelectedMpHealProfile is not null)
                {
                    int index = MpHealProfiles.IndexOf(SelectedMpHealProfile);
                    MpHealProfiles.Remove(SelectedMpHealProfile);
                    if (MpHealProfiles.Count > index)
                        SelectedMpHealProfile = MpHealProfiles[index];
                }
            });
        }

        public const string MapleStoryNotFound = "MapleStory not found!";

        [JsonIgnore]
        public string Status { get; set; } = MapleStoryNotFound;

        [JsonIgnore]
        public string Slogan { get; set; } = "多喝水沒事，沒事多喝水";

        public string MapleStoryProcessName { get; set; } = "MapleStory";

        [JsonIgnore]
        public HealProfile? SelectedHpHealProfile { get; set; }

        public ConcurrentObservableCollection<HealProfile> HpHealProfiles { get; set; } = new();

        [JsonIgnore]
        public HealProfile? SelectedMpHealProfile { get; set; }

        public ConcurrentObservableCollection<HealProfile> MpHealProfiles { get; set; } = new();
        public RelayCommand AddHpProfileCommand { get; }
        public RelayCommand RemoveHpProfileCommand { get; }
        public RelayCommand AddMpProfileCommand { get; }
        public RelayCommand RemoveMpProfileCommand { get; }
        public int Delay { get; set; } = 50;
    }
}