using System;

namespace AkashaNavigator.Core.Events
{
    /// <summary>
    /// 事件总线接口，用于组件间的解耦通信
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 发布事件到所有订阅者
        /// </summary>
        void Publish<TEvent>(TEvent @event) where TEvent : class;

        /// <summary>
        /// 订阅事件
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        /// <summary>
        /// 清除所有订阅（用于测试或应用关闭时）
        /// </summary>
        void Clear();
    }
}
