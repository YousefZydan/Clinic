using Application.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Doctor, DoctorDto>()
              .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
              .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User.PhoneNumber))
              .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
              .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.User.PhotoUrl));
            


            CreateMap<User, userDto>().ReverseMap();

            CreateMap<Favourite, FavouriteDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Doctor!.Category.Name));

            CreateMap<FavouriteCreateDto, Favourite>();

            CreateMap<NotificationCreateDto, Notification>().ReverseMap();
            CreateMap<NotificationDto, Notification>().ReverseMap();

            CreateMap<WorkingTime, WorkingTimeDto>().ReverseMap();

            CreateMap<DoctorDetails, DoctorDetailsDto>()
                              .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.Name))
                              .ForMember(dest => dest.DoctorCategoryName, opt => opt.MapFrom(src => src.Doctor.Category.Name))
                              .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Doctor.User.PhotoUrl));







        }
    }
}




