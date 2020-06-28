using System.Collections.Generic;
using Hinode.Serialization;
using UnityEngine.Assertions;

namespace Hinode
{
    /// <summary>
	/// <see cref="InputRecord"/>
	/// </summary>
    public interface IFrameDataRecorder
    {
        /// <summary>
        /// 初期状態に戻す
        ///
        /// 基本的にGetValuesEnumerable()で取得できる値を処理対象にしてください。
        /// </summary>
        void ResetDatas();

        /// <summary>
        /// 基本的にGetValuesEnumerable()で取得できる値を処理対象にしてください。
        /// </summary>
        void RefleshUpdatedFlags();

        /// <summary>
        /// 1フレームのReplayableInputのデータを記録します
        ///
        /// 基本的にGetValuesEnumerable()で取得できる値を処理対象にしてください。
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        void Record(ReplayableInput input);

        /// <summary>
        /// この関数を呼び出した後は、このインスタンスとReplayableInputのパラメータがFrameのものへ更新されます。
        ///
        /// 基本的にGetValuesEnumerable()で取得できる値を処理対象にしてください。
        /// </summary>
        /// <param name="input"></param>
        void RecoverTo(ReplayableInput input);

        /// <summary>
        /// 基本的にGetValuesEnumerable()で取得できる値を処理対象にしてください。
		/// </summary>
		/// <param name="frameDataRecorder"></param>
        void CopyUpdatedDatasTo(IFrameDataRecorder other);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<FrameInputDataKeyValue> GetValuesEnumerable();

    }

    /// <summary>
    /// <see cref="IFrameDataRecorder"/>
    /// <see cref="IFrameDataRecorder.GetValuesEnumerable()"/>
    /// </summary>
    public class FrameInputDataKeyValue
    {
        public string Key { get; }
        public IUpdateObserver Value { get; }
        public FrameInputDataKeyValue(string key, IUpdateObserver value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator FrameInputDataKeyValue((string key, IUpdateObserver value) data)
            => new FrameInputDataKeyValue(data.key, data.value);
        public static implicit operator (string key, IUpdateObserver value)(FrameInputDataKeyValue data)
            => (data.Key, data.Value);
    }

    public static partial class IFrameDataRecorderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="recorder"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public static InputRecord.Frame WriteToFrame(this IFrameDataRecorder recorder, ISerializer serializer)
        {
            Assert.IsNotNull(serializer);

            var frame = new InputRecord.Frame();
            frame.InputText = serializer.Serialize(recorder);
            return frame;

        }

        /// <summary>
        /// フレームのデータを復元します
        /// この関数を呼び出した後は、このインスタンスとReplayableInputのパラメータがFrameのものへ更新されます。
        /// </summary>
        /// <param name="recorder"></param>
        /// <param name="frame"></param>
        /// <param name="serializer"></param>
        public static void RecoverFromFrame(this IFrameDataRecorder recorder, InputRecord.Frame frame, ISerializer serializer)
        {
            Assert.IsNotNull(frame);
            Assert.IsNotNull(serializer);

            var recoverInput = serializer.Deserialize(frame.InputText, recorder.GetType())
                as IFrameDataRecorder;

            recoverInput.CopyUpdatedDatasTo(recorder);
        }
    }
}
