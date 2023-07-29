using System;
using System.Collections;
using BCIEssentials.Tests.Utilities;
using LSL;
using LSL4Unity.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BCIEssentials.Tests.LSLService
{
    public class ResolveStreamTests : PlayModeTestRunnerBase
    {
        private const float k_testTimeoutDuration = 10;
        private const float k_streamResolveTimeout = 1;

        [Test]
        public void WhenStreamOutletCreatedAndResolveStreams_ThenStreamAvailableImmediately()
        {
            var streamName = Guid.NewGuid().ToString();
            NewStreamOutlet(streamName);

            var resolvedStreams = LSL.LSL.resolve_streams(k_streamResolveTimeout);

            foreach (var streamInfo in resolvedStreams)
            {
                if (streamInfo.name() != streamName) continue;
                
                Assert.Pass("A matching stream was found");
                return;
            }
            
            Assert.Fail("No matching stream was found.");
        }

        [UnityTest]
        public IEnumerator WhenStreamOutletCreatedAndUseResolver_ThenStreamsResolvedWithinReasonableTime()
        {
            var streamName = Guid.NewGuid().ToString();
            bool streamResolved = false;
            
            //Resolver won't find a stream unless it's had samples pushed.
            NewStreamOutlet(streamName).push_sample(new[]{"marker"}); 
            AddComponent<Resolver>().OnStreamFound += info =>
            {
                streamResolved = info.name() == streamName;
            };

            yield return new WaitForSecondsRealtime(1); //Continue test next frame for resolver coroutine to run again.
            
            var duration = 0f;
            while (!streamResolved && duration < k_testTimeoutDuration)
            {
                duration += Time.fixedDeltaTime;
                yield return null;
            }
            
            Assert.True(streamResolved);
        }

        private static StreamOutlet NewStreamOutlet(string streamName)
        {
            return new StreamOutlet(new StreamInfo(streamName, "", 1, 0D, channel_format_t.cf_string));
        }
    }
}
