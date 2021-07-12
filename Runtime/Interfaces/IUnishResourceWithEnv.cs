namespace RUtil.Debug.Shell
{
    public interface IUnishResourceWithEnv : IUnishResource
    {
        IUnishEnv GlobalEnv { set; }
    }
}
