namespace Quizanchos.WebApi.Dto.TopUp;

public record TopUpOrderStatusDto(Guid OrderId, string Status, int CoinsBalance);
