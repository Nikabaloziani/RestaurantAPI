using RestaurantDATA.Entities;
using Microsoft.AspNetCore.Identity;

namespace RestaurantSERVICE.Interfaces;

/// <summary>
/// Internal hashing service. Do not expose PIN verification over HTTP
/// until trusted-device validation, rate limiting and lockouts are implemented.
/// </summary>
public interface IStaffPinHasher
{
    string Hash(StaffPinCredential credential, string pin);
    PasswordVerificationResult Verify(StaffPinCredential credential, string pin);
}
