using AutoMapper;
using GymManagementProject_Infrastructure.Models;

public class UserRequestProfile : Profile
{
    public UserRequestProfile()
    {
        CreateMap<UserResponseDto, User>()
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());
    }
}
