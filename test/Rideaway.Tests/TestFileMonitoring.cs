using System;
using System.IO;
using System.Threading;
using rideaway_backend.FileMonitoring;
using Xunit;

namespace Rideaway.Tests
{
    public class TestFileMonitoring
    {
        private static string _contents;

        [Fact]
        public void Test1()
        {
            _contents = "Abc";

            var p = "MonitoredFileTest"                ;
            File.WriteAllText(p, _contents);

            var fm = new FileMonitor(p, TimeSpan.FromMilliseconds(500),
                () => { _contents = File.ReadAllText(p); });


            Thread.Sleep(1000);
            File.WriteAllText(p, "def");
            Assert.Equal("Abc",  _contents);

            Thread.Sleep(1000);
            Assert.Equal("def",  _contents);
        }
    }
}