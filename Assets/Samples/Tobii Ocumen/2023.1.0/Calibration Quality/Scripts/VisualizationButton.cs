using Tobii.G2OM;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Tobii.Ocumen.Configuration.Samples
{
    public class VisualizationButton : MonoBehaviour, IGazeFocusable
    {
        public Eye Eye => eye;

#pragma warning disable 649
        [SerializeField] private Eye eye;
        [SerializeField] private Transform textLabel;
        [SerializeField] private Image highlightUnderline;
        [SerializeField] private float highlightScaleFactor = 0.05f;
        [SerializeField] private float highlightAnimationTime = 0.1f;
#pragma warning restore 649

        private float _highlightTargetValue;

        private void Update()
        {
            UpdateGraphics();
        }

        private void UpdateGraphics()
        {
            highlightUnderline.fillAmount = Mathf.Lerp(highlightUnderline.fillAmount, _highlightTargetValue,
                Time.deltaTime * (1f / highlightAnimationTime));
            var newScale = Mathf.Lerp(textLabel.localScale.x, 1 + (_highlightTargetValue * highlightScaleFactor),
                Time.deltaTime * (1f / highlightAnimationTime));
            textLabel.localScale = new Vector3(newScale, newScale, newScale);
        }

        public void GazeFocusChanged(bool hasFocus)
        {
            _highlightTargetValue = hasFocus ? 1 : 0;
        }
    }
}