using ScoreSaber.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace ScoreSaber.Core {
    internal class StandardInstaller : Installer {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<GamePresencePatches>().AsSingle();
        }
    }
}
