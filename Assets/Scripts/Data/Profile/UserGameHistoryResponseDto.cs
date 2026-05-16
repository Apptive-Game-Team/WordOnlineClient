using System;

namespace Data.Profile
{
    [Serializable]
    public class UserGameHistoryResponseDto
    {
        public UserGameHistoryDto[] games;
        public UserGameHistoryDto[] content;
        public int number;
        public int page;
        public int size;
        public int totalPages;
        public bool last;

        public UserGameHistoryDto[] Items => games ?? content ?? Array.Empty<UserGameHistoryDto>();
        public int PageNumber => number != 0 ? number : page;
        public bool HasExplicitLast => last || totalPages > 0;
        public bool IsLastPage => last || (totalPages > 0 && PageNumber >= totalPages - 1);
    }
}
