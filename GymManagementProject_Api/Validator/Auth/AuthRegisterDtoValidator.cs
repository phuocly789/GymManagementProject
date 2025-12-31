using FluentValidation;

public class AuthRegisterDtoValidator : AbstractValidator<AuthRegisterDto>
{
    public AuthRegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email không được để trống.")
            .EmailAddress()
            .WithMessage("Email không hợp lệ.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6)
            .WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");

        RuleFor(x => x.FullName).NotEmpty().WithMessage("Họ tên không được để trống.");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Số điện thoại không được để trống.")
            .Matches(@"^(0|\+84)[0-9]{9}$")
            .WithMessage("Số điện thoại không hợp lệ.");

        RuleFor(x => x.DateOfBirth)
            .NotNull()
            .WithMessage("Ngày sinh không được để trống.")
            .LessThan(DateTime.Today)
            .WithMessage("Ngày sinh không hợp lệ.");

        RuleFor(x => x.Gender).IsInEnum().WithMessage("Giới tính không hợp lệ.");

        RuleFor(x => x.HomeBranchId).NotEmpty().WithMessage("Chi nhánh không được để trống.");

        RuleFor(x => x.Address)
            .NotNull()
            .WithMessage("Địa chỉ không được để trống.")
            .SetValidator(new AddressDtoValidator());
    }
}

public class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(x => x.Detail).NotEmpty().WithMessage("Chi tiết không được để trống.");

        RuleFor(x => x.Ward).NotEmpty().WithMessage("Phường không được để trống.");

        RuleFor(x => x.District).NotEmpty().WithMessage("Quận/Huyện không được để trống.");

        RuleFor(x => x.Province).NotEmpty().WithMessage("Tỉnh/Thành phố không được để trống.");

        RuleFor(x => x.Full).NotEmpty().WithMessage("Địa chỉ đầy đủ không được để trống.");
    }
}
