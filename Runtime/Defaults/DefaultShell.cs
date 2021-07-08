using System.Collections.Generic;
using UnityEngine;

namespace RUtil.Debug.Shell
{
    public class DefaultShell : UnishCore
    {
        public override IUnishView View { get; } = DefaultUnishView.Instance;
        public override IUnishCommandRepository CommandRepository { get; } = DefaultUnishCommandRepository.Instance;
        public override IColorParser ColorParser { get; } = DefaultColorParser.Instance;

        public override IUnishInputHandler InputHandler { get; } =
            new DefaultUnishInputHandler(DefaultTimeProvider.Instance);

        public override ITimeProvider TimeProvider { get; } = DefaultTimeProvider.Instance;

        public override IUnishRcRepository RcRepository { get; } = DefaultUnishRcRepository.Instance;

        private static readonly IUnishDirectorySystem[] mDirectorySystems =
        {
            new RealFileSystem("pdp", Application.persistentDataPath),
            new RealFileSystem("dp", Application.dataPath),
        };
        public override IEnumerable<IUnishDirectorySystem> DirectorySystems => mDirectorySystems;
    }
}