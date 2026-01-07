using FluentValidation;

public class ConfirmResetPasswordDtoValidator : AbstractValidator<ConfirmResetPasswordDto>
{
    public ConfirmResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email không được để trống.")
            .EmailAddress()
            .WithMessage("Email không hợp lệ.");

        RuleFor(x => x.Otp)
            .NotEmpty()
            .WithMessage("Mã OTP không được để trống.")
            .Length(6)
            .WithMessage("Mã OTP phải đúng 6 chữ số.")
            .Matches(@"^\d{6}$")
            .WithMessage("Mã OTP chỉ được chứa chữ số.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6)
            .WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");
    }
}
