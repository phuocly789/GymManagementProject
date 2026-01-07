using FluentValidation;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email không được để trống.")
            .EmailAddress()
            .WithMessage("Email không hợp lệ.");
    }
}
