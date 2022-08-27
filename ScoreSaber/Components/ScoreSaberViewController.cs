using BeatSaberMarkupLanguage.ViewControllers;

namespace ScoreSaber.Components;

internal class ScoreSaberViewController : BSMLAutomaticViewController
{
    protected new virtual void NotifyPropertyChanged(string name) => base.NotifyPropertyChanged(name);
}