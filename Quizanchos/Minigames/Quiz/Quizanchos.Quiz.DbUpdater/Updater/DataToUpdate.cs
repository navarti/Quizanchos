using Quizanchos.Common.Enums;

namespace Quizanchos.DbUpdater.Updater;

internal class DataToUpdate
{
    public FeatureType FeatureType { get; }
    public string CategoryName { get; }
    public string ImageUrl { get; }
    public string AuthorName { get; }
    public DateTime CreationDate { get; }
    public string QuestionToDisplay { get; }
    public bool IsPremium { get; set; }
    public EntityWithValueToUpdate[] Entities { get; }

    public DataToUpdate(FeatureType featureType, string categoryName, string imageUrl, string authorName, DateTime creationDate,
        string questionToDisplay, bool isPremium, EntityWithValueToUpdate[] entities)
    {
        FeatureType = featureType;
        CategoryName = categoryName;
        Entities = entities;
        ImageUrl = imageUrl;
        AuthorName = authorName;
        CreationDate = creationDate;
        QuestionToDisplay = questionToDisplay;
        IsPremium = isPremium;
    }
}
