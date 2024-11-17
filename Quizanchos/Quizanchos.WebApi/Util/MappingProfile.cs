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
                src.CreationTime, src.CurrentCardIndex, src.Score, src.IsFinished, src.CardsCount));

        CreateMap<QuizCardFloat, QuizCardFloatDto>()
            .ConstructUsing(src => new QuizCardFloatDto(src.Id, src.CardIndex, src.Option1.Value.Value, src.Option2.Value.Value, src.OptionPicked));

        CreateMap<QuizCardInt, QuizCardIntDto>()
            .ConstructUsing(src => new QuizCardIntDto(src.Id, src.CardIndex, src.Option1.Value.Value, src.Option2.Value.Value, src.OptionPicked));
    }
}
