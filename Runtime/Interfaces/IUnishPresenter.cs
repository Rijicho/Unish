using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public interface IUnishPresenter
    {
        IUnishIO            IO          { get; }
        IUnishInterpreter   Interpreter { get; }
        IUnishDirectoryRoot Directory   { get; }
        string              Prompt      { set; }

        UniTask RunAsync();
        void Halt();
    }
}
