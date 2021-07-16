namespace RUtil.Debug.Shell
{
    public interface IUnishProcess
    {
        IUnishProcess        Parent      { get; }
        UnishEnvSet          Env         { get; }
        UnishIOs             IO          { get; }
        IUnishInterpreter    Interpreter { get; }
        IUnishFileSystemRoot Directory   { get; }
        IUnishProcess Fork(UnishIOs io);
    }
}
