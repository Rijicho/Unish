using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishPresenter
    {
        IUnishEnv           Env         { get; }
        IUnishStandardIO    IO          { get; }
        IUnishInterpreter   Interpreter { get; }
        IUnishDirectoryRoot Directory   { get; }

        UniTask RunAsync();
        void Halt();
    }
}
