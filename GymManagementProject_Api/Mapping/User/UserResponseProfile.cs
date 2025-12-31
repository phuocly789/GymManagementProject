using AutoMapper;
using GymManagementProject_Infrastructure.Models;

public class UserResponseProfile : Profile
{
    public UserResponseProfile()
    {
        CreateMap<User, UserResponseDto>()
            .ForMember(d => d.Roles, o => o.MapFrom(s => s.Roles.Select(r => r.Name)));
    }
}
