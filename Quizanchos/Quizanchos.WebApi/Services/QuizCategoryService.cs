using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services;

public class QuizCategoryService
{
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly IMapper _mapper;

    public QuizCategoryService(IQuizCategoryRepository quizCategoryRepository, IMapper mapper)
    {
        _quizCategoryRepository = quizCategoryRepository;
        _mapper = mapper;
    }

    public async Task<QuizCategoryDto> Create(BaseQuizCategoryDto baseQuizCategoryDto)
    {
        _ = baseQuizCategoryDto ?? throw HandledExceptionFactory.CreateNullException(nameof(baseQuizCategoryDto));

        QuizCategory quizCategory = _mapper.Map<QuizCategory>(baseQuizCategoryDto);
        quizCategory = await _quizCategoryRepository.Create(quizCategory).ConfigureAwait(false);

        return _mapper.Map<QuizCategoryDto>(quizCategory);
    }

    public async Task<QuizCategoryDto> GetById(Guid id)
    {
        QuizCategory quizCategory = await _quizCategoryRepository.GetById(id).ConfigureAwait(false);
        return _mapper.Map<QuizCategoryDto>(quizCategory);
    }

    public async Task<List<QuizCategoryDto>> GetAll()
    {
        List<QuizCategory> quizCategories = await _quizCategoryRepository.Get().ToListAsync().ConfigureAwait(false);
        return _mapper.Map<List<QuizCategoryDto>>(quizCategories);
    }

    public async Task<QuizCategoryDto> Update(QuizCategoryDto quizCategoryDto)
    {
        _ = quizCategoryDto ?? throw HandledExceptionFactory.CreateNullException(nameof(quizCategoryDto));

        QuizCategory quizCategory = await _quizCategoryRepository.GetById(quizCategoryDto.Id).ConfigureAwait(false);
        
        _mapper.Map(quizCategoryDto, quizCategory);

        await _quizCategoryRepository.Update(quizCategory);
        return quizCategoryDto;
    }

    public async Task Delete(Guid id)
    {
        QuizCategory quizCategory = await _quizCategoryRepository.GetById(id).ConfigureAwait(false);
        await _quizCategoryRepository.Delete(quizCategory);
    }
}
