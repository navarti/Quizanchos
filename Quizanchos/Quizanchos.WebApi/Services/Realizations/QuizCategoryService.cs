using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;
using Quizanchos.WebApi.Dto;
using Quizanchos.WebApi.Services.Interfaces;
using Quizanchos.WebApi.Util;


namespace Quizanchos.WebApi.Services.Realizations;

public class QuizCategoryService : IQuizCategoryService
{
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly IMapper _mapper;

    public QuizCategoryService(IQuizCategoryRepository quizCategoryRepository, IMapper mapper)
    {
        _quizCategoryRepository = quizCategoryRepository;
        _mapper = mapper;
    }

    public async Task<QuizCategoryDto> Create(BaseQuizCategoryDto BaseQuizCategoryDto)
    {
        _ = BaseQuizCategoryDto ?? throw ExceptionFactory.CreateNullException(nameof(BaseQuizCategoryDto));

        QuizCategory quizCategory = _mapper.Map<QuizCategory>(BaseQuizCategoryDto);

        quizCategory = await _quizCategoryRepository.Create(quizCategory).ConfigureAwait(false);

        return _mapper.Map<QuizCategoryDto>(quizCategory);
    }

    public async Task<QuizCategoryDto> GetById(Guid id)
    {
        QuizCategory quizCategory = await _quizCategoryRepository.GetById(id).ConfigureAwait(false)
            ?? throw ExceptionFactory.CreateIdNotFoundException(id, nameof(quizCategory));

        return _mapper.Map<QuizCategoryDto>(quizCategory);
    }

    public async Task<List<QuizCategoryDto>> GetAll()
    {
        List<QuizCategory> quizCategories = await _quizCategoryRepository.Get().ToListAsync().ConfigureAwait(false);

        return _mapper.Map<List<QuizCategoryDto>>(quizCategories);
    }

    public async Task<QuizCategoryDto> Update(QuizCategoryDto QuizCategoryDto)
    {
        _ = QuizCategoryDto ?? throw ExceptionFactory.CreateNullException(nameof(QuizCategoryDto));

        QuizCategory quizCategory = await _quizCategoryRepository.GetById(QuizCategoryDto.Id).ConfigureAwait(false) ??
            throw ExceptionFactory.CreateIdNotFoundException(QuizCategoryDto.Id, nameof(quizCategory));

        _mapper.Map(QuizCategoryDto, quizCategory);

        await _quizCategoryRepository.Update(quizCategory);

        return QuizCategoryDto;
    }

    public async Task Delete(Guid id)
    {
        QuizCategory quizCategory = await _quizCategoryRepository.GetById(id).ConfigureAwait(false)
            ?? throw ExceptionFactory.CreateIdNotFoundException(id, nameof(quizCategory));

        await _quizCategoryRepository.Delete(quizCategory);
    }
}




