using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple EventBus System
/// </summary>
public class EventBus : System.IDisposable
{
	#region Variables

	private Dictionary <string, UnityEvent> eventDictionary;

	#endregion

	/// <summary>
	/// Constructor
	/// </summary>
	/// <returns>An EventBus instance</returns>
	public EventBus ()
	{
		eventDictionary = new Dictionary<string, UnityEvent> ();
	}
	
	// ================================================================================
	// Public Methods
	// ================================================================================
	
	/// <summary>
	/// Adds a listener to a UnityEvent
	/// </summary>
	/// <param name="eventName">the event to listen for</param>
	/// <param name="listener">the callback to call</param>
	/// <returns></returns>
	public void Add (string eventName, UnityAction listener)
	{
		UnityEvent thisEvent = null;
		if (eventDictionary.TryGetValue (eventName, out thisEvent)) {
			thisEvent.AddListener (listener);
		} else {
			thisEvent = new UnityEvent ();
			thisEvent.AddListener (listener);
			eventDictionary.Add (eventName, thisEvent);
		}
	}
	
	/// <summary>
	/// Adds a listener to a UnityEvent which is removed as soon as the event is dispatched
	/// </summary>
	/// <param name="eventName">the event to listen for</param>
	/// <param name="listener">the callback to call</param>
	/// <returns></returns>
	public void AddOnce (string eventName, UnityAction listener)
	{
		UnityEvent thisEvent = null;
		UnityAction onceListener = null;
		onceListener = () =>
		{
			thisEvent.RemoveListener (onceListener);
			listener ();
		};
		if (eventDictionary.TryGetValue (eventName, out thisEvent)) {
			thisEvent.AddListener (onceListener);
		} else {
			thisEvent = new UnityEvent ();
			thisEvent.AddListener (onceListener);
			eventDictionary.Add (eventName, thisEvent);
		}
	}
	
	/// <summary>
	/// Removes a listener from a UnityEvent
	/// </summary>
	/// <param name="eventName">the event to listen for</param>
	/// <param name="listener">the callback to call</param>
	/// <returns></returns>
	public void Remove (string eventName, UnityAction listener)
	{
		UnityEvent thisEvent = null;
		if (eventDictionary.TryGetValue (eventName, out thisEvent)) {
			thisEvent.RemoveListener (listener);
		}
	}
	
	/// <summary>
	/// Dispatches a UnityEvent
	/// </summary>
	/// <param name="eventName">the event to listen for</param>
	/// <returns></returns>
	public void Send (string eventName)
	{
		UnityEvent thisEvent = null;
		if (eventDictionary.TryGetValue (eventName, out thisEvent)) {
			thisEvent.Invoke ();
		}
	}
	
	/// <summary>
	/// Removes all UnityEvent listeners
	/// </summary>
	/// <returns></returns>
	public void RemoveAll ()
	{
		foreach (KeyValuePair<string, UnityEvent> entry in eventDictionary) {
			entry.Value.RemoveAllListeners ();
		}
		eventDictionary.Clear ();
	}
	
	/// <summary>
	/// IDisposable interface implementation
	/// </summary>
	/// <returns></returns>
	public void Dispose ()
	{
		RemoveAll ();
		if (eventDictionary != null) {
			eventDictionary = null;
		}
	}
}
