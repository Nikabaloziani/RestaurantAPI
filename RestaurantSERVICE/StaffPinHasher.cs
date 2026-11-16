using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RestaurantDATA.Entities;
using RestaurantSERVICE.Interfaces;

namespace RestaurantSERVICE.Services;

public sealed class StaffPinHasher : IStaffPinHasher
{
    private readonly byte[] _pepperV1;
    private readonly IPasswordHasher<StaffPinCredential> _passwordHasher;

    public StaffPinHasher(IConfiguration config)
    {
        var pepperBase64 = config["StaffPin:PepperV1"];
        if (string.IsNullOrWhiteSpace(pepperBase64))
            throw new InvalidOperationException(
                "StaffPin:PepperV1 is missing. Configure StaffPin__PepperV1 securely.");

        try
        {
            _pepperV1 = Convert.FromBase64String(pepperBase64);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "StaffPin:PepperV1 must be a Base64-encoded random secret.", ex);
        }

        if (_pepperV1.Length < 32)
            throw new InvalidOperationException(
                "StaffPin:PepperV1 must decode to at least 32 random bytes.");

        _passwordHasher = new PasswordHasher<StaffPinCredential>(
            Options.Create(new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                IterationCount = 600_000
            }));
    }

    public string Hash(StaffPinCredential credential, string pin)
    {
        ValidateVersion(credential);
        return _passwordHasher.HashPassword(credential, PreHash(credential, pin));
    }

    public PasswordVerificationResult Verify(StaffPinCredential credential, string pin)
    {
        ValidateVersion(credential);
        if (string.IsNullOrWhiteSpace(credential.PinHash))
            return PasswordVerificationResult.Failed;

        return _passwordHasher.VerifyHashedPassword(
            credential, credential.PinHash, PreHash(credential, pin));
    }

    private string PreHash(StaffPinCredential credential, string pin)
    {
        // Bind the credential to both organization and user identity.
        var material = Encoding.UTF8.GetBytes(
            $"RestaurantOS:StaffPin:v1:{credential.OrganizationId:N}:{credential.UserId}:{pin}");
        return Convert.ToBase64String(HMACSHA256.HashData(_pepperV1, material));
    }

    private static void ValidateVersion(StaffPinCredential credential)
    {
        if (credential.PepperKeyVersion != 1)
            throw new InvalidOperationException(
                "Unsupported PIN pepper version. Account password reset is required.");
    }
}
