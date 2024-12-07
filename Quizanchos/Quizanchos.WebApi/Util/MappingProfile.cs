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
            .ConstructUsing(src => new SingleGameSessionDto(src.Id, src.QuizCategory.Id, src.GameLevel, src.ApplicationUser.Id.ToString(), 
                src.CreationTime, src.CurrentCardIndex, src.Score, src.IsFinished, src.IsTerminatedByTime, src.CardsCount, src.SecondsPerCard,
                src.OptionCount));
    }
}
