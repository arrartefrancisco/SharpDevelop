﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ICSharpCode.NRefactory.Utils
{
	/// <summary>
	/// Allows the registration of static "caching types" which can then be used to efficiently retrieve an
	/// instance per CacheManager (or even per CacheManager and thread).
	/// </summary>
	/// <remarks>This class is thread-safe</remarks>
	public sealed class CacheManager : IDisposable
	{
		readonly ConcurrentDictionary<object, object> dict = new ConcurrentDictionary<object, object>(ReferenceComparer.Instance);
		
		public ConcurrentDictionary<object, object> Dictionary {
			get { return dict; }
		}
		
		/* Lots of code commented out because I don't know if it's useful, clients can usually replicate
		 * the functionality much more easily and only need the Disposed event to ensure cleanup.
		 * 
		 * Actually, what I've implemented here looks very much like .NET's internal System.LocalDataStore
		 * (used for Thread.GetData/SetData)
		 * 
		static int nextSharedIndex, nextThreadLocalIndex;
		
		/// <summary>
		/// Registers a new cache type. This causes each CacheManager to allocate space for the new cache type.
		/// </summary>
		/// <param name="isThreadLocal">Specifies whether this cache is shared (multi-threaded) or whether
		/// there is one instance per thread.</param>
		/// <returns>Returns a token that can be used to access the cache.</returns>
		public static CacheToken<T> RegisterType<T>(CacheMode mode) where T : class, new()
		{
			int index;
			switch (mode) {
				case CacheMode.Shared:
					index = Interlocked.Increment(ref nextSharedIndex);
					break;
				case CacheMode.ThreadLocal:
					index = Interlocked.Increment(ref nextThreadLocalIndex);
					break;
				default:
					throw new ArgumentException("Invalid value for CacheMode", "mode");
			}
			return new CacheToken<T>(mode, index);
		}
		
		readonly object lockObj = new object();
		volatile object[] _sharedCaches = new object[nextSharedIndex];
		ThreadLocal<object[]> threadLocalCaches = new ThreadLocal<object[]>(() => new object[nextThreadLocalIndex]);
		
		/// <summary>
		/// Gets the cache using the specified token.
		/// </summary>
		public T Get<T>(CacheToken<T> token) where T : class, new()
		{
			switch (token.Mode) {
				case CacheMode.Shared:
					object[] sharedCaches = this._sharedCaches;
					if (token.Index < sharedCaches.Length) {
						object c = sharedCaches[token.Index];
						if (c != null)
							return (T)c;
					}
					// it seems like the cache doesn't exist yet, so try to create it:
					T newCache = new T();
					lock (lockObj) {
						sharedCaches = this._sharedCaches; // fetch fresh value after locking
						// use double-checked locking
						if (token.Index < sharedCaches.Length) {
							object c = sharedCaches[token.Index];
							if (c != null) {
								// looks like someone else was faster creating it than this thread
								return (T)c;
							}
						} else {
							Array.Resize(ref sharedCaches, nextSharedIndex);
							this._sharedCaches = sharedCaches;
						}
						sharedCaches[token.Index] = newCache;
					}
					return newCache;
				case CacheMode.ThreadLocal:
					object[] localCaches = threadLocalCaches.Value;
					if (token.Index >= localCaches.Length) {
						Array.Resize(ref localCaches, nextThreadLocalIndex);
						threadLocalCaches.Value = localCaches;
					}
					object lc = localCaches[token.Index];
					if (lc != null) {
						return (T)lc;
					} else {
						T newLocalCache = new T();
						localCaches[token.Index] = newLocalCache;
						return newLocalCache;
					}
				default:
					throw new ArgumentException("Invalid token");
			}
		}
		 */
		
		public event EventHandler Disposed;
		
		/// <summary>
		/// Invokes the <see cref="Disposed"/> event.
		/// </summary>
		public void Dispose()
		{
			//threadLocalCaches.Dispose(); // dispose the ThreadLocal<T>
			// TODO: test whether this frees the referenced value on all threads
			
			// fire Disposed() only once by removing the old event handlers
			EventHandler disposed = Interlocked.Exchange(ref Disposed, null);
			if (disposed != null)
				disposed(this, EventArgs.Empty);
		}
	}
	
	/*
	public enum CacheMode
	{
		// don't use 0 so that default(CacheToken<...>) is an invalid mode
		Shared = 1,
		ThreadLocal = 2
	}
	
	public struct CacheToken<T> where T : class, new()
	{
		internal readonly CacheMode Mode;
		internal readonly int Index;
		
		internal CacheToken(CacheMode mode, int index)
		{
			this.Mode = mode;
			this.Index = index;
		}
	}*/
}
