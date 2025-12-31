using FluentValidation;

public class VerifyEmailDtoValidator : AbstractValidator<VerifyEmailDto>
{
    public VerifyEmailDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email không được để trống.")
            .EmailAddress()
            .WithMessage("Email không hợp lệ.");

        RuleFor(x => x.OTP)
            .NotEmpty()
            .WithMessage("Mã OTP không được để trống.")
            .Length(6)
            .WithMessage("Mã OTP phải đúng 6 chữ số.")
            .Matches(@"^\d{6}$")
            .WithMessage("Mã OTP chỉ được chứa chữ số.");
    }
}
