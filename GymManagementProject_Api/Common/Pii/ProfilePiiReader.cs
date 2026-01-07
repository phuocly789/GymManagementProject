using GymManagementProject_Infrastructure.Models;

public interface IProfilePiiReader
{
    void FillFromUserProfile(IPiiTarget target, UserProfile profile, bool canSeePii);

    void FillFromMemberProfile(IPiiTarget target, MemberProfile profile, bool canSeePii);
}

public class ProfilePiiReader : IProfilePiiReader
{
    private readonly IEncryptionService _encryptionService;

    public ProfilePiiReader(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public void FillFromUserProfile(IPiiTarget target, UserProfile profile, bool canSeePii)
    {
        if (target == null || profile == null)
            return;

        FillCommon(
            target,
            profile.PhoneEnc,
            profile.IdentityCardNoEnc,
            profile.AddressFullEnc,
            profile.DateOfBirth,
            profile.Gender,
            canSeePii
        );
    }

    public void FillFromMemberProfile(IPiiTarget target, MemberProfile profile, bool canSeePii)
    {
        if (target == null || profile == null)
            return;

        FillCommon(
            target,
            profile.PhoneEnc,
            null,
            profile.AddressFullEnc,
            profile.DateOfBirth,
            profile.Gender,
            canSeePii
        );
    }

    private void FillCommon(
        IPiiTarget target,
        byte[]? phoneEnc,
        byte[]? identityEnc,
        byte[]? addressEnc,
        DateOnly? dob,
        string? gender,
        bool canSeePii
    )
    {
        if (phoneEnc != null)
        {
            var phone = _encryptionService.Decrypt(phoneEnc);
            target.Phone = canSeePii ? phone : MaskHelper.MaskPhone(phone);
        }

        if (identityEnc != null)
        {
            var id = _encryptionService.Decrypt(identityEnc);
            target.IdentityCardNo = canSeePii ? id : MaskHelper.MaskGeneric(id);
        }

        if (addressEnc != null)
        {
            var address = _encryptionService.Decrypt(addressEnc);
            target.Address = canSeePii ? address : MaskHelper.MaskGeneric(address);
        }

        target.DateOfBirth ??= dob;
        target.Gender ??= gender;
    }
}
