using System;
using System.Collections;
using BCIEssentials.LSLFramework;
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
        private const float k_streamResolveTimeout = 0.1f;
        private static readonly float[] k_limitValues = { 0, 0.1f, 0.5f, 1, 2, 5, 10 };

        [UnityTest]
        public IEnumerator WhenStreamOutletCreatedAndResolveStreams_ThenStreamsResolvedWithinTimeLimit([ValueSource(nameof(k_limitValues))] float limit)
        {
            var streamFound = false;
            var streamName = Guid.NewGuid().ToString();
            NewStreamOutlet(streamName);
            
            var duration = 0f;
            while (!streamFound && duration <= limit)
            {
                var resolvedStreams = LSL.LSL.resolve_streams(k_streamResolveTimeout);

                foreach (var streamInfo in resolvedStreams)
                {
                    if (streamInfo.name() == streamName)
                    {
                        streamFound = true;
                        break;
                    }
                }
                
                duration += Time.fixedDeltaTime;
                yield return null;
            }
            
            Assert.True(streamFound);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WhenStreamOutletCreatedAndUseResolver_ThenStreamsResolvedWithinTimeLimit([ValueSource(nameof(k_limitValues))]float limit)
        {
            var streamFound = false;
            var streamName = Guid.NewGuid().ToString();
            
            NewStreamOutlet(streamName).push_sample(new[]{"marker"}); //Resolver won't find a stream unless it's had samples pushed.
            AddComponent<Resolver>().OnStreamFound += info =>
            {
                if (info.name() != streamName) return;
                streamFound = true;
            };

            var duration = 0f;
            while (!streamFound && duration <= limit)
            {
                duration += Time.fixedDeltaTime;
                yield return null;
            }

            Assert.True(streamFound);
        }

        [UnityTest]
        public IEnumerator WhenStreamOutletCreatedAndUseLSLServiceProvider_ThenStreamsResolvedWithinTimeLimit([ValueSource(nameof(k_limitValues))]float limit)
        {
            var streamFound = false;
            var streamName = Guid.NewGuid().ToString();
            NewStreamOutlet(streamName);
            var provider = AddComponent<LSLServiceProvider>();
            
            var duration = 0f;
            while (!streamFound && duration <= limit)
            {
                if (provider.GetMarkerReceiverByName(streamName) != null)
                {
                    streamFound = true;
                }
                
                duration += Time.fixedDeltaTime;
                yield return null;
            }

            Assert.True(streamFound);
        }

        private static StreamOutlet NewStreamOutlet(string streamName)
        {
            return new StreamOutlet(new StreamInfo(streamName, "", 1, 0D, channel_format_t.cf_string));
        }
    }
}
