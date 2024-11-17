namespace Quizanchos.WebApi.Services.Interfaces;

public interface IQuizCardService
{
    public Task GetCardForSession(Guid gameSessionid, int cardIndex);
}
