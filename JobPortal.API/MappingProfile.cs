using AutoMapper;
using JobPortal.API.DTOs;
using JobPortal.API.Models;

namespace JobPortal.API;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Job, JobDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.EmployerId, o => o.MapFrom(s => s.EmployerId))
            .ForMember(d => d.CategoryId, o => o.MapFrom(s => s.CategoryId))
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
            .ForMember(d => d.Salary, o => o.MapFrom(s => s.Salary))
            .ForMember(d => d.Location, o => o.MapFrom(s => s.Location))
            .ForMember(d => d.PostingStatus, o => o.MapFrom(s => s.PostingStatus))
            .ForMember(d => d.StartDate, o => o.MapFrom(s => s.StartDate))
            .ForMember(d => d.EndDate, o => o.MapFrom(s => s.EndDate))
            .ForMember(d => d.ExpiryDate, o => o.MapFrom(s => s.ExpiryDate));

        CreateMap<JobDto, Job>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Employer, o => o.Ignore())
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Images, o => o.Ignore())
            .ForMember(d => d.Applications, o => o.Ignore())
            .ForMember(d => d.Reviews, o => o.Ignore())
            .ForMember(d => d.EmployerId, o => o.MapFrom(s => s.EmployerId))
            .ForMember(d => d.CategoryId, o => o.MapFrom(s => s.CategoryId))
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
            .ForMember(d => d.Salary, o => o.MapFrom(s => s.Salary))
            .ForMember(d => d.Location, o => o.MapFrom(s => s.Location))
            .ForMember(d => d.PostingStatus, o => o.MapFrom(s => s.PostingStatus))
            .ForMember(d => d.StartDate, o => o.MapFrom(s => s.StartDate))
            .ForMember(d => d.EndDate, o => o.MapFrom(s => s.EndDate))
            .ForMember(d => d.ExpiryDate, o => o.MapFrom(s => s.ExpiryDate));
    }
}
