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
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Doctor!.Category.Name))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Doctor!.Name))
                .ForMember(dest => dest.About, opt => opt.MapFrom(src => src.Doctor!.About))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.User!.PhotoUrl))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User!.Email))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.User!.PhoneNumber))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Doctor!.Id));







            CreateMap<FavouriteCreateDto, Favourite>();

            CreateMap<WorkingTime, BookingDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient!.Name))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Patient!.User.PhoneNumber));


            CreateMap<WorkingTime, GetAllBookingResult>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.DoctorDetails.Doctor.Name));




            CreateMap<NotificationCreateDto, Notification>().ReverseMap();
            CreateMap<NotificationDto, Notification>().ReverseMap();

            CreateMap<WorkingTime, WorkingTimeDto>().ReverseMap();

            CreateMap<DoctorDetails, DoctorDetailsDto>()
                              .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.Name))
                              .ForMember(dest => dest.DoctorCategoryName, opt => opt.MapFrom(src => src.Doctor.Category.Name))
                              .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Doctor.User.PhotoUrl))
                              .ForMember(dest => dest.IsFave, opt => opt.MapFrom(src => src.Doctor.IsFav));








        }
    }
}




