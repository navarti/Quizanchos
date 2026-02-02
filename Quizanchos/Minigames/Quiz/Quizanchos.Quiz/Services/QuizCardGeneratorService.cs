using Microsoft.Extensions.Logging;
using Quizanchos.Common.Enums;
using Quizanchos.Domain.Entities.Quiz;
using Quizanchos.Domain.Repositories.Quiz.Interfaces;
using Quizanchos.Quiz.GameLogic;

namespace Quizanchos.Quiz.Services;

public class QuizCardGeneratorService
{
    private readonly ILogger<QuizCardGeneratorService> _logger;
    private readonly IQuizCategoryRepository _categoryRepository;
    private readonly IQuizEntityRepository _entityRepository;
    private readonly IFeatureIntRepository _featureIntRepository;
    private readonly IFeatureFloatRepository _featureFloatRepository;

    public QuizCardGeneratorService(
        ILogger<QuizCardGeneratorService> logger,
        IQuizCategoryRepository categoryRepository,
        IQuizEntityRepository entityRepository,
        IFeatureIntRepository featureIntRepository,
        IFeatureFloatRepository featureFloatRepository)
    {
        _logger = logger;
        _categoryRepository = categoryRepository;
        _entityRepository = entityRepository;
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

        // Get the category
        QuizCategory category = await _categoryRepository.GetById(categoryId);
        
        // Get all entities for this category  
        List<QuizEntity> allEntities = (await _entityRepository.GetByFilter(e => true)).ToList();
        
        if (allEntities.Count < optionCount)
        {
            throw new InvalidOperationException($"Not enough entities ({allEntities.Count}) to create cards with {optionCount} options");
        }

        Random random = new Random();

        // Randomly select entities for this card
        List<QuizEntity> selectedEntities = allEntities
            .OrderBy(x => random.Next())
            .Take(optionCount)
            .ToList();

        Guid[] entityIds = selectedEntities.Select(e => e.Id).ToArray();
        string[] entityNames = selectedEntities.Select(e => e.Name).ToArray();
        
        // Get feature values and determine correct answer
        object[] optionValues;
        int correctOption;

        if (category.FeatureType == FeatureType.Int)
        {
            int[] values = new int[optionCount];
            
            for (int j = 0; j < optionCount; j++)
            {
                FeatureInt? feature = await _featureIntRepository
                    .FindFirstOrDefaultAsync(f => f.QuizEntity.Id == entityIds[j] && f.QuizCategory.Id == categoryId);
                
                values[j] = feature?.Value.Value ?? 0;
            }

            optionValues = values.Cast<object>().ToArray();
            
            // Find the index with the maximum value (correct answer)
            correctOption = Array.IndexOf(values, values.Max());
        }
        else if (category.FeatureType == FeatureType.Float)
        {
            float[] values = new float[optionCount];
            
            for (int j = 0; j < optionCount; j++)
            {
                FeatureFloat? feature = await _featureFloatRepository
                    .FindFirstOrDefaultAsync(f => f.QuizEntity.Id == entityIds[j] && f.QuizCategory.Id == categoryId);
                
                values[j] = feature?.Value.Value ?? 0f;
            }

            optionValues = values.Cast<object>().ToArray();
            
            // Find the index with the maximum value (correct answer)
            correctOption = Array.IndexOf(values, values.Max());
        }
        else
        {
            throw new InvalidOperationException($"Unknown feature type: {category.FeatureType}");
        }

        // Add card to state
        state.Cards.Add(new QuizGameState.QuizCard
        {
            Id = Guid.NewGuid(),
            CardIndex = cardIndex,
            CorrectOption = correctOption,
            EntityIds = entityIds,
            EntityNames = entityNames,
            OptionValues = optionValues,
            PlayerAnswers = new Dictionary<Guid, int?>(),
            CreationTime = DateTime.UtcNow
        });
        
        _logger.LogInformation("Generated card {CardIndex}: Entities={Entities}, CorrectOption={CorrectOption}",
            cardIndex, string.Join(",", entityNames), correctOption);
    }
}
