using Quizanchos.Common.FeatureTypes;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services;

public class FeatureIntService
{
    private readonly IFeatureIntRepository _featureIntRepository;
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly IQuizEntityRepository _quizEntityRepository;

    public FeatureIntService(IFeatureIntRepository featureIntRepository, IQuizCategoryRepository quizCategoryRepository, IQuizEntityRepository quizEntityRepository)
    {
        _featureIntRepository = featureIntRepository;
        _quizCategoryRepository = quizCategoryRepository;
        _quizEntityRepository = quizEntityRepository;
    }

    public async Task<FeatureIntDto> GetById(Guid id)
    {
        FeatureInt feature = await _featureIntRepository.GetByIdIncluding(id);
        return MapFeature(feature);
    }

    public async Task<List<FeatureIntDto>> GetAllByCategory(Guid categoryId)
    {
        List<FeatureInt> features = await _featureIntRepository.GetByCategoryId(categoryId);
        return MapFeatures(features);
    }

    public async Task<FeatureIntDto> Create(BaseFeatureIntDto baseFeatureIntDto)
    {
        _ = baseFeatureIntDto ?? throw HandledExceptionFactory.CreateNullException(nameof(baseFeatureIntDto));

        QuizCategory quizCategory = await _quizCategoryRepository.GetById(baseFeatureIntDto.QuizCategoryId);
        QuizEntity quizEntity = await _quizEntityRepository.GetById(baseFeatureIntDto.QuizEntityId);
        FeatureValueInt featureValueInt = new FeatureValueInt(baseFeatureIntDto.Value);

        FeatureInt feature = new FeatureInt()
        {
            QuizCategory = quizCategory,
            QuizEntity = quizEntity,
            Value = featureValueInt
        };
        feature = await _featureIntRepository.Create(feature);

        return MapFeature(feature);
    }

    public async Task Delete(Guid id)
    {
        FeatureInt feature = await _featureIntRepository.GetByIdIncluding(id);
        await _featureIntRepository.Delete(feature);
    }

    public async Task<FeatureIntDto> Update(FeatureIntDto featureIntDto)
    {
        _ = featureIntDto ?? throw HandledExceptionFactory.CreateNullException(nameof(featureIntDto));

        QuizCategory quizCategory = await _quizCategoryRepository.GetById(featureIntDto.QuizCategoryId);
        QuizEntity quizEntity = await _quizEntityRepository.GetById(featureIntDto.QuizEntityId);
        FeatureValueInt featureValueInt = new FeatureValueInt(featureIntDto.Value);

        FeatureInt feature = await _featureIntRepository.GetById(featureIntDto.Id);
        
        feature.QuizCategory = quizCategory;
        feature.QuizEntity = quizEntity;
        feature.Value = featureValueInt;

        feature = await _featureIntRepository.Update(feature);
        
        return MapFeature(feature);
    }

    private FeatureIntDto MapFeature(FeatureInt feature)
    {
        return new FeatureIntDto(feature.Id, feature.Value.Value, feature.QuizCategory.Id, feature.QuizEntity.Id);
    }

    private List<FeatureIntDto> MapFeatures(List<FeatureInt> features)
    {
        return features.Select(MapFeature).ToList();
    }
}
