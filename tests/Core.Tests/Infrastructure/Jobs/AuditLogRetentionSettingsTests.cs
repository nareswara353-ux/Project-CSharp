using FluentAssertions;
using Infrastructure.Jobs;

namespace Core.Tests.Infrastructure.Jobs;

public class AuditLogRetentionSettingsTests
{
    [Fact]
    public void DefaultValues_ShouldBeSensible()
    {
        var settings = new AuditLogRetentionSettings();

        settings.Enabled.Should().BeTrue();
        settings.RetentionDays.Should().Be(365);
        settings.BatchSize.Should().Be(5000);
        settings.ArchiveBeforeDelete.Should().BeFalse();
    }

    [Fact]
    public void SectionName_ShouldBeAuditLogRetention()
    {
        AuditLogRetentionSettings.SectionName.Should().Be("AuditLogRetention");
    }

    [Fact]
    public void Settings_ShouldAllowCustomValues()
    {
        var settings = new AuditLogRetentionSettings
        {
            Enabled = false,
            RetentionDays = 30,
            BatchSize = 500,
            ArchiveBeforeDelete = true
        };

        settings.Enabled.Should().BeFalse();
        settings.RetentionDays.Should().Be(30);
        settings.BatchSize.Should().Be(500);
        settings.ArchiveBeforeDelete.Should().BeTrue();
    }
}
