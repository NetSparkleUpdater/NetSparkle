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
        public void CanFilterItemsByVersionChannel()
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
            filter.ChannelSearchNames = new List<string>() { "alpha" };
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Single(filtered);
            Assert.Equal("1.1-alpha1", filtered[0].Version);
            filter.ChannelSearchNames = new List<string>() { "beta" };
            filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("2.0-beta1", filtered[0].Version);
        }

        [Fact]
        public void CanFilterItemsByVersionChannelStrangeCasing()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            var currentVersion = new SemVerLike("1.0.0", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "2.0-BEta1" },
                new AppCastItem() { Version = "1.1-beta1" },
                new AppCastItem() { Version = "1.1-AlPha1" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = new List<string>() { "alpha" };
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Single(filtered);
            Assert.Equal("1.1-AlPha1", filtered[0].Version);
            filter.ChannelSearchNames = new List<string>() { "beta" };
            filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("2.0-BEta1", filtered[0].Version);
            Assert.Equal("1.1-beta1", filtered[1].Version);
        }

        [Fact]
        public void CanFilterItemsByAppCastItemChannel()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            var currentVersion = new SemVerLike("1.0.0", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "2.0", Channel = "gamma" },
                new AppCastItem() { Version = "1.2-preview", Channel = "beta" },
                new AppCastItem() { Version = "1.1-beta1" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = new List<string>() { "gamma" };
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Single(filtered);
            Assert.Equal("2.0", filtered[0].Version);
            filter.ChannelSearchNames = new List<string>() { "beta" };
            filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("1.2-preview", filtered[0].Version);
            Assert.Equal("beta", filtered[0].Channel);
            Assert.Equal("1.1-beta1", filtered[1].Version);
            Assert.Null(filtered[1].Channel);
            filter.ChannelSearchNames = new List<string>() { "beta", "preview" };
            filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("1.2-preview", filtered[0].Version);
            Assert.Equal("beta", filtered[0].Channel);
            Assert.Equal("1.1-beta1", filtered[1].Version);
            Assert.Null(filtered[1].Channel);
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
            filter.ChannelSearchNames = new List<string>() { "beta" };
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
            filter.ChannelSearchNames = new List<string>() { "alpha", "beta" };
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
            filter.ChannelSearchNames = new List<string>() { "beta" };
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("1.2-beta1", filtered[0].Version);
            Assert.Equal("1.1-beta4", filtered[1].Version);
        }

        [Fact]
        public void NonStableIgnoredIfNoChannels()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            var currentVersion = new SemVerLike("1.0.0", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "2.0-beta1" },
                new AppCastItem() { Version = "1.9" },
                new AppCastItem() { Version = "1.3-beta1" },
                new AppCastItem() { Version = "1.2-beta1" },
                new AppCastItem() { Version = "1.2" },
                new AppCastItem() { Version = "1.1-beta4" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = new List<string>() { };
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("1.9", filtered[0].Version);
            Assert.Equal("1.2", filtered[1].Version);
        }

        [Fact]
        public void GetsLatestIfBothStableAndChannelAvailable()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            var currentVersion = new SemVerLike("1.0.0", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "1.1" },
                new AppCastItem() { Version = "1.1-rc1" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = new List<string>() { "rc" };
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("1.1", filtered[0].Version);
            Assert.Equal("1.1-rc1", filtered[1].Version);

            items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "1.2-beta1" },
                new AppCastItem() { Version = "1.1" },
                new AppCastItem() { Version = "1.1-rc1" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = new List<string>() { "rc", "beta" };
            filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(3, filtered.Count);
            Assert.Equal("1.2-beta1", filtered[0].Version);
            Assert.Equal("1.1", filtered[1].Version);
            Assert.Equal("1.1-rc1", filtered[2].Version);
        }

        [Fact]
        public void TestRemoveOlderItems()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            filter.RemoveOlderItems = false;
            var currentVersion = new SemVerLike("1.0.0", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "2.0-beta1" },
                new AppCastItem() { Version = "1.1-beta4" },
                new AppCastItem() { Version = "1.0.0" },
                new AppCastItem() { Version = "0.9.0" },
                new AppCastItem() { Version = "0.4.0" },
            };
            filter.ChannelSearchNames = new List<string>() { "beta" };
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(5, filtered.Count);
            Assert.Equal("2.0-beta1", filtered[0].Version);
            Assert.Equal("1.1-beta4", filtered[1].Version);
            Assert.Equal("1.0.0", filtered[2].Version);
            Assert.Equal("0.9.0", filtered[3].Version);
            Assert.Equal("0.4.0", filtered[4].Version);
            // now remove older items
            filter.RemoveOlderItems = true;
            filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("2.0-beta1", filtered[0].Version);
            Assert.Equal("1.1-beta4", filtered[1].Version);
        }

        [Fact]
        public void TestKeepItemsWithSuffix()
        {
            var logWriter = new LogWriter(LogWriterOutputMode.Console);
            var filter = new ChannelAppCastFilter(logWriter);
            filter.KeepItemsWithNoChannelInfo = true;
            var currentVersion = new SemVerLike("1.0.0", "");
            var items = new List<AppCastItem>()
            {
                new AppCastItem() { Version = "2.0", Channel = "" },
                new AppCastItem() { Version = "2.0", Channel = "gamma" }, // will be discarded!
                new AppCastItem() { Version = "2.0-beta1" },
                new AppCastItem() { Version = "1.0.0" },
            };
            filter.ChannelSearchNames = new List<string>() { "beta" };
            var filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Equal(2, filtered.Count);
            Assert.Equal("2.0", filtered[0].Version);
            Assert.Equal("2.0-beta1", filtered[1].Version);
            // now don't keep items with no channel names
            filter.KeepItemsWithNoChannelInfo = false;
            filtered = filter.GetFilteredAppCastItems(currentVersion, items).ToList();
            Assert.Single(filtered);
            Assert.Equal("2.0-beta1", filtered[0].Version);
        }
    }
}
