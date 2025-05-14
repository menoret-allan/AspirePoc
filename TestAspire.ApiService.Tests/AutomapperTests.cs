using AutoMapper;

namespace TestAspire.ApiService.Tests;

public class AutomapperTests
{
    [Fact]
    public void ProfileIsValid()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<AutomapperProfile>());

        configuration.AssertConfigurationIsValid();
    }
}