using AutoMapper;
using Employee.Application.Common.Models;
using Employee.Domain.Entities;

namespace Employee.Application.Employees.Mappings
{
    public class EmployeeMappingProfile : Profile
    {
        public EmployeeMappingProfile()
        {
            CreateMap<EmployeeEntity, EmployeeDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

            CreateMap<EmployeeDto, EmployeeEntity>();
        }
    }
}
