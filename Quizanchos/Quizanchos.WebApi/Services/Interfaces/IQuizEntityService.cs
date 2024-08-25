using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IQuizEntityService
{
    Task<QuizEntityDto> Create(BaseQuizEntityDto baseQuizEntityDto);

    Task<QuizEntityDto> GetById(Guid id);
    Task<List<QuizEntityDto>> GetAll();

    Task<QuizEntityDto> Update(QuizEntityDto quizEntityDto);

    Task Delete(Guid id);
}
