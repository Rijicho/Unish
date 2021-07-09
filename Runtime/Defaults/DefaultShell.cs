using System.Collections.Generic;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public sealed class DefaultShell : UnishCore
    {
        public override IUnishView                         View              { get; }
        public override IUnishCommandRepository            CommandRepository { get; }
        public override IUnishColorParser                  ColorParser       { get; }
        public override IUnishTimeProvider                 TimeProvider      { get; }
        public override IUnishRcRepository                 RcRepository      { get; }
        public override IEnumerable<IUnishDirectorySystem> DirectorySystems  { get; }

        public DefaultShell()
        {
            CommandRepository = DefaultUnishCommandRepository.Instance;
            ColorParser       = DefaultColorParser.Instance;
            TimeProvider      = DefaultTimeProvider.Instance;
            RcRepository      = DefaultUnishRcRepository.Instance;
            var inputHandler      = new DefaultUnishInputHandler(DefaultTimeProvider.Instance);
            View              = new DefaultUnishView(inputHandler, TimeProvider);
            DirectorySystems = new[]
            {
                new RealFileSystem("pdp", Application.persistentDataPath),
                new RealFileSystem("dp", Application.dataPath),
            };
        }
    }
}
