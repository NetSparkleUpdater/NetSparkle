using NetSparkleUpdater;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace NetSparkleUnitTests
{
    public class ChannelAppCastFilterTests
    {
        [Fact]
        public void CanFilterItemsByChannel()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            var currentVersion = new SemVerLike("1.0.0", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "2.0-beta1" },
                new AppCastItem() { Version = "1.1-beta1" },
                new AppCastItem() { Version = "1.1-alpha1" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = ["alpha"];
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Single(filtered);
            Assert.Equal("1.1-alpha1", filtered[0].Version);
            filter.ChannelSearchNames = ["beta"];
            filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("2.0-beta1", filtered[0].Version);
        }

        [Fact]
        public void CanUpgradeToNewStableVersionWhileOnBeta()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            var currentVersion = new SemVerLike("1.1-alpha1", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "2.0" },
                new AppCastItem() { Version = "1.1-beta1" },
                new AppCastItem() { Version = "1.1-alpha1" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = ["beta"];
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Single(filtered);
            Assert.Equal("2.0", filtered[0].Version);
        }

        [Fact]
        public void CanFilterByMultipleChannels()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            var currentVersion = new SemVerLike("1.0.0", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "1.1-beta1" },
                new AppCastItem() { Version = "1.1-alpha1" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = ["alpha", "beta"];
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("1.1-beta1", filtered[0].Version);
            Assert.Equal("1.1-alpha1", filtered[1].Version);
        }

        [Fact]
        public void FirstItemIsLatestInSeriesOfChannel()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            var currentVersion = new SemVerLike("1.0.0", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "1.2-beta1" },
                new AppCastItem() { Version = "1.1-beta4" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = ["beta"];
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("1.2-beta1", filtered[0].Version);
            Assert.Equal("1.1-beta4", filtered[1].Version);
        }

        // TODO: test for RemoveOlderItems
        // TODO: test for KeepItemsWithNoSuffix
    }
}
