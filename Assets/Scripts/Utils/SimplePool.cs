using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// Generic pool class
// Taken from: https://github.com/yasirkula/UnityGenericPool/blob/master/SimplePool.cs
public class SimplePool<T> where T : class
{
	// Objects stored in the pool
	private Stack<T> pool = null;

	// A function that can be used to override default NewObject( T ) function
	public Func<T> CreateFunction;

	// Actions that can be used to implement extra logic on pushed/popped objects
	public Action<T> OnPush, OnPop;

	public SimplePool( Func<T> CreateFunction = null, Action<T> OnPush = null, Action<T> OnPop = null )
	{
		pool = new Stack<T>();

		this.CreateFunction = CreateFunction;
		this.OnPush = OnPush;
		this.OnPop = OnPop;
	}

	// Populate the pool with the default blueprint
	public bool Populate( int count )
	{
		if( count <= 0 )
			return true;

		// Create a single object first to see if everything works fine
		// If not, return false
		T obj = NewObject();
		if( obj == null )
			return false;

		Push( obj );

		// Everything works fine, populate the pool with the remaining items
		for( int i = 1; i < count; i++ )
			Push( NewObject() );

		return true;
	}

	// Fetch an item from the pool
	public T Pop()
	{
		T objToPop;

		if( pool.Count == 0 )
		{
			// Pool is empty, instantiate the blueprint
			objToPop = NewObject();
		}
		else
		{
			// Pool is not empty, fetch the first item in the pool
			objToPop = pool.Pop();
			while( objToPop == null )
			{
				// Some objects in the pool might have been destroyed (maybe during a scene transition),
				// consider that case
				if( pool.Count > 0 )
					objToPop = pool.Pop();
				else
				{
					objToPop = NewObject();
					break;
				}
			}
		}

		if( OnPop != null )
			OnPop( objToPop );

		return objToPop;
	}

	// Fetch multiple items at once from the pool
	public T[] Pop( int count )
	{
		if( count <= 0 )
			return new T[0];

		T[] result = new T[count];
		for( int i = 0; i < count; i++ )
			result[i] = Pop();

		return result;
	}

	// Pool an item
	public void Push( T obj )
	{
		if( obj == null ) return;

#if UNITY_EDITOR
		for( int i = 0; i < pool.Count; i++ )
		{
			if( pool.Contains( obj ) )
				throw new Exception( "Object is already in pool!" );
		}
#endif

		if( OnPush != null )
			OnPush( obj );

		pool.Push( obj );
	}

	// Pool multiple items at once
	public void Push( IEnumerable<T> objects )
	{
		if( objects == null ) return;

		foreach( T obj in objects )
			Push( obj );
	}

	// Clear the pool
	public void Clear( bool destroyObjects = true )
	{
		if( destroyObjects )
		{
			// Destroy all the Objects in the pool
			foreach( T item in pool )
			{
				Object.Destroy( item as Object );
			}
		}

		pool.Clear();
	}

	// Create an instance of the blueprint and return it
	private T NewObject()
	{
		if( CreateFunction != null )
			return CreateFunction();

		return null;
	}
}