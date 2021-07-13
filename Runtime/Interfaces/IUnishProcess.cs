namespace RUtil.Debug.Shell
{
    public interface IUnishProcess
    {
        UnishEnvSet         Env         { get; }
        IUnishIO            IO          { get; }
        IUnishInterpreter   Interpreter { get; }
        IUnishDirectoryRoot Directory   { get; }
        IUnishProcess Fork(IUnishIO io);
    }
}
