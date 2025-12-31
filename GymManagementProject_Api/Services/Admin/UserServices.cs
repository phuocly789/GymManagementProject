using AutoMapper;
using GymManagementProject_Infrastructure.Models;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task DeleteAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<UserResponseDto>>(users);
    }

    public async Task<User?> GetByIdAsync(Guid id) => await _userRepository.GetByIdAsync(id);

    public async Task AddAsync(User entity) => await _userRepository.AddAsync(entity);

    public async Task UpdateAsync(User entity) => _userRepository.Update(entity);

    public async Task DeleteAsync(Guid id) => await _userRepository.DeleteAsync(id);
}
