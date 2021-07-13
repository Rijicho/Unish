using System.Collections.Generic;

namespace RUtil.Debug.Shell
{
    public interface IUnishCommandRepository : IUnishResource
    {
        IReadOnlyDictionary<string, UnishCommandBase> Map { get; }
    }
}
