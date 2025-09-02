using AutoMapper;
using webapplication.Models.DTOs;
using webapplication.Models.Entities;

namespace webapplication.Utilities
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ApplicationUser, UserDto>();
        }
    }
}
