using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Update Manager Assessment - Midnight  14/10/2025
/// More info Coming Soon
/// STANDALONE: Does not require any other scripts.
/// NOTE: PLEASE DO NOT KILL OR DESTROY OBJECTS WITHOUT UNSUBSCRIBING IF they are using this if you do this script DOES NOT check.
/// Updated: Midnight - 05/01/2025 (Performence Update), Midnight - 08/01/2025 (Fetal Flaw removal) 
/// </summary>

public class UpdateManager : MonoBehaviour
{
    
    [Serializable]
    public struct SubscriptionToken
    {
        public readonly UpdateType Type;
        public readonly int Index;
        private readonly Action _action; // Store reference to validate that this isn't another method or is that method of course now does need a Action to check it with though.
        
        public SubscriptionToken(UpdateType type, int index, Action action)
        {
            Type = type;
            Index = index;
            _action = action;
        }
        
        /// <summary>
        /// Checks if this token's action matches the given action.
        /// </summary>
        public bool Matches(Action action) => _action == action;
    }

    internal static UpdateManager Instance { get; private set; }
    
    [SerializeField] private int initialCapacity = 256;
    
    private readonly List<Action>[] _subs = new List<Action>[3]; 
    // I didn't actually Know this but adding the [3] means there are 3 Lists 0 = Update, 1 = Fixed, 2 = Late.
    // Which this method is insanely fast, for what this doing It needs to be.
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            name = nameof(UpdateManager);
            for (var i = 0; i < 3; i++) // Initialize all 3 update lists.
            {
                _subs[i] = new List<Action>(initialCapacity);
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
        if (action == null) return new SubscriptionToken(type, -1, null);
        var list = _subs[(int)type];
        var index = list.Count;
        list.Add(action);
        return new SubscriptionToken(type, index, action);
    }

    /// <summary>
    /// Subscribes to the update manager and returns true if the action was added.
    /// This method doesn't allow duplicate actions.
    /// it out puts SubscriptionToken so you can remove it if needed. - I BONK MYSELF FOR THIS SHOULD HAVE BEEN HERE FROM THE SATRT. 
    /// </summary>
    public bool TrySubscribe(UpdateType type, Action action, out SubscriptionToken? token)
    {
        token = null;
        if (action == null) return false;
        var list = _subs[(int)type];
        if (list.Contains(action)) return false;
        var index = list.Count;
        list.Add(action);
        token = new SubscriptionToken(type, index, action);
        return true;
    }

    /// <summary>
    /// Unsubscribes to the update manager and returns true if the action was removed.
    /// </summary>
    public bool Unsubscribe(UpdateType type, Action action)
    {
        if (action == null) return false;
        var list = _subs[(int)type];
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
    /// Now validates that the token's action matches what's at the index.
    /// </summary>
    public bool Unsubscribe(SubscriptionToken token)
    {
        if (token.Index < 0) return false;
        var list = _subs[(int)token.Type];
        if (token.Index >= list.Count) return false;
        
        var action = list[token.Index];
        if (action == null || !token.Matches(action)) 
            return false; // Token is stale or invalid!
        
        var last = list.Count - 1;
        if (token.Index != last) list[token.Index] = list[last];
        list.RemoveAt(last);
        return true;
    }
    
    /// <summary>
    /// Returns true if the token is valid.
    /// Now validates that the action at the index matches the token's action.
    /// </summary>
    public bool IsValid(SubscriptionToken token)
    {
        if (token.Index < 0) return false;
        var list = _subs[(int)token.Type];
        if (token.Index >= list.Count) return false;
        
        var action = list[token.Index];
        return action != null && token.Matches(action);
    }

    /// <summary>
    /// Unsubscribes all subscribers of the given update type.
    /// </summary>
    public void ClearSubscribers(UpdateType type) => _subs[(int)type].Clear();

    /// <summary>
    /// Unsubscribes all subscribers from all update types.
    /// </summary>
    private void ClearAllSubscribers()
    {
        for (var i = 0; i < _subs.Length; i++)
        {
            _subs[i].Clear();
        }
    }

    /// <summary>
    /// Gets Subscriber Count for the specific Update Type.
    /// </summary>
    public int GetSubscriberCount(UpdateType type) => _subs[(int)type].Count;

    #region  DO NOT TOUCH
    /// <summary>
    /// Okay you can Touch these but keep in mind they are ran from
    /// </summary>
    private void Update() => InvokeSubscribers(UpdateType.Update);
    private void FixedUpdate() => InvokeSubscribers(UpdateType.FixedUpdate);
    private void LateUpdate() => InvokeSubscribers(UpdateType.LateUpdate);
    #endregion
    
    private void InvokeSubscribers(UpdateType type) // Reduced Over head and faster, but less safe.
    {
        var list = _subs[(int)type];
        var count = list.Count;
        if (count == 0) return;
        for (var i = 0; i < count; i++)
        {
            list[i]();
        }
    }
}

public enum UpdateType
{
    Update,
    FixedUpdate,
    LateUpdate
}
