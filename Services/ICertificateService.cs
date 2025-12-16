using InsuranceCertificates.Models;

namespace InsuranceCertificates.Services;

public interface ICertificateService
{
    Task<CertificateCreationResult> CreateCertificateAsync(CreateCertificateRequest request);
}

public class CertificateCreationResult
{
    public bool IsSuccess { get; private set; }
    public CertificateModel? Certificate { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static CertificateCreationResult Success(CertificateModel certificate)
        => new() { IsSuccess = true, Certificate = certificate };

    public static CertificateCreationResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
