using BeatSaberMarkupLanguage.ViewControllers;

namespace ScoreSaber.UI
{
    internal class ScoreSaberViewController : BSMLAutomaticViewController
    {
        protected new virtual void NotifyPropertyChanged(string name) => base.NotifyPropertyChanged(name);
    }
}
