using InsuranceCertificates.Data;
using InsuranceCertificates.Models;
using InsuranceCertificates.Services;
using Microsoft.EntityFrameworkCore;

namespace InsuranceCertificates.Tests;

public class CertificateServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateCertificate_WithValidData_ReturnsSuccess()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var service = new CertificateService(dbContext);
        var request = new CreateCertificateRequest
        {
            CustomerName = "John Doe",
            CustomerDateOfBirth = new DateTime(1990, 1, 1),
            InsuredItem = "iPhone 15",
            InsuredSum = 75m
        };

        // Act
        var result = await service.CreateCertificateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Certificate);
        Assert.Equal("00001", result.Certificate.Number);
        Assert.Equal(15m, result.Certificate.CertificateSum);
    }

    [Fact]
    public async Task CreateCertificate_WithCustomerUnder18_ReturnsFailure()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var service = new CertificateService(dbContext);
        var request = new CreateCertificateRequest
        {
            CustomerName = "Young Person",
            CustomerDateOfBirth = DateTime.UtcNow.AddYears(-17),
            InsuredItem = "iPhone 15",
            InsuredSum = 75m
        };

        // Act
        var result = await service.CreateCertificateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("18 years old", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateCertificate_WithPriceTooLow_ReturnsFailure()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var service = new CertificateService(dbContext);
        var request = new CreateCertificateRequest
        {
            CustomerName = "John Doe",
            CustomerDateOfBirth = new DateTime(1990, 1, 1),
            InsuredItem = "Cheap Item",
            InsuredSum = 10m
        };

        // Act
        var result = await service.CreateCertificateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("between 20.00 and 200.00", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateCertificate_WithPriceTooHigh_ReturnsFailure()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var service = new CertificateService(dbContext);
        var request = new CreateCertificateRequest
        {
            CustomerName = "John Doe",
            CustomerDateOfBirth = new DateTime(1990, 1, 1),
            InsuredItem = "Expensive Item",
            InsuredSum = 500m
        };

        // Act
        var result = await service.CreateCertificateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("between 20.00 and 200.00", result.ErrorMessage);
    }

    [Theory]
    [InlineData(20.00, 8)]
    [InlineData(50.00, 8)]
    [InlineData(50.01, 15)]
    [InlineData(75.00, 15)]
    [InlineData(100.00, 15)]
    [InlineData(100.01, 25)]
    [InlineData(150.00, 25)]
    [InlineData(200.00, 25)]
    public async Task CreateCertificate_CalculatesCorrectCertificateSum(decimal insuredSum, decimal expectedSum)
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var service = new CertificateService(dbContext);
        var request = new CreateCertificateRequest
        {
            CustomerName = "John Doe",
            CustomerDateOfBirth = new DateTime(1990, 1, 1),
            InsuredItem = "Test Item",
            InsuredSum = insuredSum
        };

        // Act
        var result = await service.CreateCertificateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedSum, result.Certificate!.CertificateSum);
    }

    [Fact]
    public async Task CreateCertificate_GeneratesSequentialNumbers()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var service = new CertificateService(dbContext);
        var request = new CreateCertificateRequest
        {
            CustomerName = "John Doe",
            CustomerDateOfBirth = new DateTime(1990, 1, 1),
            InsuredItem = "Test Item",
            InsuredSum = 75m
        };

        // Act
        var result1 = await service.CreateCertificateAsync(request);
        var result2 = await service.CreateCertificateAsync(request);
        var result3 = await service.CreateCertificateAsync(request);

        // Assert
        Assert.Equal("00001", result1.Certificate!.Number);
        Assert.Equal("00002", result2.Certificate!.Number);
        Assert.Equal("00003", result3.Certificate!.Number);
    }

    [Fact]
    public async Task CreateCertificate_SetsValidToAtMidnightOneYearLater()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var service = new CertificateService(dbContext);
        var request = new CreateCertificateRequest
        {
            CustomerName = "John Doe",
            CustomerDateOfBirth = new DateTime(1990, 1, 1),
            InsuredItem = "Test Item",
            InsuredSum = 75m
        };

        // Act
        var result = await service.CreateCertificateAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        var certificate = result.Certificate!;
        var expectedValidTo = certificate.CreationDate.AddYears(1).Date;
        Assert.Equal(expectedValidTo, certificate.ValidTo);
        Assert.Equal(TimeSpan.Zero, certificate.ValidTo.TimeOfDay);
    }

    [Fact]
    public async Task CreateCertificate_WithEmptyCustomerName_ReturnsFailure()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var service = new CertificateService(dbContext);
        var request = new CreateCertificateRequest
        {
            CustomerName = "",
            CustomerDateOfBirth = new DateTime(1990, 1, 1),
            InsuredItem = "Test Item",
            InsuredSum = 75m
        };

        // Act
        var result = await service.CreateCertificateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Customer name", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateCertificate_WithEmptyInsuredItem_ReturnsFailure()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var service = new CertificateService(dbContext);
        var request = new CreateCertificateRequest
        {
            CustomerName = "John Doe",
            CustomerDateOfBirth = new DateTime(1990, 1, 1),
            InsuredItem = "",
            InsuredSum = 75m
        };

        // Act
        var result = await service.CreateCertificateAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Insured item", result.ErrorMessage);
    }
}
