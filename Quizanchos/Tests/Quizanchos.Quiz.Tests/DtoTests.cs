using Quizanchos.Domain.Quiz.Enums;
using Quizanchos.Quiz.Dto;
using Xunit;

namespace Quizanchos.Quiz.Tests;

public class DtoTests
{
    [Fact]
    public void AnswerDto_EqualityByValue()
    {
        var id = Guid.NewGuid();
        var a = new AnswerDto(id, 2);
        var b = new AnswerDto(id, 2);

        Assert.Equal(a, b);
    }

    [Fact]
    public void AnswerDto_DifferentOption_AreNotEqual()
    {
        var id = Guid.NewGuid();
        var a = new AnswerDto(id, 0);
        var b = new AnswerDto(id, 1);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void QuizEntityDto_InheritsBase_AndCarriesIdAndName()
    {
        var id = Guid.NewGuid();
        var dto = new QuizEntityDto(id, "Lion");

        Assert.IsAssignableFrom<BaseQuizEntityDto>(dto);
        Assert.Equal(id, dto.Id);
        Assert.Equal("Lion", dto.Name);
    }

    [Fact]
    public void FeatureIntDto_AllFieldsAccessible()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var dto = new FeatureIntDto(id, 100, categoryId, entityId);

        Assert.Equal(id, dto.Id);
        Assert.Equal(100, dto.Value);
        Assert.Equal(categoryId, dto.QuizCategoryId);
        Assert.Equal(entityId, dto.QuizEntityId);
    }

    [Fact]
    public void FeatureFloatDto_AllFieldsAccessible()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var dto = new FeatureFloatDto(id, 3.14f, categoryId, entityId);

        Assert.Equal(id, dto.Id);
        Assert.Equal(3.14f, dto.Value);
        Assert.Equal(categoryId, dto.QuizCategoryId);
        Assert.Equal(entityId, dto.QuizEntityId);
    }

    [Fact]
    public void BaseQuizCategoryDto_ConstructsCorrectly()
    {
        var creationDate = new DateTime(2026, 5, 17, 0, 0, 0, DateTimeKind.Utc);
        var dto = new BaseQuizCategoryDto(
            Name: "Animals",
            FeatureType: FeatureType.Int,
            ImageUrl: "/img.png",
            AuthorName: "John",
            CreationDate: creationDate,
            QuestionToDisplay: "Heaviest?",
            IsPremium: true);

        Assert.Equal("Animals", dto.Name);
        Assert.Equal(FeatureType.Int, dto.FeatureType);
        Assert.True(dto.IsPremium);
        Assert.Equal(creationDate, dto.CreationDate);
    }
}
