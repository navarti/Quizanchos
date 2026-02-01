using AutoMapper;
using Quizanchos.Quiz.Dto;
using Quizanchos.Quiz.Entities;

namespace Quizanchos.Quiz.Util;

public class QuizMappingProfile : Profile
{
    public QuizMappingProfile()
    {
        CreateMap<BaseQuizEntityDto, QuizEntity>();
        CreateMap<QuizEntityDto, QuizEntity>().ReverseMap();
        
        CreateMap<BaseQuizCategoryDto, QuizCategory>();
        CreateMap<QuizCategoryDto, QuizCategory>().ReverseMap();
    }
}
