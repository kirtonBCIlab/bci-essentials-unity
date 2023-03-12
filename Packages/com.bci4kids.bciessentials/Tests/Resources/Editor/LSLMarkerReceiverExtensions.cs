using BCIEssentials.LSLFramework;
using NUnit.Framework;

namespace Tests.Resources.Scripts
{
    public static class LSLMarkerReceiverExtensions
    {
        public static LSLMarkerReceiver InitializeWithStreamName(this LSLMarkerReceiver receiver, string streamName, LSLMarkerReceiverSettings settings = null)
        {
            var resolvedSteams = LSL.LSL.resolve_stream($"name='{streamName}'", 0, 0);
            if (resolvedSteams.Length == 0)
            {
                Assert.Fail($"No stream found for name: {streamName}");
            }

            var streamInfo = resolvedSteams[0];
            return receiver.Initialize(streamInfo.uid(), streamInfo, settings);
        }
    }
}