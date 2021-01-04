using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProgressHierarchy.Tests
{
    using ProgressHierarchy;
    
    public class ProgressTests
    {
        [Test]
        public void Progress_Report_raises_event()
        {
            var sut = new Progress();

            var eventsRaised = 0;
            sut.ProgressChanged += (sender, args) => eventsRaised++;

            sut.Report(0.5);

            Assert.That(eventsRaised, Is.EqualTo(1));
        }

        [Test]
        public void Progress_Report_doesnt_throw_without_subscriber()
        {
            var sut = new Progress();

            Assert.That(() => sut.Report(0.5), Throws.Nothing);
        }

        [Test]
        public void Progress_Report_raises_correct_args()
        {
            var sut = new Progress();

            var eventArgs = new List<ProgressChangedEventArgs>();
            sut.ProgressChanged += (sender, args) => eventArgs.Add(args);

            sut.Report(0.5, "Testing");

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Progress, Is.EqualTo(0.5));
            Assert.That(eventArgs[0].Messages, Is.EqualTo(new[] { "Testing" }));
        }

        [Test]
        public void Progress_Dispose_raises_100_percent_event()
        {
            var sut = new Progress();

            var eventArgs = new List<ProgressChangedEventArgs>();
            sut.ProgressChanged += (sender, args) => eventArgs.Add(args);
            sut.Dispose();

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Progress, Is.EqualTo(1));
        }

        [Test]
        public void Methods_on_disposed_instance_throw()
        {
            var sut = new Progress();
            sut.Dispose();

            Assert.That(() => sut.Report(0.5), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => sut.Report(0.5, "Testing"), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => sut.Fork(), Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => sut.ProgressChanged += NoopEventHandler, Throws.InstanceOf<ObjectDisposedException>());
            Assert.That(() => sut.ProgressChanged -= NoopEventHandler, Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        public void Event_handlers_can_be_added_and_removed()
        {
            var sut = new Progress();

            var eventCount = 0;
            void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs args) => eventCount++;

            sut.ProgressChanged += ProgressChangedEventHandler;
            
            sut.Report(0.5);

            sut.ProgressChanged -= ProgressChangedEventHandler;

            sut.Report(0.75);

            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void Fork_scale_smaller_0_throws()
        {
            var sut = new Progress();
            
            Assert.That(() => sut.Fork(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Report_after_Fork_throws()
        {
            var sut = new Progress();
            sut.Fork();

            Assert.That(() => sut.Report(0.5), Throws.InvalidOperationException);
        }

        [Test]
        public void Fork_after_Report_throws()
        {
            var sut = new Progress();
            sut.Report(0.5);

            Assert.That(() => sut.Fork(), Throws.InvalidOperationException);
        }

        [Test]
        public void Forked_progress_scales_correctly()
        {
            var sut = new Progress();

            var eventArgs = new List<ProgressChangedEventArgs>();
            sut.ProgressChanged += (sender, args) => eventArgs.Add(args);

            var sut2 = sut.Fork(0.5);

            sut2.Report(0.5);

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Progress, Is.EqualTo(0.25));
        }

        [Test]
        public void Forked_progress_concatenates_messages_correctly()
        {
            var sut = new Progress();

            var eventArgs = new List<ProgressChangedEventArgs>();
            sut.ProgressChanged += (sender, args) => eventArgs.Add(args);

            var sut2 = sut.Fork(message:"Forking");

            sut2.Report(0.5, "Testing");

            Assert.That(eventArgs.Count, Is.EqualTo(1));
            Assert.That(eventArgs[0].Messages, Is.EqualTo(new[] { "Forking", "Testing" }));
        }

        private static void NoopEventHandler(object o, ProgressChangedEventArgs progressChangedEventArgs)
        {
        }
    }
}
