using UnityEngine;

namespace Tobii.Ocumen.Configuration.Samples
{
    public class PositionGuideView : MonoBehaviour
    {
#pragma warning disable 0649 // Field is never assigned to, and will always have its default value null
        [Tooltip("The left eye graphic that moves in relation to the frame.")] [SerializeField]
        private Transform leftMovableGraphic;

        [Tooltip("The right eye graphic that moves in relation to the frame.")] [SerializeField]
        private Transform rightMovableGraphic;

        [Header("Position Guide Customization")]
        [Tooltip("Change the movement multiplier to match the frame graphic's size.")]
        [SerializeField]
        private float movementMultiplier;

        [Tooltip(
            "Lock the horizontal axis to only position vertically. This can help to simplify the position guide for headsets that don't have physical IPD adjustment.")]
        [SerializeField]
        private bool lockX;

        [Tooltip("Horizontally invert the direction of the movable graphics.")] [SerializeField]
        private bool invertX;

        [Tooltip("Vertically invert the direction of the movable graphics.")] [SerializeField]
        private bool invertY;
#pragma warning restore 0649

        private Vector2 _newPositionLeft;
        private Vector2 _newPositionRight;
        private readonly ConfigurationManager _configurationManager = new ConfigurationManager();

        private void Update()
        {
            leftMovableGraphic.gameObject.SetActive(_configurationManager.PositionGuideData.LeftIsValid);
            rightMovableGraphic.gameObject.SetActive(_configurationManager.PositionGuideData.RightIsValid);

            // Retrieve the position guide signal and subtract 0.5 to make 0 the center point, since 0.5 is the signal's center point.
            _newPositionLeft = _configurationManager.PositionGuideData.Left - Vector2.one * 0.5f;
            _newPositionRight = _configurationManager.PositionGuideData.Right - Vector2.one * 0.5f;

            // Lock to a single axis, if desired.
            if (lockX)
            {
                _newPositionLeft.x = 0;
                _newPositionRight.x = 0;
            }

            // Invert movable graphic direction, if desired.
            if (invertX)
            {
                _newPositionLeft.x *= -1;
                _newPositionRight.x *= -1;
            }

            if (invertY)
            {
                _newPositionLeft.y *= -1;
                _newPositionRight.y *= -1;
            }

            leftMovableGraphic.localPosition = _newPositionLeft * movementMultiplier;
            rightMovableGraphic.localPosition = _newPositionRight * movementMultiplier;
        }
    }
}