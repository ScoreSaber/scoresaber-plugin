using ScoreSaber.Networking;
using ScoreSaber.Services;
using ScoreSaber.Services.Login;
using Zenject;

namespace ScoreSaber.Installers;

internal class ScoreSaberCoreInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<Http>().AsSingle();
        Container.Bind<IPlatformLoginService>().To<SteamLoginService>().AsSingle();
    }
}