using System;

namespace ProtocolLibrary
{
    public class CommandConstants
    {
        public const int Login =  1;
        public const int ListGames = Login+1;
        public const int Purchase = ListGames+1;
        public const int Publish = Purchase+1;
        public const int Search = Publish+1;
        public const int FilterRating = Search+1;
        public const int ListPublishedGames = FilterRating+1;
        public const int ModifyGame = ListPublishedGames+1;
        public const int DeleteGame = ModifyGame+1;
        public const int GetReviews = DeleteGame+1;
        public const int Rate = GetReviews+1;
        public const int DetailGame = Rate+1;
        public const int Download = DetailGame+1;
        public const int ModifyImage = Download+1;
    }
}
