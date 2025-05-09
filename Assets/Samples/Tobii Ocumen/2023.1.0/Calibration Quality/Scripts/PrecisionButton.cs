using Tobii.G2OM;
using UnityEngine;
using UnityEngine.UI;

namespace Tobii.Ocumen.Configuration.Samples
{
    public class PrecisionButton : MonoBehaviour, IGazeFocusable
    {
        public int PrecisionStandardDeviation => precisionStandardDeviation;

#pragma warning disable 649
        [SerializeField] private int precisionStandardDeviation;
        [SerializeField] private Transform textLabel;
        [SerializeField] private Image highlightUnderline;
        [SerializeField] private float highlightScaleFactor = 0.05f;
        [SerializeField] private float highlightAnimationTime = 0.1f;
        [SerializeField] private Color buttonHighlightColor;
        [SerializeField] private Color buttonPressedColor;
#pragma warning restore 649

        private float _highlightTargetValue;
        private Image _buttonImage;
        private Color _defaultButtonColor;
        private Color _targetButtonColor;
        private bool _selected;

        private void Awake()
        {
            _buttonImage = GetComponent<Image>();
            _defaultButtonColor = _buttonImage.color;
            _targetButtonColor = _defaultButtonColor;
        }

        private void Update()
        {
            UpdateGraphics();
        }

        private void UpdateGraphics()
        {
            if (_selected) return;

            // Highlight underline
            highlightUnderline.fillAmount = Mathf.Lerp(highlightUnderline.fillAmount, _highlightTargetValue,
                Time.deltaTime * (1f / highlightAnimationTime));

            // Highlight scale
            var currentScale = textLabel.localScale.x;
            var newScale = Mathf.Lerp(currentScale, 1 + (_highlightTargetValue * highlightScaleFactor),
                Time.deltaTime * (1f / highlightAnimationTime));
            textLabel.localScale = new Vector3(newScale, newScale, newScale);

            // Highlight color
            var newButtonColor = Color.Lerp(_buttonImage.color, _targetButtonColor,
                Time.deltaTime * (1f / highlightAnimationTime));
            _buttonImage.color = newButtonColor;
        }

        public void GazeFocusChanged(bool hasFocus)
        {
            _highlightTargetValue = hasFocus ? 1 : 0;
            _targetButtonColor = hasFocus ? buttonHighlightColor : _defaultButtonColor;
        }

        public void Select()
        {
            _selected = true;
            _buttonImage.color = buttonPressedColor;
            highlightUnderline.fillAmount = 1;
            textLabel.localScale = Vector3.one;
        }

        public void Deselect()
        {
            _selected = false;
            _buttonImage.color = _defaultButtonColor;
            highlightUnderline.fillAmount = 0;
        }
    }
}