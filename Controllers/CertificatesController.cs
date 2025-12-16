using InsuranceCertificates.Data;
using InsuranceCertificates.Models;
using InsuranceCertificates.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceCertificates.Controllers;

[ApiController]
[Route("[controller]")]
public class CertificatesController : ControllerBase
{
    private readonly AppDbContext _appDbContext;
    private readonly ICertificateService _certificateService;

    public CertificatesController(AppDbContext appDbContext, ICertificateService certificateService)
    {
        _appDbContext = appDbContext;
        _certificateService = certificateService;
    }

    [HttpGet]
    public async Task<IEnumerable<CertificateModel>> Get()
    {
        return await _appDbContext.Certificates.Select(c => new CertificateModel
        {
            Number = c.Number,
            CreationDate = c.CreationDate,
            ValidFrom = c.ValidFrom,
            ValidTo = c.ValidTo,
            CustomerName = c.Customer.Name,
            CustomerDateOfBirth = c.Customer.DateOfBirth,
            InsuredItem = c.InsuredItem,
            InsuredSum = c.InsuredSum,
            CertificateSum = c.CertificateSum
        }).ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCertificateRequest request)
    {
        var result = await _certificateService.CreateCertificateAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Certificate);
    }
}