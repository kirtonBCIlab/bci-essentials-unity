using System;
using System.Collections;
using System.Collections.Generic;
using BCIEssentials.LSLFramework;
using BCIEssentials.Tests.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace BCIEssentials.Tests
{
    [TestFixture]
    public class LSLResponseStreamTests : PlayModeTestRunnerBase
    {
        private string _testStreamName;
        private LSLResponseStream _testResponseStream;
        private LSLMarkerStream _testMarkerStream;
        
        [UnitySetUp]
        public override IEnumerator TestSetup()
        {
            Debug.Log($"<b><color=green>Starting Test Run:</color></b> {TestContext.CurrentContext.Test.Name }");
            yield return LoadEmptySceneAsync();
            
            _testMarkerStream = AddComponent<LSLMarkerStream>();
            _testMarkerStream.gameObject.name = Guid.NewGuid().ToString();
            Debug.Log($"marker go name: '{_testMarkerStream.gameObject.name}'");
            
            _testMarkerStream.StreamName = "PythonResponse";
            _testMarkerStream.InitializeStream();
            
            _testResponseStream = AddComponent<LSLResponseStream>();
        }

        [Test]
        public void WhenConnect_ThenConnected()
        {
            _testResponseStream.Connect();
            
            Assert.IsTrue(_testResponseStream.Connected);
        }

        [Test]
        public void WhenConnectWithTargetStreamName_ThenConnected()
        {
            var markerStream = AddComponent<LSLMarkerStream>();
            markerStream.StreamName = "mystreamname";
            markerStream.InitializeStream();
            
            _testResponseStream.Connect("mystreamname");
            
            Assert.IsTrue(_testResponseStream.Connected);
        }

        [Test]
        public void WhenConnectAndStreamDoesNotExists_ThenNotConnected()
        {
            Object.DestroyImmediate(_testMarkerStream);
            
            _testResponseStream.Connect();
            
            Assert.IsFalse(_testResponseStream.Connected);
        }

        [Test]
        public void WhenConnectedAndConnect_ThenDisconnectThenConnect()
        {
            _testResponseStream.Connect();
            _testResponseStream.Connect();
            
            Assert.IsTrue(_testResponseStream.Connected);
        }

        [Test]
        public void WhenDisconnect_ThenDisconnected()
        {
            _testResponseStream.Connect();
            
            _testResponseStream.Disconnect();
            
            Assert.IsFalse(_testResponseStream.Connected);
        }

        [UnityTest]
        public IEnumerator WhenDisconnectAndWasPolling_ThenDisconnectedAndNoResponses()
        {
            var writeMarkers = RepeatForSeconds(() => { _testMarkerStream.Write("amarkervalue"); }, 5);
            _testResponseStream.Connect();
            
            writeMarkers.StartRun();
            _testResponseStream.StartPolling();
            yield return new WaitUntil(()=> !writeMarkers.IsRunning);
            _testResponseStream.Disconnect();
            
            Assert.IsFalse(_testResponseStream.Connected);
            Assert.IsFalse(_testResponseStream.HasStoredResponses);
        }

        [UnityTest]
        public IEnumerator WhenStartReceiving_ThenReceiveResponses()
        {
            var writeMarkers = RepeatForSeconds(() => { _testMarkerStream.Write("amarkervalue"); }, 5);
            _testResponseStream.Connect();
            
            writeMarkers.StartRun();
            _testResponseStream.StartPolling();
            yield return new WaitWhile(()=> writeMarkers.IsRunning);
            
            Assert.IsTrue(_testResponseStream.HasStoredResponses);
        }

        [UnityTest]
        public IEnumerator WhenMultipleSubscribedToSameStream_ThenMarkersPoached()
        {
            var writeMarkers = RepeatForSeconds(() => { _testMarkerStream.Write("amarkervalue"); }, 5);
            _testResponseStream.Connect();
            var secondResponseStream = AddComponent<LSLResponseStream>();

            
            writeMarkers.StartRun();
            _testResponseStream.StartPolling();
            secondResponseStream.StartPolling(_ =>
            {
                Assert.Fail("Second Response Stream received markers");
            });
            
            yield return new WaitWhile(()=> writeMarkers.IsRunning);
            
            Assert.IsTrue(_testResponseStream.HasStoredResponses);
            Assert.IsFalse(secondResponseStream.HasStoredResponses);
        }

        [UnityTest]
        //FAILS IF RUN AS PART OF GROUP
        public IEnumerator WhenStartReceivingWithAction_ThenReceiveResponses()
        {
            var writeMarkers = RepeatForSeconds(() => { _testMarkerStream.Write("amarkervalue"); }, 5);
            var responses = new List<string[]>();
            _testResponseStream.Connect();
            
            _testResponseStream.StartPolling((rs)=> responses.Add(rs));
            writeMarkers.StartRun();
            yield return new WaitWhile(()=> writeMarkers.IsRunning);
            
            Assert.AreEqual(5, responses.Count);
        }

        [UnityTest]
        //FAILS IF RUN AS PART OF GROUP
        public IEnumerator WhenStopReceiving_ThenStopReceivingResponses()
        {
            _testResponseStream.Connect();
            _testResponseStream.StartPolling();
            
            _testMarkerStream.Write("amarkervalue");
            yield return null;
            var hadResponses = _testResponseStream.HasStoredResponses;
            _testResponseStream.StopPolling();
            yield return null;
            _testMarkerStream.Write("amarkervalue");
            yield return null;
            
            Assert.AreNotEqual(hadResponses, _testResponseStream.HasStoredResponses);
        }

        [UnityTest]
        public IEnumerator WhenGetResponses_ThenReturnsResponses()
        {
            _testResponseStream.Connect();
            _testMarkerStream.Write("amarkervalue");
            _testMarkerStream.Write("amarkervalue");
            _testMarkerStream.Write("amarkervalue");
            yield return null;

            var responses = _testResponseStream.GetResponses();
            
            Assert.AreEqual(3, responses.Length);
        }

        [UnityTest]
        public IEnumerator WhenPollingAndGetResponses_ThenReturnsAllAvailableResponses()
        {
            _testResponseStream.Connect();
            _testResponseStream.StartPolling();
            var sentMarkers = 3;
            for (int i = 0; i < sentMarkers; i++)
            {
                _testMarkerStream.Write("amarkervalue");
                yield return new WaitForSeconds(_testResponseStream.PollFrequency);
            }
            
            
            var responses = _testResponseStream.GetResponses();
            
            Assert.AreEqual(sentMarkers, responses.Length);
            Assert.False(_testResponseStream.HasStoredResponses);
        }

        [Test]
        public void WhenClearResponses_ThenCurrentResponsesCleared()
        {
            _testResponseStream.Connect();
            _testMarkerStream.Write("amarkervalue");
            _testResponseStream.StartPolling();

            var hadResponses = _testResponseStream.HasStoredResponses;
            
            _testResponseStream.ClearPolledResponses();
            
            Assert.AreNotEqual(hadResponses, _testResponseStream.HasStoredResponses);
        }
    }
}