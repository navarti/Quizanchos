namespace Quizanchos.DbUpdater.Utils;

internal class RandomIntPicker
{
    private readonly int[] _pages;
    private int _remainingCount;
    private readonly Random _random;

    public RandomIntPicker(int pagesCount)
    {
        if (pagesCount <= 0)
            throw new ArgumentException("pagesCount must be greater than 0", nameof(pagesCount));

        _pages = new int[pagesCount];
        for (int i = 0; i < pagesCount; i++)
        {
            _pages[i] = i;
        }

        _remainingCount = pagesCount;
        _random = new Random();
    }

    public int? PickRandomPage()
    {
        if (_remainingCount == 0)
            return null;

        int randomIndex = _random.Next(0, _remainingCount);
        int pickedPage = _pages[randomIndex];

        _pages[randomIndex] = _pages[_remainingCount - 1];
        _pages[_remainingCount - 1] = pickedPage;

        _remainingCount--;

        return pickedPage;
    }
}
