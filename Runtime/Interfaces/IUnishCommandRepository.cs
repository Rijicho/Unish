using System.Collections.Generic;

namespace RUtil.Debug.Shell
{
    public interface IUnishCommandRepository
    {
        IReadOnlyList<UnishCommandBase>               Commands { get; }
        IReadOnlyDictionary<string, UnishCommandBase> Map      { get; }

        IDictionary<string, string> Aliases { get; }

        void Initialize();
    }
}
