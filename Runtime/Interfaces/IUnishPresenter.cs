using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishPresenter : IUnishProcess
    {
        IUnishEnv           Env         { get; }
        IUnishIO            IO          { get; }
        IUnishInterpreter   Interpreter { get; }
        IUnishDirectoryRoot Directory   { get; }

        UniTask RunAsync();
        void Halt();
    }

    public interface IUnishRoot : IUnishProcess
    {
        IUnishEnv GlobalEnv { get; }
        UniTask RunAsync();
        void Halt();
    }

    public interface IUnishProcess
    {
        IUnishProcess Parent { get; }
    }
}
