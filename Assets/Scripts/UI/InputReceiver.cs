using UnityEngine;
using UnityEngine.EventSystems;

public class InputReceiver : MonoBehaviour, IPointerClickHandler
{
	public delegate void ClickDelegate( PointerEventData eventData );
	public event ClickDelegate ClickEvent;

	public void OnPointerClick( PointerEventData eventData )
	{
		if( ClickEvent != null )
			ClickEvent( eventData );
	}
}