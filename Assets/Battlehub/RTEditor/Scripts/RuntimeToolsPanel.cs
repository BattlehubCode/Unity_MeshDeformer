using UnityEngine;
using UnityEngine.UI;

using Battlehub.RTHandles;
namespace Battlehub.RTEditor
{
    public class RuntimeToolsPanel : MonoBehaviour
    {
        private bool m_handleValueChange = true;

        public Toggle ViewToggle;
        public Toggle MoveToggle;
        public Toggle RotateToggle;
        public Toggle ScaleToggle;

        public Toggle PivotRotationToggle;
        public Toggle WireframeToggle;
      
        private void OnEnable()
        {   
            OnRuntimeToolChanged();
            OnPivotRotationChanged();
            RuntimeTools.ToolChanged += OnRuntimeToolChanged;
            RuntimeTools.PivotRotationChanged += OnPivotRotationChanged;
            if (ViewToggle != null)
            {
                ViewToggle.onValueChanged.AddListener(OnViewToggleValueChanged);
            }
            if (MoveToggle != null)
            {
                MoveToggle.onValueChanged.AddListener(OnMoveToggleValueChanged);
            }
            if (RotateToggle != null)
            {
                RotateToggle.onValueChanged.AddListener(OnRotateToggleValueChanged);
            }
            if (ScaleToggle != null)
            {
                ScaleToggle.onValueChanged.AddListener(OnScaleToggleValueChanged);
            }
            if(PivotRotationToggle != null)
            {
                PivotRotationToggle.onValueChanged.AddListener(OnPivotRotationToggleValueChanged);
            }
            if(WireframeToggle != null)
            {
                WireframeToggle.onValueChanged.AddListener(OnWireframeToggleValueChanged);
            }
        }

        private void OnDisable()
        {
            RuntimeTools.ToolChanged -= OnRuntimeToolChanged;
            RuntimeTools.PivotRotationChanged -= OnPivotRotationChanged;
            if (ViewToggle != null)
            {
                ViewToggle.onValueChanged.RemoveListener(OnViewToggleValueChanged);
            }
            if (MoveToggle != null)
            {
                MoveToggle.onValueChanged.RemoveListener(OnMoveToggleValueChanged);
            }
            if (RotateToggle != null)
            {
                RotateToggle.onValueChanged.RemoveListener(OnRotateToggleValueChanged);
            }
            if (ScaleToggle != null)
            {
                ScaleToggle.onValueChanged.RemoveListener(OnScaleToggleValueChanged);
            }
            if (PivotRotationToggle != null)
            {
                PivotRotationToggle.onValueChanged.RemoveListener(OnPivotRotationToggleValueChanged);
            }
            if (WireframeToggle != null)
            {
                WireframeToggle.onValueChanged.RemoveListener(OnWireframeToggleValueChanged);
            }
        }

        private void OnViewToggleValueChanged(bool value)
        {
       
            if(!m_handleValueChange)
            {
                return;
            }
            if (value)
            {
                RuntimeTools.Current = RuntimeTool.View;
                m_handleValueChange = false;
                RotateToggle.isOn = false;
                ScaleToggle.isOn = false;
                MoveToggle.isOn = false;
                m_handleValueChange = true;
            }
            else
            {
                if (RuntimeTools.Current == RuntimeTool.View)
                {
                    ViewToggle.isOn = true;
                }
            }
        }
        private void OnMoveToggleValueChanged(bool value)
        {
            if (!m_handleValueChange)
            {
                return;
            }
            if (value)
            {
                RuntimeTools.Current = RuntimeTool.Move;
                m_handleValueChange = false;
                RotateToggle.isOn = false;
                ScaleToggle.isOn = false;
                ViewToggle.isOn = false;
                m_handleValueChange = true;

            }
            else
            {
                if (RuntimeTools.Current == RuntimeTool.Move)
                {
                    MoveToggle.isOn = true;
                }
            }
        }

        private void OnRotateToggleValueChanged(bool value)
        {
            if (!m_handleValueChange)
            {
                return;
            }
            if (value)
            {
                RuntimeTools.Current = RuntimeTool.Rotate;
                m_handleValueChange = false;
                ViewToggle.isOn = false;
                ScaleToggle.isOn = false;
                MoveToggle.isOn = false;
                m_handleValueChange = true;
            }
            else
            {
                if (RuntimeTools.Current == RuntimeTool.Rotate)
                {
                    RotateToggle.isOn = true;
                }
            }

        }

        private void OnScaleToggleValueChanged(bool value)
        {
            if (!m_handleValueChange)
            {
                return;
            }
            if (value)
            {
                RuntimeTools.Current = RuntimeTool.Scale;
                m_handleValueChange = false;
                ViewToggle.isOn = false;
                RotateToggle.isOn = false;
                MoveToggle.isOn = false;
                m_handleValueChange = true;
            }
            else
            {
                if(RuntimeTools.Current == RuntimeTool.Scale)
                {
                    ScaleToggle.isOn = true;
                }
            }
        }

        private void OnPivotRotationToggleValueChanged(bool value)
        {
            if(value)
            {
                RuntimeTools.PivotRotation = RuntimePivotRotation.Global;
            }
            else
            {
                RuntimeTools.PivotRotation = RuntimePivotRotation.Local;
            }
        }

        private void OnWireframeToggleValueChanged(bool value)
        {
            //NOT IMPLEMENTED
        }

        private void OnPivotRotationChanged()
        {
            if(PivotRotationToggle != null)
            {
                if (RuntimeTools.PivotRotation == RuntimePivotRotation.Global)
                {
                    PivotRotationToggle.isOn = true;
                }
                else
                {
                    PivotRotationToggle.isOn = false;
                }
            }
        }

        private void OnRuntimeToolChanged()
        {
            if(!m_handleValueChange)
            {
                return;
            }
            if (ViewToggle != null)
            {
                ViewToggle.isOn = RuntimeTools.Current == RuntimeTool.View;
            }
            if (MoveToggle != null)
            {
                MoveToggle.isOn = RuntimeTools.Current == RuntimeTool.Move;
            }
            if (RotateToggle != null)
            {
                RotateToggle.isOn = RuntimeTools.Current == RuntimeTool.Rotate;
            }
            if (ScaleToggle != null)
            {
                ScaleToggle.isOn = RuntimeTools.Current == RuntimeTool.Scale;
            }
        }
    }
}
