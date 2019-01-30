using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

namespace Battlehub.RTEditor
{
    public class TransformComponent : MonoBehaviour
    {
        public Toggle EnableDisableToggle;
        public GameObject TransformComponentUI;
        public InputField PositionX;
        public InputField PositionY;
        public InputField PositionZ;
        public InputField RotationX;
        public InputField RotationY;
        public InputField RotationZ;
        public InputField ScaleX;
        public InputField ScaleY;
        public InputField ScaleZ;
        public Button Reset;

        private Transform[] m_transforms;
        private HashSet<GameObject> m_selectedGameObjects = new HashSet<GameObject>();

        private void Awake()
        {
            RuntimeSelection.SelectionChanged += OnRuntimeSelectionChanged;
        }

        private void OnDestroy()
        {
            RuntimeSelection.SelectionChanged -= OnRuntimeSelectionChanged;
        }

        private void OnEnable()
        {
            ExposeToEditor.TransformChanged += OnTransformChanged;

            PositionX.onValueChanged.AddListener(OnPositionXChanged);
            PositionY.onValueChanged.AddListener(OnPositionYChanged);
            PositionZ.onValueChanged.AddListener(OnPositionZChanged);
            RotationX.onValueChanged.AddListener(OnRotationXChanged);
            RotationY.onValueChanged.AddListener(OnRotationYChanged);
            RotationZ.onValueChanged.AddListener(OnRotationZChanged);
            ScaleX.onValueChanged.AddListener(OnScaleXChanged);
            ScaleY.onValueChanged.AddListener(OnScaleYChanged);
            ScaleZ.onValueChanged.AddListener(OnScaleZChanged);

            PositionX.onEndEdit.AddListener(OnEndEdit);
            PositionY.onEndEdit.AddListener(OnEndEdit);
            PositionZ.onEndEdit.AddListener(OnEndEdit);
            RotationX.onEndEdit.AddListener(OnEndEdit);
            RotationY.onEndEdit.AddListener(OnEndEdit);
            RotationZ.onEndEdit.AddListener(OnEndEdit);
            ScaleX.onEndEdit.AddListener(OnEndEdit);
            ScaleY.onEndEdit.AddListener(OnEndEdit);
            ScaleZ.onEndEdit.AddListener(OnEndEdit);

            Reset.onClick.AddListener(OnResetClick);
            EnableDisableToggle.onValueChanged.AddListener(OnEnableDisableValueChanged);

            OnRuntimeSelectionChanged(null);
        }

        private void OnDisable()
        {
            ExposeToEditor.TransformChanged -= OnTransformChanged;

            PositionX.onValueChanged.RemoveListener(OnPositionXChanged);
            PositionY.onValueChanged.RemoveListener(OnPositionYChanged);
            PositionZ.onValueChanged.RemoveListener(OnPositionZChanged);
            RotationX.onValueChanged.RemoveListener(OnRotationXChanged);
            RotationY.onValueChanged.RemoveListener(OnRotationYChanged);
            RotationZ.onValueChanged.RemoveListener(OnRotationZChanged);
            ScaleX.onValueChanged.RemoveListener(OnScaleXChanged);
            ScaleY.onValueChanged.RemoveListener(OnScaleYChanged);
            ScaleZ.onValueChanged.RemoveListener(OnScaleZChanged);

            PositionX.onEndEdit.RemoveListener(OnEndEdit);
            PositionY.onEndEdit.RemoveListener(OnEndEdit);
            PositionZ.onEndEdit.RemoveListener(OnEndEdit);
            RotationX.onEndEdit.RemoveListener(OnEndEdit);
            RotationY.onEndEdit.RemoveListener(OnEndEdit);
            RotationZ.onEndEdit.RemoveListener(OnEndEdit);
            ScaleX.onEndEdit.RemoveListener(OnEndEdit);
            ScaleY.onEndEdit.RemoveListener(OnEndEdit);
            ScaleZ.onEndEdit.RemoveListener(OnEndEdit);

            Reset.onClick.RemoveListener(OnResetClick);

            EnableDisableToggle.onValueChanged.RemoveListener(OnEnableDisableValueChanged);
        }

        private bool m_handleTransformChange = true;
        private void HandlePositionChanged()
        {
            if(!m_handleTransformChange)
            {
                return;
            }

            if (m_transforms == null || m_transforms.Length == 0)
            {
                return;
            }
            float xVal;
            if (!float.TryParse(PositionX.text, out xVal))
            {
                return;
            }
            float yVal;
            if (!float.TryParse(PositionY.text, out yVal))
            {
                return;
            }
            float zVal;
            if (!float.TryParse(PositionZ.text, out zVal))
            {
                return;
            }
            for (int i = 0; i < m_transforms.Length; ++i)
            {
                m_transforms[i].position = new Vector3(xVal, yVal, zVal);
            }
        }

        private void HandleRotationChanged()
        {
            if (!m_handleTransformChange)
            {
                return;
            }
            if (m_transforms == null || m_transforms.Length == 0)
            {
                return;
            }
            float xVal;
            if (!float.TryParse(RotationX.text, out xVal))
            {
                return;
            }
            float yVal;
            if (!float.TryParse(RotationY.text, out yVal))
            {
                return;
            }
            float zVal;
            if (!float.TryParse(RotationZ.text, out zVal))
            {
                return;
            }
            for (int i = 0; i < m_transforms.Length; ++i)
            {
                m_transforms[i].rotation = Quaternion.Euler(xVal, yVal, zVal);
            }
        }

        private void HandleScaleChanged()
        {
            if (!m_handleTransformChange)
            {
                return;
            }
            if (m_transforms == null || m_transforms.Length == 0)
            {
                return;
            }
            float xVal;
            if (!float.TryParse(ScaleX.text, out xVal))
            {
                return;
            }
            float yVal;
            if (!float.TryParse(ScaleY.text, out yVal))
            {
                return;
            }
            float zVal;
            if (!float.TryParse(ScaleZ.text, out zVal))
            {
                return;
            }
            for (int i = 0; i < m_transforms.Length; ++i)
            {
                m_transforms[i].localScale = new Vector3(xVal, yVal, zVal);
            }
        }

        private void EndEditField(InputField field)
        {
            float val;
            if (!float.TryParse(field.text, out val))
            {
                field.text = "0";
            }
        }

        private void OnEndEdit(string value)
        {
            EndEditField(PositionX);
            EndEditField(PositionY);
            EndEditField(PositionZ);
            EndEditField(RotationX);
            EndEditField(RotationY);
            EndEditField(RotationZ);
            EndEditField(ScaleX);
            EndEditField(ScaleY);
            EndEditField(ScaleZ);
        }

        private void OnPositionXChanged(string value)
        {
            HandlePositionChanged();
        }

        private void OnPositionYChanged(string value)
        {
            HandlePositionChanged();
        }

        private void OnPositionZChanged(string value)
        {
            HandlePositionChanged();
        }

        private void OnRotationXChanged(string value)
        {
            HandleRotationChanged();
        }

        private void OnRotationYChanged(string value)
        {
            HandleRotationChanged();
        }

        private void OnRotationZChanged(string value)
        {
            HandleRotationChanged();
        }

        private void OnScaleXChanged(string value)
        {
            HandleScaleChanged();
        }

        private void OnScaleYChanged(string value)
        {
            HandleScaleChanged();
        }

        private void OnScaleZChanged(string value)
        {
            HandleScaleChanged();
        }

        private void OnTransformChanged(ExposeToEditor obj)
        {
            
            if (!m_selectedGameObjects.Contains(obj.gameObject))
            {
                return;
            }

            m_handleTransformChange = false;
            UpdateAllFields();
            m_handleTransformChange = true;
        }

        private void OnRuntimeSelectionChanged(Object[] unselected)
        {
            
            GameObject[] gameObjects = RuntimeSelection.gameObjects;
            if(gameObjects == null)
            {
                m_selectedGameObjects.Clear();

                EnableDisableToggle.gameObject.SetActive(false);
                TransformComponentUI.gameObject.SetActive(false);
                m_transforms = null;
            }
            else
            {
                m_selectedGameObjects.Clear();
                m_transforms = gameObjects.Where(g => g.GetComponent<ExposeToEditor>()).Select(g => g.GetComponent<Transform>()).Where(t => t.GetType() == typeof(Transform)).ToArray();
                for(int i = 0; i < m_transforms.Length; ++i)
                {
                    m_selectedGameObjects.Add(m_transforms[i].gameObject);
                }

                if (m_transforms.Length > 0)
                {
                    EnableDisableToggle.gameObject.SetActive(true);
                    TransformComponentUI.gameObject.SetActive(true);
                    m_handleTransformChange = false;
                    UpdateAllFields();
                    m_handleTransformChange = true;
                }
                else
                {
                    EnableDisableToggle.gameObject.SetActive(false);
                    TransformComponentUI.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateAllFields()
        {
            var positionX = m_transforms.Select(t => t.position.x);
            var positionY = m_transforms.Select(t => t.position.y);
            var positionZ = m_transforms.Select(t => t.position.z);
            var rotationX = m_transforms.Select(t => t.rotation.eulerAngles.x);
            var rotationY = m_transforms.Select(t => t.rotation.eulerAngles.y);
            var rotationZ = m_transforms.Select(t => t.rotation.eulerAngles.z);
            var scaleX = m_transforms.Select(t => t.localScale.x);
            var scaleY = m_transforms.Select(t => t.localScale.y);
            var scaleZ = m_transforms.Select(t => t.localScale.z);
            SetFieldValue(PositionX, positionX);
            SetFieldValue(PositionY, positionY);
            SetFieldValue(PositionZ, positionZ);
            SetFieldValue(RotationX, rotationX);
            SetFieldValue(RotationY, rotationY);
            SetFieldValue(RotationZ, rotationZ);
            SetFieldValue(ScaleX, scaleX);
            SetFieldValue(ScaleY, scaleY);
            SetFieldValue(ScaleZ, scaleZ);
            EnableDisableToggle.isOn = m_transforms.All(t => t.gameObject.activeSelf);
        }

        private void SetFieldValue(InputField field, IEnumerable<float> values)
        {
            if (values.Any(p => p != values.First()))
            {
                field.text = string.Empty;
            }
            else
            {
                field.text = values.First().ToString();
            }
        }

        private void OnResetClick()
        {
            float zero = 0;
            float one = 1;
            PositionX.text = zero.ToString();
            PositionY.text = zero.ToString();
            PositionZ.text = zero.ToString();
            RotationX.text = zero.ToString();
            RotationY.text = zero.ToString();
            RotationZ.text = zero.ToString();
            ScaleX.text = one.ToString();
            ScaleY.text = one.ToString();
            ScaleZ.text = one.ToString();
        }

        private void OnEnableDisableValueChanged(bool value)
        {
            for (int i = 0; i < m_transforms.Length; ++i)
            {
                m_transforms[i].gameObject.SetActive(value);
            }
        }
    }
}
