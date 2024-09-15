using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services.Interfaces;

public interface IQuizCategoryService
{
    Task<QuizCategoryDto> Create(BaseQuizCategoryDto baseQuizEntityDto);

    #region Get
    Task<QuizCategoryDto> GetById(Guid id);

    Task<List<QuizCategoryDto>> GetAll();
    #endregion

    Task<QuizCategoryDto> Update(QuizCategoryDto quizEntityDto);

    Task Delete(Guid id);
}
