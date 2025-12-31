// using System.ComponentModel.DataAnnotations;

// public class RefreshTokenRequestDto
// {
//     [Required]
//     public string RefreshToken { get; set; } = null!;
// }
using FluentValidation;

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token không được để trống.");
    }
}
