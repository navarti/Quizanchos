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
    }
}
