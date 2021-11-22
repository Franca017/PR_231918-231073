using System.Threading.Tasks;
using Domain;

namespace LogicInterface
{
    public interface ILogBuilderLogic
    {
        void BuildLog(Game game, string user, string action, string message);
    }
}