namespace RUtil.Debug.Shell
{
    public enum UnishCommandTokenType
    {
        Invalid,
        Param,
        Option,
        RedirectIn,
        RedirectOut,
        RedirectOutAppend,
        RedirectErr,
        RedirectErrAppend,
        Pipe,
        Separate,
    }
}
