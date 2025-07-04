using MidTerm2;
using UnityEngine;

namespace ReflectionTest
{
    public class ReflectionTest : MonoBehaviour
    {
        [SerializeField] private GameObject castlesGame;

        void Start()
        {
            GameObject castles = Instantiate(castlesGame);
            castles.GetComponent<CastlesProgram>().Initialize();
        }
    }
}