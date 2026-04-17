using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Models;

namespace JobPortal.API;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Job, JobDto>().ReverseMap();
        CreateMap<JobApplication, ApplicationDto>()
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job.Title))
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName));
    }
}
