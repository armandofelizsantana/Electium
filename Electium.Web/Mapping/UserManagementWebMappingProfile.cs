using AutoMapper;
using Electium.Application.DTOs;
using Electium.Web.ViewModels;

namespace Electium.Web.Mapping;

public class UserManagementWebMappingProfile : Profile
{
    public UserManagementWebMappingProfile()
    {
        CreateMap<SystemUserDto, UserListViewModel>();

        CreateMap<SystemUserDto, EditUserViewModel>()
            .ForMember(
                destination => destination.Roles,
                options => options.Ignore())
            .ForMember(
                destination => destination.Password,
                options => options.Ignore())
            .ForMember(
                destination => destination.ConfirmPassword,
                options => options.Ignore());

        CreateMap<SystemUserDto, ConfirmUserStatusViewModel>();
        CreateMap<CreateUserViewModel, CreateSystemUserDto>();
        CreateMap<EditUserViewModel, UpdateSystemUserDto>();
    }
}