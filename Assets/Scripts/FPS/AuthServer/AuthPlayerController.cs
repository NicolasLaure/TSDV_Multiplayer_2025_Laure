using Health;

namespace FPS.AuthServer
{
    public class AuthPlayerController : PlayerController
    {
        public int id;

        protected override void Start()
        {
            InputHandler.Instance.idToMoveActions[id] += HandleDir;
            InputHandler.Instance.idToCrouchActions[id] += ToggleCrouch;
            InputHandler.Instance.idToShootActions[id] += Shoot;

            lastFrameTrs = transform.localToWorldMatrix;

            _healthPoints = GetComponent<HealthPoints>();
            _healthPoints.onDeathEvent += OnDeath;
        }
    }
}