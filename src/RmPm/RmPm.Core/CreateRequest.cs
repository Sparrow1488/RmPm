namespace RmPm.Core;

/// <summary>
/// Запрос на создание прокси-клиента
/// </summary>
/// <param name="FriendlyName">Имя клиента</param>
public record CreateRequest(string FriendlyName);
