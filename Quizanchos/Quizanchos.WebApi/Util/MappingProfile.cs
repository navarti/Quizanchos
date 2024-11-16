using AutoMapper;
using Quizanchos.Domain.Entities;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<BaseQuizEntityDto, QuizEntity>();
        CreateMap<QuizEntityDto, QuizEntity>().ReverseMap();
        
        CreateMap<BaseQuizCategoryDto, QuizCategory>();
        CreateMap<QuizCategoryDto, QuizCategory>().ReverseMap();

        CreateMap<SingleGameSession, SingleGameSessionDto>();
            //.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.ApplicationUser.Id.ToString()));
    }
}
