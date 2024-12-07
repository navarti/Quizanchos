using Quizanchos.Common.FeatureTypes;
using Quizanchos.Common.Util;
using Quizanchos.Domain.Entities;
using Quizanchos.Domain.Repositories.Interfaces;
using Quizanchos.Domain.Repositories.Realizations;
using Quizanchos.WebApi.Dto;

namespace Quizanchos.WebApi.Services;

public class FeatureFloatService
{
    private readonly IFeatureFloatRepository _featureFloatRepository;
    private readonly IQuizCategoryRepository _quizCategoryRepository;
    private readonly IQuizEntityRepository _quizEntityRepository;

    public FeatureFloatService(IFeatureFloatRepository featureFloatRepository, IQuizCategoryRepository quizCategoryRepository, IQuizEntityRepository quizEntityRepository)
    {
        _featureFloatRepository = featureFloatRepository;
        _quizCategoryRepository = quizCategoryRepository;
        _quizEntityRepository = quizEntityRepository;
    }

    public async Task<FeatureFloatDto> GetById(Guid id)
    {
        FeatureFloat feature = await _featureFloatRepository.GetByIdIncluding(id);
        return MapFeature(feature);
    }

    public async Task<List<FeatureFloatDto>> GetAllByCategory(Guid categoryId)
    {
        List<FeatureFloat> features = await _featureFloatRepository.GetByCategoryId(categoryId);
        return MapFeatures(features);
    }

    public async Task<FeatureFloatDto> Create(BaseFeatureFloatDto baseFeatureFloatDto)
    {
        _ = baseFeatureFloatDto ?? throw HandledExceptionFactory.CreateNullException(nameof(baseFeatureFloatDto));

        QuizCategory quizCategory = await _quizCategoryRepository.GetById(baseFeatureFloatDto.QuizCategoryId);
        QuizEntity quizEntity = await _quizEntityRepository.GetById(baseFeatureFloatDto.QuizEntityId);
        FeatureValueFloat featureValue = new FeatureValueFloat(baseFeatureFloatDto.Value);

        FeatureFloat feature = new FeatureFloat()
        {
            QuizCategory = quizCategory,
            QuizEntity = quizEntity,
            Value = featureValue
        };
        feature = await _featureFloatRepository.Create(feature);

        return MapFeature(feature);
    }

    public async Task Delete(Guid id)
    {
        FeatureFloat feature = await _featureFloatRepository.GetByIdIncluding(id);
        await _featureFloatRepository.Delete(feature);
    }

    public async Task<FeatureFloatDto> Update(FeatureFloatDto featurefloatDto)
    {
        _ = featurefloatDto ?? throw HandledExceptionFactory.CreateNullException(nameof(featurefloatDto));

        QuizCategory quizCategory = await _quizCategoryRepository.GetById(featurefloatDto.QuizCategoryId);
        QuizEntity quizEntity = await _quizEntityRepository.GetById(featurefloatDto.QuizEntityId);
        FeatureValueFloat featureValue = new FeatureValueFloat(featurefloatDto.Value);

        FeatureFloat feature = await _featureFloatRepository.GetById(featurefloatDto.Id);

        feature.QuizCategory = quizCategory;
        feature.QuizEntity = quizEntity;
        feature.Value = featureValue;

        feature = await _featureFloatRepository.Update(feature);

        return MapFeature(feature);
    }

    private FeatureFloatDto MapFeature(FeatureFloat feature)
    {
        return new FeatureFloatDto(feature.Id, feature.Value.Value, feature.QuizCategory.Id, feature.QuizEntity.Id);
    }

    private List<FeatureFloatDto> MapFeatures(List<FeatureFloat> features)
    {
        return features.Select(MapFeature).ToList();
    }
}
