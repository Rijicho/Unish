using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace RUtil.Debug.Shell
{
    public static class Unish
    {
        private static IUnishPresenter mShell;

        public static bool IsRunning { get; private set; }
        
        public static void Start<T>()
            where T : IUnishPresenter, new()
        {
            StartAsync<T>().Forget();
        }

        public static void Stop()
        {
            mShell?.Halt();
        }
            
        
        public static async UniTask StartAsync<T>()
            where T : IUnishPresenter, new()
        {
            if (IsRunning)
            {
                UnityEngine.Debug.LogError("Multi-run is not supported.");
                return;
            }

            IsRunning = true;
            mShell    = new T();
            await mShell.RunAsync();
            mShell    = null;
            IsRunning = false;
        }

        public static async UniTask StopAsync()
        {
            mShell?.Halt();
            while (IsRunning)
                await UniTask.Yield();
        }
    }
}
