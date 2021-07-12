﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RUtil.Debug.Shell
{
    public class DefaultIO : IUnishIO
    {
        public           IUnishEnv          GlobalEnv { protected get; set; }
        private const    string             SceneName = "UnishDefault";
        private readonly IUnishInputHandler mInputHandler;
        private readonly IUnishTimeProvider mTimeProvider;
        private readonly IUnishColorParser  mColorParser;
        private readonly Font               mFont;
        private          Scene              loadedScene;
        private          Image              background;
        private          Text               text;
        private          int                mCharCountPerLine;
        private          int                mLineCount;
        private          bool               mIsReading;

        // 入力履歴
        private readonly List<string> mSubmittedInputs = new List<string>();

        // 表示履歴
        private readonly List<string> mSubmittedLines = new List<string>();

        // 入力中文字列
        private string mInput = "";


        // 入力履歴参照先インデックス
        private int mReferenceIndex;

        // 入力履歴を参照中にもともとの履歴を保持しておくキャッシュ
        private string mReferenceCache = "";

        // 表示履歴のオフセット
        private int mDisplayLineOffset;

        // カーソル位置　終端にある状態が0, 始端にある状態がinput.Length
        private int mCursorIndex;

        // カーソルの点滅開始時刻
        private float mCursorBrinkStartTime;


        private Color BackgroundColor
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


        public DefaultIO() : this(
            default,
            new DefaultInputHandler(DefaultTimeProvider.Instance),
            DefaultTimeProvider.Instance,
            DefaultColorParser.Instance
        )
        {
        }

        public DefaultIO(Font font) : this(
            font,
            new DefaultInputHandler(DefaultTimeProvider.Instance),
            DefaultTimeProvider.Instance,
            DefaultColorParser.Instance
        )
        {
        }

        public DefaultIO(Font font, IUnishInputHandler inputHandler, IUnishTimeProvider timeProvider, IUnishColorParser colorParser)
        {
            mFont         = font;
            mInputHandler = inputHandler;
            mTimeProvider = timeProvider;
            mColorParser  = colorParser;
        }

        public async UniTask InitializeAsync()
        {
            if (loadedScene.IsValid())
            {
                throw new Exception("Unish scene has already been loaded.");
            }

            mSubmittedLines.Clear();
            mSubmittedInputs.Clear();

            await SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            loadedScene = SceneManager.GetSceneByName(SceneName);
            var component = loadedScene.GetRootGameObjects()[0].GetComponent<DefaultDisplay>();
            background = component.Background;
            text       = component.Text;
            if (mFont)
            {
                text.font = mFont;
            }

            var env = GlobalEnv;

            if (!env.TryGet(UnishBuiltInEnvKeys.BgColor, out Color bgColor))
            {
                bgColor = mColorParser.Parse("#000000cc");
                env.Set(UnishBuiltInEnvKeys.BgColor, bgColor);
            }

            if (!env.TryGet(UnishBuiltInEnvKeys.CharCountPerLine, out mCharCountPerLine))
            {
                mCharCountPerLine = 100;
                env.Set(UnishBuiltInEnvKeys.CharCountPerLine, mCharCountPerLine);
            }

            if (!env.TryGet(UnishBuiltInEnvKeys.LineCount, out mLineCount))
            {
                mLineCount = 24;
                env.Set(UnishBuiltInEnvKeys.LineCount, mLineCount);
            }

            // 背景色設定
            BackgroundColor = bgColor;

            // 画面サイズ設定
            RefleshSize();

            await mInputHandler.InitializeAsync();
            env.OnSet     += OnEnvSet;
            env.OnRemoved += OnEnvRemoved;

            StartUpdate().Forget();
        }

        public async UniTask FinalizeAsync()
        {
            if (!loadedScene.IsValid())
            {
                throw new Exception("Unish scene has not been loaded.");
            }

            GlobalEnv.OnRemoved -= OnEnvRemoved;
            GlobalEnv.OnSet     -= OnEnvSet;

            await mInputHandler.FinalizeAsync();
            await SceneManager.UnloadSceneAsync(loadedScene);
            text        = null;
            background  = null;
            loadedScene = default;
        }

        public event Action OnHaltInput;


        public UniTask WriteAsync(string input)
        {
            if (mSubmittedLines.Count == 0)
            {
                mSubmittedLines.Add("");
            }

            var j = 0;
            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '\n')
                {
                    mSubmittedLines[mSubmittedLines.Count - 1] += input.Substring(j, i - j);
                    mSubmittedLines.Add("");
                    j = i + 1;
                }
            }

            if (j < input.Length)
            {
                mSubmittedLines[mSubmittedLines.Count - 1] += input.Substring(j);
            }

            return default;
        }

        public async UniTask WriteErrorAsync(Exception error)
        {
            await this.WriteLineAsync($"error: {error.Message}", "#ff7777");
            UnityEngine.Debug.LogError(error);
        }


        public async UniTask<string> ReadAsync(bool withPrompt)
        {
            mIsReading = true;
            if (withPrompt)
            {
                await WriteAsync(ParsedPrompt);
            }

            string ret = null;
            while (ret == null)
            {
                ret = await HandleSubmissionInput();
                await UniTask.Yield();
            }

            mIsReading = false;
            return ret;
        }

        private void OnEnvSet(UnishVariable envvar)
        {
            switch (envvar.Name)
            {
                case UnishBuiltInEnvKeys.BgColor:
                    BackgroundColor = envvar.CastOr(mColorParser.Parse("#000000cc"));
                    break;
                case UnishBuiltInEnvKeys.CharCountPerLine:
                    mCharCountPerLine = Mathf.Max(20, envvar.CastOr(100));
                    RefleshSize();
                    break;
                case UnishBuiltInEnvKeys.LineCount:
                    mLineCount = Mathf.Max(1, envvar.CastOr(24));
                    RefleshSize();
                    break;
            }
        }

        private void OnEnvRemoved(string key)
        {
            switch (key)
            {
                case UnishBuiltInEnvKeys.BgColor:
                    BackgroundColor = mColorParser.Parse("#000000cc");
                    break;
                case UnishBuiltInEnvKeys.CharCountPerLine:
                    mCharCountPerLine = 100;
                    RefleshSize();
                    break;
                case UnishBuiltInEnvKeys.LineCount:
                    mLineCount = 24;
                    RefleshSize();
                    break;
            }
        }

        private void RefleshSize()
        {
            var prevText    = text.text;
            var placeHolder = new StringBuilder();
            for (var i = 0; i < mLineCount; i++)
            {
                placeHolder.AppendLine(new string('0', mCharCountPerLine));
            }


            text.text                          = placeHolder.ToString();
            text.rectTransform.sizeDelta       = new Vector2(text.preferredWidth, text.preferredHeight);
            background.rectTransform.sizeDelta = new Vector2(text.preferredWidth + 20, text.preferredHeight + 20);
            text.text                          = prevText;
        }

        private async UniTaskVoid StartUpdate()
        {
            await foreach (var _ in UniTaskAsyncEnumerable.EveryUpdate())
            {
                if (!OnUpdate())
                {
                    break;
                }
            }
        }

        private bool isInputEventUsed;

        private bool OnUpdate()
        {
            isInputEventUsed = HandleScrollInput();
            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Quit))
            {
                OnHaltInput?.Invoke();
                return false;
            }

            UpdateDisplay();
            return true;
        }

        private async UniTask<string> HandleSubmissionInput()
        {
            if (isInputEventUsed)
            {
                return null;
            }


            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Up))
            {
                if (mReferenceIndex < mSubmittedInputs.Count)
                {
                    if (mReferenceIndex == 0)
                    {
                        mReferenceCache = mInput;
                    }

                    mReferenceIndex++;
                    mInput = mSubmittedInputs[mSubmittedInputs.Count - mReferenceIndex];
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Down))
            {
                if (mReferenceIndex > 0)
                {
                    mReferenceIndex--;
                }

                mInput = mReferenceIndex > 0
                    ? mSubmittedInputs[mSubmittedInputs.Count - mReferenceIndex]
                    : mReferenceCache;
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Left))
            {
                if (mCursorIndex < mInput.Length)
                {
                    mCursorIndex++;
                    mCursorBrinkStartTime = mTimeProvider.Now;
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Right))
            {
                if (mCursorIndex > 0)
                {
                    mCursorIndex--;
                    mCursorBrinkStartTime = mTimeProvider.Now;
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.BackSpace))
            {
                if (mInput.Length > 0)
                {
                    if (mCursorIndex == 0)
                    {
                        mInput = mInput.Substring(0, mInput.Length - 1);
                    }
                    else if (mCursorIndex < mInput.Length)
                    {
                        mInput = mInput.Substring(0, mInput.Length - mCursorIndex - 1) +
                                 mInput.Substring(mInput.Length - mCursorIndex);
                    }
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Delete))
            {
                if (mInput.Length > 0 && mCursorIndex > 0)
                {
                    //3210   
                    //abc
                    if (mCursorIndex == 1)
                    {
                        mInput = mInput.Substring(0, mInput.Length - 1);
                    }
                    else if (mCursorIndex == mInput.Length)
                    {
                        mInput = mInput.Substring(1);
                    }
                    else
                    {
                        mInput = mInput.Substring(0, mInput.Length - mCursorIndex) +
                                 mInput.Substring(mInput.Length - mCursorIndex + 1);
                    }

                    mCursorIndex--;
                }
            }
            else if (mInputHandler.CheckInputOnThisFrame(UnishInputType.Submit))
            {
                if (!string.IsNullOrWhiteSpace(mInput))
                {
                    mSubmittedInputs.Add(mInput);
                }

                var ret = mInput;
                await this.WriteLineAsync(mInput);
                mInput             = "";
                mDisplayLineOffset = 0;
                mReferenceIndex    = 0;
                mCursorIndex       = 0;
                return ret;
            }

            if (mInputHandler.CurrentCharInput != default)
            {
                mInput = mInput.Insert(mInput.Length - mCursorIndex, mInputHandler.CurrentCharInput.ToString());
            }


            return null;
        }


        private bool HandleScrollInput()
        {
            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.ScrollUp))
            {
                if (mDisplayLineOffset < mSubmittedLines.Count - mLineCount)
                {
                    mDisplayLineOffset++;
                }

                return true;
            }

            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.ScrollDown))
            {
                if (mDisplayLineOffset > 0)
                {
                    mDisplayLineOffset--;
                }

                return true;
            }

            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.PageUp))
            {
                mDisplayLineOffset = Mathf.Min(mDisplayLineOffset + mLineCount,
                    mSubmittedLines.Count - mLineCount);
                return true;
            }

            if (mInputHandler.CheckInputOnThisFrame(UnishInputType.PageDown))
            {
                mDisplayLineOffset = Mathf.Max(mDisplayLineOffset - mLineCount, 0);
                return true;
            }

            return false;
        }


        private void UpdateDisplay()
        {
            static string TagEscape(string str)
            {
                return str.Replace("<", "<\b").Replace(">", "\b>");
            }

            var now = mTimeProvider.Now;
            var inputWithCursor = mCursorIndex == 0
                ? TagEscape(mInput) + (Mathf.RoundToInt((now - mCursorBrinkStartTime) * 60) % 60 > 30
                    ? "<color=yellow>_</color>"
                    : " ")
                : TagEscape(mInput.Substring(0, mInput.Length - mCursorIndex))
                  + (Mathf.RoundToInt((now - mCursorBrinkStartTime) * 60) % 60 > 30
                      ? "<color=yellow>_</color>"
                      : $"<color=orange>{TagEscape(mInput.Substring(mInput.Length - mCursorIndex, 1))}</color>")
                  + TagEscape(mInput.Substring(mInput.Length - mCursorIndex + 1));

            if (!text)
            {
                return;
            }

            if (mSubmittedLines.Count == 0)
            {
                text.text = mIsReading ? inputWithCursor : "";
                return;
            }

            if (mDisplayLineOffset == 0)
            {
                text.text = mSubmittedLines
                                .Skip(mSubmittedLines.Count - mLineCount)
                                .ToSingleString("\n") +
                            (mIsReading ? inputWithCursor : "");
                return;
            }

            text.text = mSubmittedLines
                            .Skip(mSubmittedLines.Count - mLineCount - mDisplayLineOffset)
                            .Take(Mathf.Min(mLineCount, mSubmittedLines.Count) - 1)
                            .ToSingleString("\n") + "\n" + mSubmittedLines.Last() +
                        (mIsReading ? inputWithCursor : "");
        }


        private string ParsedPrompt
        {
            get
            {
                var env    = GlobalEnv;
                var prompt = env[UnishBuiltInEnvKeys.Prompt].S;
                var pwd    = env[UnishBuiltInEnvKeys.WorkingDirectory].S;
                if (!prompt.Contains("%d"))
                {
                    return prompt;
                }

                if (pwd == UnishPathConstants.Root)
                {
                    return prompt.Replace("%d", UnishPathConstants.Root);
                }

                if (UnishPathUtils.IsHomePath(pwd))
                {
                    return prompt.Replace("%d", UnishPathConstants.Home);
                }

                return prompt.Replace("%d", UnishPathUtils.GetEntryName(pwd));
            }
        }
    }
}
