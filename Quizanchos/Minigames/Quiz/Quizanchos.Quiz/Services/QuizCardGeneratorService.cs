using Microsoft.Extensions.Logging;
using Quizanchos.Common.Enums;
using Quizanchos.Domain.Quiz.Enums;
using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Repositories.Quiz.Interfaces;
using Quizanchos.Quiz.GameLogic;

namespace Quizanchos.Quiz.Services;

public class QuizCardGeneratorService
{
    private readonly ILogger<QuizCardGeneratorService> _logger;
    private readonly IQuizCategoryRepository _categoryRepository;
    private readonly IFeatureIntRepository _featureIntRepository;
    private readonly IFeatureFloatRepository _featureFloatRepository;

    public QuizCardGeneratorService(
        ILogger<QuizCardGeneratorService> logger,
        IQuizCategoryRepository categoryRepository,
        IFeatureIntRepository featureIntRepository,
        IFeatureFloatRepository featureFloatRepository)
    {
        _logger = logger;
        _categoryRepository = categoryRepository;
        _featureIntRepository = featureIntRepository;
        _featureFloatRepository = featureFloatRepository;
    }

    public async Task GenerateCardsForGame(
        QuizGameState state,
        Guid categoryId,
        int totalCards,
        int optionCount,
        GameLevel gameLevel)
    {
        _logger.LogInformation("Generating all {TotalCards} cards for category {CategoryId}", totalCards, categoryId);

        // Generate all cards upfront
        for (int i = 0; i < totalCards; i++)
        {
            await GenerateSingleCard(state, categoryId, optionCount, gameLevel);
            _logger.LogInformation("Generated card {CardIndex}/{TotalCards}", i + 1, totalCards);
        }
        
        // Set CurrentCardIndex to 0 to start at the first card
        state.CurrentCardIndex = 0;
        
        _logger.LogInformation("All cards generated. Cards.Count={CardsCount}, CurrentCardIndex={CurrentCardIndex}", 
            state.Cards.Count, state.CurrentCardIndex);
    }

    public async Task GenerateSingleCard(
        QuizGameState state,
        Guid categoryId,
        int optionCount,
        GameLevel gameLevel)
    {
        int cardIndex = state.Cards.Count;

        _logger.LogInformation("Generating card {CardIndex} for category {CategoryId}", cardIndex, categoryId);

        QuizCategory category = await _categoryRepository.GetById(categoryId);

        Random random = new Random();

        Guid[] entityIds;
        string[] entityNames;
        object[] optionValues;
        int correctOption;

        if (category.FeatureType == FeatureType.Int)
        {
            List<FeatureInt> categoryFeatures = await _featureIntRepository.GetByCategoryId(categoryId);

            if (categoryFeatures.Count < optionCount)
            {
                throw new InvalidOperationException($"Not enough entities ({categoryFeatures.Count}) in category {category.Name} to create cards with {optionCount} options");
            }

            List<FeatureInt> selected = categoryFeatures
                .OrderBy(_ => random.Next())
                .Take(optionCount)
                .ToList();

            entityIds = selected.Select(f => f.QuizEntity.Id).ToArray();
            entityNames = selected.Select(f => f.QuizEntity.Name).ToArray();
            int[] values = selected.Select(f => f.Value.Value).ToArray();
            optionValues = values.Cast<object>().ToArray();
            correctOption = Array.IndexOf(values, values.Max());
        }
        else if (category.FeatureType == FeatureType.Float)
        {
            List<FeatureFloat> categoryFeatures = await _featureFloatRepository.GetByCategoryId(categoryId);

            if (categoryFeatures.Count < optionCount)
            {
                throw new InvalidOperationException($"Not enough entities ({categoryFeatures.Count}) in category {category.Name} to create cards with {optionCount} options");
            }

            List<FeatureFloat> selected = categoryFeatures
                .OrderBy(_ => random.Next())
                .Take(optionCount)
                .ToList();

            entityIds = selected.Select(f => f.QuizEntity.Id).ToArray();
            entityNames = selected.Select(f => f.QuizEntity.Name).ToArray();
            float[] values = selected.Select(f => f.Value.Value).ToArray();
            optionValues = values.Cast<object>().ToArray();
            correctOption = Array.IndexOf(values, values.Max());
        }
        else
        {
            throw new InvalidOperationException($"Unknown feature type: {category.FeatureType}");
        }

        state.Cards.Add(new QuizGameState.QuizCard
        {
            Id = Guid.NewGuid(),
            CardIndex = cardIndex,
            CorrectOption = correctOption,
            EntityIds = entityIds,
            EntityNames = entityNames,
            OptionValues = optionValues,
            PlayerAnswers = new Dictionary<string, int?>(),
            CreationTime = DateTime.UtcNow
        });

        _logger.LogInformation("Generated card {CardIndex}: Entities={Entities}, CorrectOption={CorrectOption}",
            cardIndex, string.Join(",", entityNames), correctOption);
    }
}
