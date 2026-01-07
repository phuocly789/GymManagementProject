using FluentValidation;

public class RequestChangePasswordDtoValidator : AbstractValidator<RequestChangePasswordDto>
{
    public RequestChangePasswordDtoValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty()
            .WithMessage("Mật khẩu cũ không được để trống.")
            .MinimumLength(6)
            .WithMessage("Mật khẩu cũ phải có ít nhất 6 ký tự.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("Mật khẩu mới không được để trống.")
            .MinimumLength(6)
            .WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Xác nhận mật khẩu không được để trống.")
            .Equal(x => x.NewPassword)
            .WithMessage("Mật khẩu và xác nhận mật khẩu không khớp.");
    }
}
