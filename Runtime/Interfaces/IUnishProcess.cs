namespace RUtil.Debug.Shell
{
    public interface IUnishProcess
    {
        IUnishProcess        Parent      { get; set; }
        UnishEnvSet          Env         { get; set; }
        UnishIOs             IO          { get; set; }
        IUnishInterpreter    Interpreter { get; set; }
        IUnishFileSystemRoot Directory   { get; set; }
        IUnishProcess Fork(UnishIOs io);
    }
}
