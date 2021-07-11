namespace RUtil.Debug.Shell
{
    public interface IUnishParser : IUnishResource
    {
        UnishParseResult Parse(string input, IUnishEnv env);
    }
}
