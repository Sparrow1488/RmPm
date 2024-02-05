namespace RmPm.Core;

/// <summary>
/// Запрос на создание прокси-клиента
/// </summary>
/// <param name="Name">Имя клиента</param>
/// <param name="Method">Метод шифрования</param>
public record CreateRequest(string Name, string Method);
