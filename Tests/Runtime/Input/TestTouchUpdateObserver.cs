using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hinode.Tests.Input
{
    public class TestTouchUpdateObserver : TestBase
    {
        [Test]
        public void BasicUsagePasses()
        {
            var touch = new TouchUpdateObserver();
            touch.AltitudeAngle = 1f;
            touch.AzimuthAngle = 2f;
            touch.DeltaPosition = new Vector2(22, 33);
            touch.DeltaTime = 0.1f;
            touch.FingerId = 1;
            touch.MaximumPossiblePressure = 0.2f;
            touch.Phase = TouchPhase.Moved;
            touch.Position = new Vector2(11, 21);
            touch.Pressure = 0.3f;
            touch.Radius = 12f;
            touch.RadiusVariance = 5f;
            touch.RawPosition = new Vector2(22.2f, 33.3f);
            touch.TapCount = 2;
            touch.Type = TouchType.Indirect;

            foreach(TouchUpdateObserver.ValueKey key in System.Enum.GetValues(typeof(TouchUpdateObserver.ValueKey)))
            {
                Assert.IsTrue(touch.DidUpdatedKey(key), $"'{key}' did not update...");
            }

            var serializer = new JsonSerializer();
            var json = serializer.Serialize(touch);
            Debug.Log($"debug json => {json}");
            var dest = serializer.Deserialize<TouchUpdateObserver>(json);

            Assert.AreEqual(touch.AltitudeAngle, dest.AltitudeAngle);
            Assert.AreEqual(touch.AzimuthAngle, dest.AzimuthAngle);
            Assert.AreEqual(touch.DeltaPosition, dest.DeltaPosition);
            Assert.AreEqual(touch.DeltaTime, dest.DeltaTime);
            Assert.AreEqual(touch.FingerId, dest.FingerId);
            Assert.AreEqual(touch.MaximumPossiblePressure, dest.MaximumPossiblePressure);
            Assert.AreEqual(touch.Phase, dest.Phase);
            Assert.AreEqual(touch.Position, dest.Position);
            Assert.AreEqual(touch.Pressure, dest.Pressure);
            Assert.AreEqual(touch.Radius, dest.Radius);
            Assert.AreEqual(touch.RadiusVariance, dest.RadiusVariance);
            Assert.AreEqual(touch.RawPosition, dest.RawPosition);
            Assert.AreEqual(touch.TapCount, dest.TapCount);
            Assert.AreEqual(touch.Type, dest.Type);
            Assert.IsTrue(touch.Equals(dest));
        }

        [Test]
        public void CastTouchPasses()
        {
            var touch = new TouchUpdateObserver();
            touch.AltitudeAngle = 1f;
            touch.AzimuthAngle = 2f;
            touch.DeltaPosition = new Vector2(22, 33);
            touch.DeltaTime = 0.1f;
            touch.FingerId = 1;
            touch.MaximumPossiblePressure = 0.2f;
            touch.Phase = TouchPhase.Moved;
            touch.Position = new Vector2(11, 21);
            touch.Pressure = 0.3f;
            touch.Radius = 12f;
            touch.RadiusVariance = 5f;
            touch.RawPosition = new Vector2(22.2f, 33.3f);
            touch.TapCount = 2;
            touch.Type = TouchType.Indirect;

            var rawTouch = (Touch)touch;
            Assert.IsTrue(touch.Equals(rawTouch));
        }
    }
}