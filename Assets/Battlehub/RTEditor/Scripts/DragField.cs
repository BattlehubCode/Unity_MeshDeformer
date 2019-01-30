using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class DragField : MonoBehaviour, IDragHandler, IBeginDragHandler, IDropHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public InputField Field;
        public float IncrementFactor = 0.1f;
      
        public Texture2D DragCursor;

        private void Start()
        {
            if(Field == null)
            {
                Debug.LogWarning("Set Field");
                return;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
           
        }

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
           
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if(Field == null)
            {
                return;
            }

            if (Field.contentType == InputField.ContentType.DecimalNumber)
            {
                float d;
                if(float.TryParse(Field.text, out d))
                {
                    d += IncrementFactor * eventData.delta.x;
                    Field.text = d.ToString();
                }
                
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Cursor.SetCursor(DragCursor, new Vector2(24, 24), CursorMode.Auto);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }

}
