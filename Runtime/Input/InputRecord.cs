using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace Hinode
{
    /// <summary>
    /// 入力データを保存・管理するためのクラス
    /// 
    /// 保存時の入力データは文字列として保存されます。
    /// 入力データの記録のシーケンスとして以下のものを想定しています。
    /// 1. 1フレームの入力データをInputRecord#Frameに保存
    /// 1. InputRecord#Push()で登録。
    ///
    /// 入力データを保存する時は以下のケースを想定しています。
    /// - 文字列へ変換するものを用意し、InputRecord#Frame#InputTextを直接使用する。
    /// - InputRecord#Frame#SetInputTextByJSON()とInputRecord#Frame#ConvertFromJsonInputText()を使用する。
    /// <seealso cref="InputRecorder"/>
    /// <seealso cref="Hinode.Tests.Input.TestInputRecord"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Hinode/Input Record")]
    public class InputRecord : ScriptableObject
    {
		[SerializeField] Vector2Int _screenSize;
		[SerializeField] List<Frame> _frames = new List<Frame>();

        public Vector2Int ScreenSize { get => _screenSize; set => _screenSize = value; }
        public IEnumerable<Frame> Frames { get => _frames != null ? _frames : _frames; }
        public int FrameCount { get => _frames.Count; }

        public Frame this[int index]
        {
            get => _frames[index];
        }

        public static InputRecord Create()
            => Create(new Vector2Int(Screen.width, Screen.height));

        public static InputRecord Create(Vector2Int screenSize)
        {
            var inst = CreateInstance<InputRecord>();
            inst._screenSize = screenSize;
            return inst;
        }

        public void ClearFrames()
        {
            _frames.Clear();
        }

        public void Push(Frame frame)
        {
            _frames.Add(frame);
        }

        public void Push(IEnumerable<Frame> frames)
        {
            _frames.AddRange(frames);
        }

        public void Push(params Frame[] frames)
        {
            Push(frames);
        }

        /// <summary>
        /// 1フレームあたりの入力データを表す。
        /// </summary>
        [System.Serializable]
        public class Frame
		{
            [SerializeField] uint _frameNo = 0;
			[SerializeField] float _deltaSecond;
			[SerializeField] string _inputText;

            public Frame() : this(0, 0f) { }
            public Frame(uint frameNo, float deltaSecond)
            {
                _frameNo = frameNo;
                _deltaSecond = deltaSecond;
            }

            public uint FrameNo { get => _frameNo; set => _frameNo = value; }
            /// <summary>
            /// 計算誤差であまり信頼できない値になるかもしれないので注意してください。
            /// </summary>
            public float DeltaSecond { get => _deltaSecond; set => _deltaSecond = value; }
            public bool IsEmptyInputText { get => InputText == null || InputText == "" || InputText == "{}"; }
            public string InputText { get => _inputText; set => _inputText = value; }

            public T ConvertFromJsonInputText<T>()
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(InputText);
            }

            public void SetInputTextByJson(object t)
            {
                var serializer = new JsonSerializer();
                _inputText = serializer.Serialize(t);
            }
        }

        #region FrameEnumerable

        public IEnumerable<Frame> GetFrameEnumerable()
        {
            return new FrameEnumerable(this);
        }

        class FrameEnumerable : IEnumerable<Frame>, IEnumerable
        {
            InputRecord _target;
            public FrameEnumerable(InputRecord target)
            {
                _target = target;
            }

            public IEnumerator<Frame> GetEnumerator()
            {
                return new Enumerator(_target);
            }

            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

            class Enumerator : IEnumerator<Frame>, IEnumerator, System.IDisposable
            {
                InputRecord _target;
                IEnumerator<Frame> _enumerator;
                public Enumerator(InputRecord target)
                {
                    _target = target;
                    Reset();
                }
                public Frame Current => _enumerator.Current;
                object IEnumerator.Current => Current;
                public void Dispose() { }
                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator = _target.Frames.GetEnumerator();
            }
        }
        #endregion
	}
}
