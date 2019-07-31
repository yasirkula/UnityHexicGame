using UnityEngine;
using UnityEngine.EventSystems;

public class InputReceiver : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler
{
#pragma warning disable 0649
	[SerializeField]
	private float sensitivity = 6f;
	private float swipeAmountSqr;
#pragma warning restore 0649

	public delegate void PointerDelegate( PointerEventData eventData );

	public event PointerDelegate ClickEvent;
	public event PointerDelegate SwipeEvent;

	private void Awake()
	{
		// For 1024 x 768, swipe amount is equal to "sensitivity * 9"
		swipeAmountSqr = sensitivity * ( Screen.width + Screen.height ) * 0.005f;
		swipeAmountSqr *= swipeAmountSqr;
	}

	public void OnPointerClick( PointerEventData eventData )
	{
		if( ClickEvent != null )
			ClickEvent( eventData );
	}

	public void OnBeginDrag( PointerEventData eventData )
	{
		eventData.pointerPress = null;
	}

	public void OnDrag( PointerEventData eventData )
	{
		if( ( eventData.position - eventData.pressPosition ).sqrMagnitude >= swipeAmountSqr )
		{
			eventData.pointerDrag = null;
			eventData.dragging = false;

			if( SwipeEvent != null )
				SwipeEvent( eventData );
		}
	}
}