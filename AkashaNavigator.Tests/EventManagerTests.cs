using System;
using System.Collections.Generic;
using AkashaNavigator.Plugins;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace AkashaNavigator.Tests
{
/// <summary>
/// EventManager 单元测试
/// </summary>
public class EventManagerTests
{
    private readonly EventManager _eventManager;

    public EventManagerTests()
    {
        _eventManager = new EventManager();
    }

#region On() Tests

    /// <summary>
    /// On 应该返回唯一的订阅 ID
    /// </summary>
    [Fact]
    public void On_ShouldReturnUniqueSubscriptionId()
    {
        Action<object> callback1 = (data) =>
        {};
        Action<object> callback2 = (data) =>
        {};
        Action<object> callback3 = (data) =>
        {};

        var id1 = _eventManager.On("event1", callback1);
        var id2 = _eventManager.On("event1", callback2);
        var id3 = _eventManager.On("event2", callback3);

        Assert.True(id1 > 0);
        Assert.True(id2 > 0);
        Assert.True(id3 > 0);
        Assert.NotEqual(id1, id2);
        Assert.NotEqual(id2, id3);
        Assert.NotEqual(id1, id3);
    }

    /// <summary>
    /// On 应该增加监听器计数
    /// </summary>
    [Fact]
    public void On_ShouldIncreaseListenerCount()
    {
        var eventName = "testEvent";

        Assert.Equal(0, _eventManager.GetListenerCount(eventName));

        Action<object> callback1 = (data) =>
        {};
        _eventManager.On(eventName, callback1);
        Assert.Equal(1, _eventManager.GetListenerCount(eventName));

        Action<object> callback2 = (data) =>
        {};
        _eventManager.On(eventName, callback2);
        Assert.Equal(2, _eventManager.GetListenerCount(eventName));
    }

    /// <summary>
    /// On 空事件名称应该返回 -1
    /// </summary>
    [Fact]
    public void On_EmptyEventName_ShouldReturnNegative()
    {
        Action<object> callback = (data) =>
        {};
        Assert.Equal(-1, _eventManager.On("", callback));
        Assert.Equal(-1, _eventManager.On("  ", callback));
    }

    /// <summary>
    /// On 空回调应该返回 -1
    /// </summary>
    [Fact]
    public void On_NullCallback_ShouldReturnNegative()
    {
        Assert.Equal(-1, _eventManager.On("testEvent", null!));
    }

#endregion

#region Off(subscriptionId) Tests

    /// <summary>
    /// Off 使用订阅 ID 应该移除正确的监听器
    /// </summary>
    [Fact]
    public void Off_WithSubscriptionId_ShouldRemoveCorrectListener()
    {
        var callCount1 = 0;
        var callCount2 = 0;

        Action<object> callback1 = (data) => callCount1++;
        Action<object> callback2 = (data) => callCount2++;

        var id1 = _eventManager.On("event", callback1);
        var id2 = _eventManager.On("event", callback2);

        Assert.Equal(2, _eventManager.GetListenerCount("event"));

        // 移除第一个监听器
        var result = _eventManager.Off(id1);
        Assert.True(result);
        Assert.Equal(1, _eventManager.GetListenerCount("event"));

        // 触发事件，只有第二个回调应该被调用
        _eventManager.Emit("event", new {});
        Assert.Equal(0, callCount1);
        Assert.Equal(1, callCount2);
    }

    /// <summary>
    /// Off 使用无效订阅 ID 应该返回 false
    /// </summary>
    [Fact]
    public void Off_WithInvalidSubscriptionId_ShouldReturnFalse()
    {
        Assert.False(_eventManager.Off(-1));
        Assert.False(_eventManager.Off(0));
        Assert.False(_eventManager.Off(999));
    }

    /// <summary>
    /// Off 重复移除同一订阅 ID 应该返回 false
    /// </summary>
    [Fact]
    public void Off_DuplicateRemoval_ShouldReturnFalse()
    {
        Action<object> callback = (data) =>
        {};
        var id = _eventManager.On("event", callback);

        Assert.True(_eventManager.Off(id));
        Assert.False(_eventManager.Off(id));
    }

#endregion

#region Off(eventName) Tests

    /// <summary>
    /// Off 使用事件名称应该移除该事件的所有监听器
    /// </summary>
    [Fact]
    public void Off_WithEventName_ShouldRemoveAllListeners()
    {
        Action<object> callback1 = (data) =>
        {};
        Action<object> callback2 = (data) =>
        {};
        Action<object> callback3 = (data) =>
        {};

        _eventManager.On("event", callback1);
        _eventManager.On("event", callback2);
        _eventManager.On("event", callback3);

        Assert.Equal(3, _eventManager.GetListenerCount("event"));

        _eventManager.Off("event");

        Assert.Equal(0, _eventManager.GetListenerCount("event"));
    }

    /// <summary>
    /// Off 使用事件名称不应该影响其他事件
    /// </summary>
    [Fact]
    public void Off_WithEventName_ShouldNotAffectOtherEvents()
    {
        Action<object> callback1 = (data) =>
        {};
        Action<object> callback2 = (data) =>
        {};

        _eventManager.On("event1", callback1);
        _eventManager.On("event2", callback2);

        _eventManager.Off("event1");

        Assert.Equal(0, _eventManager.GetListenerCount("event1"));
        Assert.Equal(1, _eventManager.GetListenerCount("event2"));
    }

#endregion

#region Emit() Tests

    /// <summary>
    /// Emit 应该调用所有注册的回调
    /// </summary>
    [Fact]
    public void Emit_ShouldCallAllCallbacks()
    {
        var callCount1 = 0;
        var callCount2 = 0;
        var callCount3 = 0;

        Action<object> callback1 = (data) => callCount1++;
        Action<object> callback2 = (data) => callCount2++;
        Action<object> callback3 = (data) => callCount3++;

        _eventManager.On("event", callback1);
        _eventManager.On("event", callback2);
        _eventManager.On("event", callback3);

        _eventManager.Emit("event", new {});

        Assert.Equal(1, callCount1);
        Assert.Equal(1, callCount2);
        Assert.Equal(1, callCount3);
    }

    /// <summary>
    /// Emit 应该传递正确的数据
    /// </summary>
    [Fact]
    public void Emit_ShouldPassCorrectData()
    {
        object? receivedData = null;

        Action<object> callback = (data) => receivedData = data;
        _eventManager.On("event", callback);

        var testData = new { value = 42, name = "test" };
        _eventManager.Emit("event", testData);

        Assert.Same(testData, receivedData);
    }

    /// <summary>
    /// Emit 回调异常不应该影响其他回调
    /// </summary>
    [Fact]
    public void Emit_CallbackException_ShouldNotAffectOthers()
    {
        var callCount = 0;

        Action<object> throwingCallback = (data) => throw new Exception("Test exception");
        Action<object> normalCallback = (data) => callCount++;

        _eventManager.On("event", throwingCallback);
        _eventManager.On("event", normalCallback);

        // 不应该抛出异常
        _eventManager.Emit("event", new {});

        // 第二个回调应该被调用
        Assert.Equal(1, callCount);
    }

    /// <summary>
    /// Emit 对不存在的事件不应该抛出异常
    /// </summary>
    [Fact]
    public void Emit_NonExistentEvent_ShouldNotThrow()
    {
        // 不应该抛出异常
        _eventManager.Emit("nonExistentEvent", new {});
    }

    /// <summary>
    /// Emit 空事件名称不应该抛出异常
    /// </summary>
    [Fact]
    public void Emit_EmptyEventName_ShouldNotThrow()
    {
        _eventManager.Emit("", new {});
        _eventManager.Emit("  ", new {});
    }

#endregion

#region Clear() Tests

    /// <summary>
    /// Clear 应该移除所有监听器
    /// </summary>
    [Fact]
    public void Clear_ShouldRemoveAllListeners()
    {
        Action<object> callback1 = (data) =>
        {};
        Action<object> callback2 = (data) =>
        {};
        Action<object> callback3 = (data) =>
        {};

        _eventManager.On("event1", callback1);
        _eventManager.On("event2", callback2);
        _eventManager.On("event3", callback3);

        Assert.Equal(3, _eventManager.GetTotalListenerCount());

        _eventManager.Clear();

        Assert.Equal(0, _eventManager.GetTotalListenerCount());
        Assert.Equal(0, _eventManager.GetListenerCount("event1"));
        Assert.Equal(0, _eventManager.GetListenerCount("event2"));
        Assert.Equal(0, _eventManager.GetListenerCount("event3"));
    }

#endregion

#region HasSubscription() Tests

    /// <summary>
    /// HasSubscription 应该正确检测订阅状态
    /// </summary>
    [Fact]
    public void HasSubscription_ShouldDetectCorrectly()
    {
        Action<object> callback = (data) =>
        {};
        var id = _eventManager.On("event", callback);

        Assert.True(_eventManager.HasSubscription(id));
        Assert.False(_eventManager.HasSubscription(id + 1));

        _eventManager.Off(id);

        Assert.False(_eventManager.HasSubscription(id));
    }

#endregion

#region Case Insensitivity Tests

    /// <summary>
    /// 事件名称应该不区分大小写
    /// </summary>
    [Fact]
    public void EventName_ShouldBeCaseInsensitive()
    {
        var callCount = 0;

        Action<object> callback = (data) => callCount++;
        _eventManager.On("TestEvent", callback);
        _eventManager.Emit("testevent", new {});

        Assert.Equal(1, callCount);
        Assert.Equal(1, _eventManager.GetListenerCount("TESTEVENT"));
    }

#endregion
}
}
