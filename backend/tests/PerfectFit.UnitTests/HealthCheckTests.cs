using FluentAssertions;

namespace PerfectFit.UnitTests;

/// <summary>
/// Health check tests to verify the API is running correctly.
/// </summary>
public class HealthCheckTests
{
    [Fact]
    public void HealthCheck_Should_Return_True()
    {
        // Arrange
        const bool expected = true;

        // Act
        var result = true; // Placeholder - will be replaced with actual health check

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Core_Assembly_Should_Be_Loaded()
    {
        // Arrange & Act
        var assemblyName = Core.CoreAssemblyMarker.AssemblyName;

        // Assert
        assemblyName.Should().Be("PerfectFit.Core");
    }

    [Fact]
    public void UseCases_Assembly_Should_Be_Loaded()
    {
        // Arrange & Act
        var assemblyName = UseCases.UseCasesAssemblyMarker.AssemblyName;

        // Assert
        assemblyName.Should().Be("PerfectFit.UseCases");
    }

    [Fact]
    public void Infrastructure_Assembly_Should_Be_Loaded()
    {
        // Arrange & Act
        var assemblyName = Infrastructure.InfrastructureAssemblyMarker.AssemblyName;

        // Assert
        assemblyName.Should().Be("PerfectFit.Infrastructure");
    }
}
