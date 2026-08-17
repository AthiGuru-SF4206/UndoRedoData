using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Provides internal event mechanism.
    /// </summary>
    internal class EventAggregator
    {
        internal IDictionary<string, HashSet<Action<object>>> _eventList = new Dictionary<string, HashSet<Action<object>>>();
        internal IDictionary<string, List<Func<Task, object, Task>>> _eventAsyncList = new Dictionary<string, List<Func<Task, object, Task>>>();

        public bool HasHandler(string name) =>
            name == null ? false : _eventList.TryGetValue(name, out HashSet<Action<object>>? value) && value.Count > 0;

        internal bool HasAsyncHandler(string name) =>
           name == null ? false : _eventAsyncList.TryGetValue(name, out List<Func<Task, object, Task>>? value) && value.Count > 0;

        public void Trigger(string name, object args)
        {
            if (HasHandler(name))
            {
                foreach(Action<object> handler in _eventList[name]) {
                    handler.Invoke(args);
                }
            }
        }
        internal async Task NotifyAsync(string name, object args)
        {
            if (_eventAsyncList.TryGetValue(name, out var handlers))
            {
                var taskToPass = Task.CompletedTask;
                var handlerTasks = handlers.Select(handler => handler(taskToPass, args));
                await Task.WhenAll(handlerTasks).ConfigureAwait(true);
            }
        }
        internal void AddAsync(string name, Func<Task, object, Task> handler)
        {
            if (!_eventAsyncList.TryGetValue(name, out List<Func<Task, object, Task>> ?value))
            {
                _eventAsyncList.Add(name, new List<Func<Task, object, Task>>());
            }

            if (!_eventAsyncList[name].Contains(handler))
            {
                _eventAsyncList[name].Add(handler);
            }
        }

        public void Add(string name, Action<object> handler)
        {
            if (!_eventList.TryGetValue(name, out var handlers))
            {
                handlers = new HashSet<Action<object>>();
                _eventList.Add(name, handlers);
            }

            handlers?.Add(handler);
        }

        public void Remove(string? name = null)
        {
            if (name == null)
            {
                _eventList.Clear();
            }
            else
            {
                _eventList.Remove(name);
            }
        }


        internal void RemoveAsync(string name = null!)
        {
            if (name == null)
            {
                _eventAsyncList.Clear();
            }
            else
            {
                _eventAsyncList.Remove(name);
            }
        }

        public void Remove(string name, Action<object> handler)
        {
            if (_eventList.Any())
            {
                HashSet<Action<object>>? actions = null;
                bool isPresent = _eventList.TryGetValue(name, out actions);
                if (isPresent)
                {
                    actions?.Remove(handler);
                }
            }
        }
        internal void RemoveAsync(string name, Func<Task, object, Task> handler)
        {
            if (_eventAsyncList.Any())
            {
                List<Func<Task, object, Task>> actions = null!;
                bool isPresent = _eventAsyncList.TryGetValue(name, out actions!);
                if (isPresent)
                {
                    actions?.Remove(handler);
                }
            }
        }
    }
}