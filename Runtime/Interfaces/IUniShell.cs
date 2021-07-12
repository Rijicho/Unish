using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUniShell : IUnishProcess
    {
        IUnishEnv           Env         { get; }
        IUnishIO            IO          { get; }
        IUnishInterpreter   Interpreter { get; }
        IUnishDirectoryRoot Directory   { get; }

        UniTask RunAsync();
    }
}
