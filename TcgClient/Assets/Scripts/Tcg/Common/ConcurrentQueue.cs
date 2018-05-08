using System;
using System.Collections.Generic;
using System.Threading;

public class ConcurrentQueue<T>
{
	int limit;
	private readonly object syncLock = new object();
	private Queue<T> queue;

	public int Count
	{
		get
		{
			lock (syncLock)
			{
				return queue.Count;
			}
		}
	}

	public ConcurrentQueue(int limit_ = 0)
	{
		this.queue = new Queue<T>();
		limit = limit_;
	}

	public T Peek()
	{
		lock (syncLock)
		{
			return queue.Peek();
		}
	}

	public bool TryEnqueue(T obj)
	{
		lock (syncLock)
		{
			while(true)
			{
				if (limit > 0 && queue.Count >= limit)
				{
					return false;
				}
				else
				{
					queue.Enqueue (obj);
					if (queue.Count <= 1)
					{
						Monitor.Pulse (syncLock);
					}
					return true;
				}
			}
		}
	}

	public void Enqueue(T obj)
	{
		lock (syncLock)
		{
			while(true)
			{
				if (limit > 0 && queue.Count >= limit)
				{
					Monitor.Wait (syncLock);
				}
				else
				{
					queue.Enqueue (obj);
					if (queue.Count <= 1)
					{
						Monitor.Pulse (syncLock);
					}
					break;
				}
			}
		}
	}

	public T Dequeue(int timeoutMillis = -1)
	{
		lock (syncLock)
		{
			while (true)
			{
				if(queue.Count > 0 )
				{
					return queue.Dequeue();
				}
				else
				{
					if (timeoutMillis >= 0)
					{
						Monitor.Wait (syncLock, timeoutMillis);
						if (queue.Count <= 0)
						{
							throw new TimeoutException ("timeout dequeue");
						}
					}
					else
					{
						Monitor.Wait (syncLock);
					}
				}
			}
		}
	}

	public bool TryDequeue(out T result)
	{
		lock (syncLock)
		{
			if (queue.Count > 0)
			{
				result = queue.Dequeue();
				return true;
			}
			else
			{
				result = default(T);
				return false;
			}
		}
	}

	public void Clear()
	{
		lock (syncLock)
		{
			queue.Clear();
		}
	}

	public T[] CopyToArray()
	{
		lock (syncLock)
		{
			if (queue.Count == 0)
			{
				return new T[0];
			}

			T[] values = new T[queue.Count];
			queue.CopyTo(values, 0);
			return values;
		}
	}

	public static ConcurrentQueue<T> InitFromArray(IEnumerable<T> initValues)
	{
		var queue = new ConcurrentQueue<T>();

		if (initValues == null)
		{
			return queue;
		}

		foreach (T val in initValues)
		{
			queue.Enqueue(val);
		}

		return queue;
	}
}
