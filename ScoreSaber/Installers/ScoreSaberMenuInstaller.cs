using ScoreSaber.Networking;
using Zenject;

namespace ScoreSaber.Installers;

internal class ScoreSaberMenuInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<ScoreSaberAuthorizationManager>().AsSingle();
    }
}