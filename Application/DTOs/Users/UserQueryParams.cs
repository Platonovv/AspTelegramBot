namespace AspTelegramBot.Application.DTOs.Users;

/// <summary>
/// Представляет параметры запроса для пагинации, сортировки и фильтрации списка пользователей.
/// </summary>
/// <param name="PageNumber">Номер страницы, начиная с 1.</param>
/// <param name="PageSize">Количество элементов на одной странице.</param>
/// <param name="SortBy">Имя свойства, по которому осуществляется сортировка.</param>
/// <param name="Descending">Флаг, указывающий, использовать ли сортировку по убыванию.</param>
public record UserQueryParams(int PageNumber = 1, int PageSize = 10, string? SortBy = null, bool Descending = false);