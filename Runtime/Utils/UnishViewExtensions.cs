namespace RUtil.Debug.Shell
{
    public static class UnishViewExtensions
    {
        public static void WriteLine(this IUnishView view, string line)
        {
            view.Write(line+"\n");
        }
    }
}
