namespace RUtil.Debug.Shell
{
    public interface IUnishResourceWithEnv : IUnishResource
    {
        IUnishEnv BuiltInEnv { set; }
    }
}
