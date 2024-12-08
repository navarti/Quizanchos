using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services;

public class QuizEntityService
{
    private readonly IQuizEntityRepository _quizEntityRepository;
    private readonly IMapper _mapper;

    public QuizEntityService(IQuizEntityRepository quizEntityRepository, IMapper mapper)
    {
        _quizEntityRepository = quizEntityRepository;
        _mapper = mapper;
    }

    public async Task<QuizEntityDto> Create(BaseQuizEntityDto baseQuizEntityDto)
    {
        _ = baseQuizEntityDto ?? throw HandledExceptionFactory.CreateNullException(nameof(baseQuizEntityDto));

        QuizEntity quizEntity = _mapper.Map<QuizEntity>(baseQuizEntityDto);
        quizEntity = await _quizEntityRepository.Create(quizEntity).ConfigureAwait(false);

        return _mapper.Map<QuizEntityDto>(quizEntity);
    }

    public async Task<QuizEntityDto> GetById(Guid id)
    {
        QuizEntity quizEntity = await _quizEntityRepository.GetById(id).ConfigureAwait(false);
        return _mapper.Map<QuizEntityDto>(quizEntity);
    }

    public async Task<List<QuizEntityDto>> GetAll(string name, int take, int skip)
    {
        List<QuizEntity> quizEntities = await _quizEntityRepository
            .Get(skip: skip, take: take, e => e.Name.StartsWith(name))
            .ToListAsync()
            .ConfigureAwait(false);
        return _mapper.Map<List<QuizEntityDto>>(quizEntities);
    }

    public async Task<QuizEntityDto> Update(QuizEntityDto quizEntityDto)
    {
        _ = quizEntityDto ?? throw HandledExceptionFactory.CreateNullException(nameof(quizEntityDto));

        QuizEntity quizEntity = await _quizEntityRepository.GetById(quizEntityDto.Id).ConfigureAwait(false);
        
        _mapper.Map(quizEntityDto, quizEntity);

        await _quizEntityRepository.Update(quizEntity);
        return quizEntityDto;
    }

    public async Task Delete(Guid id)
    {
        QuizEntity quizEntity = await _quizEntityRepository.GetById(id).ConfigureAwait(false);
        await _quizEntityRepository.Delete(quizEntity);
    }
}
