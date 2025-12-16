namespace InsuranceCertificates.Models;

public class CreateCertificateRequest
{
    public required string CustomerName { get; set; }

    public required DateTime CustomerDateOfBirth { get; set; }

    public required string InsuredItem { get; set; }

    public required decimal InsuredSum { get; set; }
}
