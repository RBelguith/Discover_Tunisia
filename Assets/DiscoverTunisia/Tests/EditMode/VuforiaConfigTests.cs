using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace DiscoverTunisia.Tests
{
    public class VuforiaConfigTests
    {
        private const string ConfigPath = "Assets/Resources/VuforiaConfiguration.asset";

        [Test]
        public void CommittedConfig_HasNoLicenseKey()
        {
            if (!File.Exists(ConfigPath)) Assert.Ignore("Vuforia is not installed.");
            var match = Regex.Match(File.ReadAllText(ConfigPath), @"vuforiaLicenseKey:[ \t]*(\S*)");
            Assert.That(match.Success, Is.True, "vuforiaLicenseKey field not found");
            Assert.That(match.Groups[1].Value, Is.Empty, "The license key belongs in Settings/Resources/VuforiaLicenseKey.txt, which git ignores");
        }

        [Test]
        public void CommittedConfig_DelaysInitialization()
        {
            if (!File.Exists(ConfigPath)) Assert.Ignore("Vuforia is not installed.");
            StringAssert.Contains("delayedInitialization: 1", File.ReadAllText(ConfigPath),
                "The license key is set from code, so Vuforia must wait for VuforiaImageTracker to start it");
        }
    }
}
