using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Update Manager Assessment - Midnight  14/10/2025
/// More info Coming Soon
/// STANDALONE: Does not require any other scripts.
/// </summary>

public class UpdateManager : MonoBehaviour
{
    public enum UpdateType
    {
        Update,
        FixedUpdate,
        LateUpdate
    }
    
    [Serializable]
    public struct SubscriptionToken
    {
        public readonly UpdateType Type;
        public readonly int Index;
        public SubscriptionToken(UpdateType type, int index)
        {
            Type = type;
            Index = index;
        }
    }

    internal static UpdateManager Instance { get; private set; }
    
    [SerializeField] private int initialCapacity = 256;
    
    private readonly Dictionary<UpdateType, List<Action>> _subs = new Dictionary<UpdateType, List<Action>>();
    
    private static Action[] _buffer = new Action[256];
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            name = nameof(UpdateManager);
            
            foreach (UpdateType type in Enum.GetValues(typeof(UpdateType)))
            {
                _subs[type] = new List<Action>(initialCapacity);
            }
        }
        else if (Instance != this) Destroy(gameObject);
    }
    
    private void OnDestroy()
    {
        if (Instance != this) return;
        ClearAllSubscribers();
        Instance = null;
    }
    
    /// <summary>
    /// Subscribes to the update manager.
    /// This method allows duplicates.
    /// </summary>
    public SubscriptionToken Subscribe(UpdateType type, Action action)
    {
        if (action == null) return new SubscriptionToken(type, -1);
        var list = _subs[type];
        var index = list.Count;
        list.Add(action);
        return new SubscriptionToken(type, index);
    }

    /// <summary>
    /// Subscribes to the update manager and returns true if the action was added.
    /// This method doesn't allow duplicates.
    /// </summary>
    public bool TrySubscribe(UpdateType type, Action action)
    {
        if (action == null) return false;
        var list = _subs[type];
        if (list.Contains(action)) return false;
        list.Add(action);
        return true;
    }

    /// <summary>
    /// Unsubscribes to the update manager and returns true if the action was removed.
    /// </summary>
    public bool Unsubscribe(UpdateType type, Action action)
    {
        if (action == null) return false;
        var list = _subs[type];
        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] != action) continue;
            var last = list.Count - 1;
            if (i != last) list[i] = list[last];
            list.RemoveAt(last);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Unsubscribes using the provided token and returns true if removed.
    /// Tokens can become invalid if other unsubscribes happened earlier.
    /// </summary>
    public bool Unsubscribe(SubscriptionToken token)
    {
        if (token.Index < 0) return false;
        var list = _subs[token.Type];
        if (token.Index >= list.Count || list[token.Index] == null) return false;
        
        var last = list.Count - 1;
        if (token.Index != last)
        {
            list[token.Index] = list[last];
        }
        list.RemoveAt(last);
        return true;
    }
    
    /// <summary>
    /// Returns true if the token is valid.
    /// </summary>
    public bool IsValid(SubscriptionToken token)
    {
        if (token.Index < 0) return false;
        var list = _subs[token.Type];
        return token.Index < list.Count && list[token.Index] != null;
    }

    /// <summary>
    /// Unsubscribes all subscribers of the given update type.
    /// </summary>
    public void ClearSubscribers(UpdateType type)
    {
        _subs[type].Clear();
    }

    /// <summary>
    /// Unsubscribes all subscribers from all update types.
    /// </summary>
    private void ClearAllSubscribers()
    {
        foreach (var list in _subs.Values)
        {
            list.Clear();
        }
    }

    public int GetSubscriberCount(UpdateType type)
    {
        return _subs[type].Count;
    }
    
    private void Update()
    {
        InvokeSubscribers(UpdateType.Update);
    }
    
    private void FixedUpdate()
    {
        InvokeSubscribers(UpdateType.FixedUpdate);
    }
    
    private void LateUpdate()
    {
        InvokeSubscribers(UpdateType.LateUpdate);
    }
    
    private void InvokeSubscribers(UpdateType type)
    {
        var list = _subs[type];
        var count = list.Count;
        if (count == 0) return;
        
        if (_buffer.Length < count)
            Array.Resize(ref _buffer, count * 2);
        
        list.CopyTo(_buffer, 0);
        
        for (var i = 0; i < count; i++)
        {
            var action = _buffer[i];
            if (action == null) continue;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UpdateManager] Exception in {type} subscriber: {ex}");
            }
        }
        
        var writeIndex = 0;
        for (var readIndex = 0; readIndex < count; readIndex++)
        {
            var action = list[readIndex];
            if (action == null) continue;
            list[writeIndex++] = action;
        }
        if (writeIndex < count)
            list.RemoveRange(writeIndex, count - writeIndex);
    }
}
