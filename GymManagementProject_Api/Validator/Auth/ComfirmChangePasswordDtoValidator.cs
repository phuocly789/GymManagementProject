using FluentValidation;
public class ConfirmChangePasswordDtoValidator : AbstractValidator<ConfirmChangePasswordDto>
{
    public ConfirmChangePasswordDtoValidator()
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

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6)
            .WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Xác nhận mật khẩu không được để trống.")
            .Equal(x => x.NewPassword)
            .WithMessage("Mật khẩu và xác nhận mật khẩu không khớp.");
    }
}
