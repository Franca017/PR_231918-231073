using System.Collections.Generic;
using Domain;

namespace LogicInterface
{
    public interface IGamesLogic
    {
        List<Game> GetAll();
    }
}