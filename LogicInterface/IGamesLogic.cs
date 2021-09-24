﻿using System.Collections.Generic;
using Domain;

namespace LogicInterface
{
    public interface IGamesLogic
    {
        List<Game> GetAll();
        Game GetById(int gameId);
        Game Add(Game game);
        List<Game> GetSearchedGames(string keywords);
        void Delete(int gameId);
        List<Game> GetPublishedGames(User userLogged);
        List<Review> GetGameReviews(int gameId);
        void AddReviewToGame(Review newReview);
    }
}