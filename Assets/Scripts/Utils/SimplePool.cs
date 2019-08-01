using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

// Taken from: https://github.com/yasirkula/UnityGenericPool/blob/master/SimplePool.cs
// Generic pool class
public class SimplePool<T> where T : class
{
	private Stack<T> pool = null;

	// Called when new objects need to be created
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

	// Populate the pool
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
			// Pool is empty, create new object
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

	// Pool an item
	public void Push( T obj )
	{
		if( obj == null ) return;

#if UNITY_EDITOR
		// Adding the same object to a pool more than once can result in serious bugs
		for( int i = 0; i < pool.Count; i++ )
		{
			if( pool.Contains( obj ) )
				throw new Exception( "Object is already in the pool!" );
		}
#endif

		if( OnPush != null )
			OnPush( obj );

		pool.Push( obj );
	}

	// Create a new object
	private T NewObject()
	{
		if( CreateFunction != null )
			return CreateFunction();

		return null;
	}
}