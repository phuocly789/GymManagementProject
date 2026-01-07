using AutoMapper;
using GymManagementProject_Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IUserService
{
    //Get My Profile
    //Update My Profile
    //Get User List -> users:view
    Task<PagedResult<UserResponseDto>> GetAllUsersAsync();

    //Get User Detail -> users:view
    Task<UserDetailDto> GetUserByIdAsync(Guid userId, Guid id);

    //Create User -> users:manage
    //Update User -> users:manage
    //Toggle Status -> users:manage
    //Soft Delete -> users:manage
    //Assign Role -> roles:manage
    // Assign Branch Access -> branches:manage
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User entity);
    Task UpdateAsync(User entity);
    Task DeleteAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEncryptionService _encryptionService;
    private readonly IProfilePiiReader _profilePiiReader;

    public UserService(
        IUserRepository userRepository,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IEncryptionService encryptionService,
        IProfilePiiReader profilePiiReader
    )
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _encryptionService = encryptionService;
        _profilePiiReader = profilePiiReader;
    }

    public async Task<PagedResult<UserResponseDto>> GetAllUsersAsync()
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        var tenantIdClaim = currentUser?.FindFirst("tenantId")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Invalid TenantId");
        }

        var users = await _userRepository
            .Query()
            .AsNoTracking()
            .Where(u => u.TenantId == tenantId)
            .Include(u => u.Roles)
            .ToListAsync();

        var dtos = users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Email = u.Email,
            FullName = u.FullName,
            IsActive = u.IsActive,
            EmailVerified = u.EmailVerified,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
            CreatedBy = u.CreatedBy,
            UpdatedBy = u.UpdatedBy,
            DeletedAt = u.DeletedAt,
            Version = u.Version,
            Roles = u.Roles.Select(r => r.Code).ToList(),
        });

        return new PagedResult<UserResponseDto>
        {
            TotalItems = 1,
            Page = 1,
            PageSize = 1,
            Items = dtos.ToList(),
        };
    }

    public async Task<UserDetailDto> GetUserByIdAsync(Guid userId, Guid id)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        var tenantIdClaim = currentUser?.FindFirst("tenantId")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            throw new UnauthorizedAccessException("Invalid TenantId");
        }

        //check người xem có quyền xem thông tin bảo mật kh
        bool canSeePii = currentUser?.HasClaim("Permission", "profiles:view_pii") ?? false;

        //truy vấn đầy đủ

        var user = await _userRepository
            .Query()
            .AsNoTracking()
            .AsSplitQuery()
            .Where(u => u.Id == id && u.TenantId == tenantId)
            .Include(u => u.UserProfile)
            .Include(u => u.Member)
            .ThenInclude(m => m.MemberProfile)
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .Include(u => u.UserBranchAccesses)
            .ThenInclude(uba => uba.Branch)
            .FirstOrDefaultAsync();

        if (user == null)
            return null;

        var dto = new UserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            Gender = user.UserProfile?.Gender,
            DateOfBirth = user.UserProfile?.DateOfBirth,
        };

        if (user.Member?.MemberProfile != null)
        {
            _profilePiiReader.FillFromMemberProfile(dto, user.Member.MemberProfile, canSeePii);
        }
        else if (user.UserProfile != null)
        {
            _profilePiiReader.FillFromUserProfile(dto, user.UserProfile, canSeePii);
        }

        //gom role và permission
        var perms = new HashSet<string>();
        foreach (var ur in user.Roles)
        {
            dto.Roles.Add(ur.Code);
            if (ur.Permissions != null)
            {
                foreach (var rp in ur.Permissions)
                {
                    perms.Add(rp.Code);
                }
            }
        }
        dto.Permissions = perms.ToList();

        dto.AccessibleBranches = user.UserBranchAccesses.Select(uba => uba.Branch.Name).ToList();

        return dto;
    }

    public async Task<User?> GetByIdAsync(Guid id) => await _userRepository.GetByIdAsync(id);

    public async Task AddAsync(User entity) => await _userRepository.AddAsync(entity);

    public async Task UpdateAsync(User entity) => _userRepository.Update(entity);

    public async Task DeleteAsync(Guid id)
    {
        await _userRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }
}
