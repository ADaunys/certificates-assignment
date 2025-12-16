using InsuranceCertificates.Data;
using InsuranceCertificates.Domain;
using InsuranceCertificates.Models;
using Microsoft.EntityFrameworkCore;

namespace InsuranceCertificates.Services;

public class CertificateService : ICertificateService
{
    private const int MinimumAge = 18;
    private readonly AppDbContext _dbContext;

    public CertificateService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CertificateCreationResult> CreateCertificateAsync(CreateCertificateRequest request)
    {
        var validationError = ValidateRequest(request);
        if (validationError != null)
        {
            return CertificateCreationResult.Failure(validationError);
        }

        var certificateSum = CalculateCertificateSum(request.InsuredSum);
        if (certificateSum == null)
        {
            return CertificateCreationResult.Failure("Insured item price must be between 20.00 and 200.00.");
        }

        var creationDate = DateTime.UtcNow;
        var certificateNumber = await GenerateCertificateNumberAsync();

        var customer = new Customer
        {
            Name = request.CustomerName,
            DateOfBirth = request.CustomerDateOfBirth
        };

        var certificate = new Certificate
        {
            Number = certificateNumber,
            CreationDate = creationDate,
            ValidFrom = creationDate,
            ValidTo = creationDate.AddYears(1).Date,
            Customer = customer,
            InsuredItem = request.InsuredItem,
            InsuredSum = request.InsuredSum,
            CertificateSum = certificateSum.Value
        };

        _dbContext.Certificates.Add(certificate);
        await _dbContext.SaveChangesAsync();

        var certificateModel = new CertificateModel
        {
            Number = certificate.Number,
            CreationDate = certificate.CreationDate,
            ValidFrom = certificate.ValidFrom,
            ValidTo = certificate.ValidTo,
            CustomerName = customer.Name,
            CustomerDateOfBirth = customer.DateOfBirth,
            InsuredItem = certificate.InsuredItem,
            InsuredSum = certificate.InsuredSum,
            CertificateSum = certificate.CertificateSum
        };

        return CertificateCreationResult.Success(certificateModel);
    }

    private string? ValidateRequest(CreateCertificateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return "Customer name is required.";
        }

        if (string.IsNullOrWhiteSpace(request.InsuredItem))
        {
            return "Insured item is required.";
        }

        var age = CalculateAge(request.CustomerDateOfBirth);
        if (age < MinimumAge)
        {
            return $"Customer must be at least {MinimumAge} years old.";
        }

        return null;
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    private static decimal? CalculateCertificateSum(decimal insuredSum)
    {
        return insuredSum switch
        {
            >= 20.00m and <= 50.00m => 8m,
            > 50.00m and <= 100.00m => 15m,
            > 100.00m and <= 200.00m => 25m,
            _ => null
        };
    }

    private async Task<string> GenerateCertificateNumberAsync()
    {
        var lastCertificate = await _dbContext.Certificates
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastCertificate != null && int.TryParse(lastCertificate.Number, out var lastNumber))
        {
            nextNumber = lastNumber + 1;
        }

        return nextNumber.ToString("D5");
    }
}
