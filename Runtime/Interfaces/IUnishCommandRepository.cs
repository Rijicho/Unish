using System.Collections.Generic;

namespace RUtil.Debug.Shell
{
    public interface IUnishCommandRepository : IUnishResource
    {
        IReadOnlyList<UnishCommandBase>               Commands { get; }
        IReadOnlyDictionary<string, UnishCommandBase> Map      { get; }
    }
}
