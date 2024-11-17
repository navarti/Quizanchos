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

        CreateMap<SingleGameSession, SingleGameSessionDto>()
            .ConstructUsing(src => new SingleGameSessionDto(src.Id, src.QuizCategory.Id, src.ApplicationUser.Id.ToString(), 
                src.CreationTime, src.CurrentQuestionIndex, src.Score, src.IsFinished, src.QuestionsCount));
    }
}
