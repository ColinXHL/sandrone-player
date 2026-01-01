using System;
using System.Collections.Generic;
using System.Linq;

namespace AkashaNavigator.Core.Events
{
    /// <summary>
    /// 事件总线实现，用于组件间的解耦通信
    /// 线程安全：发布事件时会锁定特定事件类型的处理器列表
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();
        private readonly object _lock = new();

        public void Publish<TEvent>(TEvent @event) where TEvent : class
        {
            if (@event == null)
                return;

            var eventType = typeof(TEvent);
            List<Delegate> handlers;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventType, out var handlersList))
                    return;

                // 创建副本以避免在迭代时修改集合
                handlers = handlersList.ToList();
            }

            foreach (var handler in handlers)
            {
                try
                {
                    handler.DynamicInvoke(@event);
                }
                catch (Exception ex)
                {
                    // 记录异常但不中断其他处理器的执行
                    System.Diagnostics.Debug.WriteLine($"EventBus handler threw exception: {ex.Message}");
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var eventType = typeof(TEvent);

            lock (_lock)
            {
                if (!_handlers.TryGetValue(eventType, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _handlers[eventType] = handlers;
                }

                // 避免重复订阅
                if (!handlers.Contains(handler))
                    handlers.Add(handler);
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null)
                return;

            var eventType = typeof(TEvent);

            lock (_lock)
            {
                if (_handlers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);

                    // 如果没有订阅者了，移除条目
                    if (handlers.Count == 0)
                        _handlers.Remove(eventType);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }
    }
}
