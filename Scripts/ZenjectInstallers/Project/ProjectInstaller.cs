using UnityEngine;
using Zenject;

internal sealed class ProjectInstaller : MonoInstaller<ProjectInstaller>
{
    private ProjectInstaller() { }

    public override void InstallBindings()
    {
        void Signals()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<ReplaceItem>();
            Container.DeclareSignal<GoToGame>();
            Container.DeclareSignal<GoToGameOver>();
            Container.DeclareSignal<GoToMenu>();
            Container.DeclareSignal<GobalScoreChangedInData>();
        }

        void Uncategorized()
        {
            Container.BindInterfacesAndSelfTo<InputHandler>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameData>().AsSingle();
        }

        Uncategorized();
        Signals();
    }
}
