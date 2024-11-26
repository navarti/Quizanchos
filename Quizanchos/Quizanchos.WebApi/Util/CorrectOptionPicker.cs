namespace Quizanchos.WebApi.Util;

public static class CorrectOptionPicker
{
    public static int PickCorrectOption<T>(IEnumerable<T> items) where T : IComparable<T>
    {
        return IndexOfMax(items);
    }

    private static int IndexOfMax<T>(IEnumerable<T> items) where T : IComparable<T>
    {
        if (items == null || !items.Any())
        {
            throw new ArgumentException("The collection is null or empty.");
        }

        int maxIndex = -1;
        T maxValue = default;
        bool isFirst = true;
        int currentIndex = 0;

        foreach (var item in items)
        {
            if (isFirst || item.CompareTo(maxValue) > 0)
            {
                maxValue = item;
                maxIndex = currentIndex;
                isFirst = false;
            }
            currentIndex++;
        }

        return maxIndex;
    }
}
