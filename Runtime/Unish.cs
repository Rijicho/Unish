﻿using Cysharp.Threading.Tasks;

namespace RUtil.Debug.Shell
{
    public static class Unish
    {
        private static IUnish mShell;

        public static bool IsRunning { get; private set; }

        public static async UniTask StartAsync<T>()
            where T : IUnish, new()
        {
            if (mShell != null)
            {
                UnityEngine.Debug.LogError("Unish already exists.");
                return;
            }

            IsRunning = true;

            mShell = new T();
            await mShell.OpenAsync();
        }

        public static async UniTask StopAsync()
        {
            if (mShell == null)
            {
                UnityEngine.Debug.LogError("Unish instance does not exist.");
                return;
            }

            await mShell.CloseAsync();
            mShell = null;
            IsRunning = false;
        }
    }
}