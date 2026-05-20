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
            .ForMember(d => d.WorkingHours, o => o.MapFrom(s => s.WorkingHours))
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
            .ForMember(d => d.WorkingHours, o => o.MapFrom(s => s.WorkingHours))
            .ForMember(d => d.ExpiryDate, o => o.MapFrom(s => s.ExpiryDate));

        CreateMap<PostingPackage, PostingPackageDto>();
        CreateMap<PostingPackageDto, PostingPackage>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Admin, o => o.Ignore())
            .ForMember(d => d.PaymentHistories, o => o.Ignore());

        CreateMap<Process, ProcessDto>();
        CreateMap<ProcessDto, Process>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Application, o => o.Ignore());

        CreateMap<Application, ApplicationDto>();
        CreateMap<ApplicationDto, Application>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.JobSeeker, o => o.Ignore())
            .ForMember(d => d.Job, o => o.Ignore())
            .ForMember(d => d.Resume, o => o.Ignore())
            .ForMember(d => d.Processes, o => o.Ignore());

        CreateMap<Resume, ResumeDto>();
        CreateMap<ResumeDto, Resume>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.JobSeeker, o => o.Ignore())
            .ForMember(d => d.Applications, o => o.Ignore());

        CreateMap<PaymentHistory, PaymentHistoryDto>();
        CreateMap<PaymentHistoryDto, PaymentHistory>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Employer, o => o.Ignore())
            .ForMember(d => d.PostingPackage, o => o.Ignore());

        CreateMap<Category, CategoryDto>();
        CreateMap<CategoryDto, Category>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Jobs, o => o.Ignore());

        CreateMap<Image, ImageDto>();
        CreateMap<ImageDto, Image>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Job, o => o.Ignore());

        CreateMap<Review, ReviewDto>();
        CreateMap<ReviewDto, Review>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Job, o => o.Ignore())
            .ForMember(d => d.Employer, o => o.Ignore())
            .ForMember(d => d.JobSeeker, o => o.Ignore());

        CreateMap<Admin, AdminDto>();
        CreateMap<Employer, EmployerDto>();
        CreateMap<JobSeeker, JobSeekerDto>();
    }
}
