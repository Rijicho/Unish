using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RUtil.Debug.Shell
{
    public class DefaultUnishView : IUnishView
    {
        private const string SceneName = "UnishDefault";

        protected DefaultUnishView()
        {
        }

        private static DefaultUnishView mInstance;

        public static DefaultUnishView Instance => mInstance ??= new DefaultUnishView();

        private Scene loadedScene;
        private Image background;
        private Text  text;

        protected virtual UniTask<Font> GetOrLoadFont()
        {
            return UniTask.FromResult<Font>(default);
        }

        public string DisplayText
        {
            get => text ? text.text : "";
            set
            {
                if (text)
                {
                    text.text = value;
                }
            }
        }

        public Color DisplayTextColor
        {
            get => text ? text.color : Color.white;
            set
            {
                if (text)
                {
                    text.color = value;
                }
            }
        }

        public Color BackgroundColor
        {
            get => background ? background.color : Color.clear;
            set
            {
                if (background)
                {
                    background.color = value;
                }
            }
        }

        public int MaxLineCount        { get; private set; }
        public int HorizontalCharCount { get; private set; }

        protected virtual Color BackgroundDefaultColor => new Color(0, 0, 0, 0.7f);

        public async UniTask InitializeAsync()
        {
            if (loadedScene.IsValid())
            {
                throw new Exception("Unish scene has already been loaded.");
            }

            await SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            loadedScene = SceneManager.GetSceneByName(SceneName);
            var component = loadedScene.GetRootGameObjects()[0].GetComponent<DefaultUnishViewRoot>();
            background          = component.Background;
            text                = component.Text;
            HorizontalCharCount = component.CharCountPerLine;
            MaxLineCount        = component.MaxLineCount;

            BackgroundColor = BackgroundDefaultColor;

            var placeHolder = new StringBuilder();
            for (var i = 0; i < MaxLineCount; i++)
            {
                placeHolder.AppendLine(new string(' ', HorizontalCharCount));
            }

            var font = await GetOrLoadFont();
            if (font)
            {
                text.font = font;
            }

            text.text                          = placeHolder.ToString();
            text.rectTransform.sizeDelta       = new Vector2(text.preferredWidth, text.preferredHeight);
            background.rectTransform.sizeDelta = new Vector2(text.preferredWidth + 20, text.preferredHeight + 20);
            text.text                          = "";
        }

        public async UniTask DestroyAsync()
        {
            if (!loadedScene.IsValid())
            {
                throw new Exception("Unish scene has not been loaded.");
            }

            await SceneManager.UnloadSceneAsync(loadedScene);
            text        = null;
            background  = null;
            loadedScene = default;
        }
    }
}
