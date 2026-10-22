using System.Collections;
using UnityEngine;
using BCIEssentials.LSLFramework;
using NUnit.Framework;

namespace BCIEssentials.Tests.LSLFramework
{
    public class ThreadedResponseProviderTests: ResponseProviderTests
    {
        protected override ResponseProvider InStream => _provider;
        ThreadedResponseProvider _provider;
        readonly WaitForSeconds _transmissionDelay = new(0.05f);

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _provider = new()
            {
                StreamType = OutletType,
                PollingPeriod = 0.02f
            };
        }

        protected override IEnumerator StepProvider()
        {
            yield return _transmissionDelay;
        }

        [Test]
        public void WhenCreated_ThenNotPolling()
        {
            Assert.IsFalse(_provider.IsPolling);
        }

        [Test]
        public void Subscribe_WhenNotPolling_ThenStartsPolling()
        {
            Assert.IsFalse(_provider.IsPolling);
            InStream.SubscribeAll(r => {});
            Assert.IsTrue(_provider.IsPolling);
        }

        [Test]
        public void Unsubscribe_WhenPolling_ThenNotPolling()
        {
            static void dummySubscriber(Response r) { }
            InStream.SubscribeAll(dummySubscriber);
            Assert.IsTrue(_provider.IsPolling);
            InStream.UnsubscribeAll(dummySubscriber);
            Assert.IsFalse(_provider.IsPolling);
        }
    }
}