using UnityEngine;

namespace MidTerm2
{
    public class CastlesController : MonoBehaviourSingleton<CastlesController>
    {
        public CastlesModel model;

        public void NextTurn()
        {
            model?.ChangeTurn();
        }
    }
}