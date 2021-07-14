namespace RUtil.Debug.Shell
{
    public interface IUnishProcess
    {
        UnishEnvSet       Env         { get; }
        IUnishIO          IO          { get; }
        IUnishInterpreter Interpreter { get; }
        IUnishFileSystemRoot  Directory   { get; }
        IUnishProcess Fork(IUnishIO io);
    }
}
