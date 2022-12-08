﻿using System.Diagnostics;
using SaladimQBot.Core;
using SaladimQBot.GoCqHttp.Apis;
using SaladimQBot.GoCqHttp.Posts;
using SaladimQBot.Shared;

namespace SaladimQBot.GoCqHttp;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class JoinedGroup : Group, IJoinedGroup, ICqMessageWindow
{
    /// <summary>
    /// 群创建时间
    /// </summary>
    public Expirable<DateTime> CreateTime { get; protected set; } = default!;

    /// <summary>
    /// 群活跃等级
    /// </summary>
    public Expirable<uint> GroupLevel { get; protected set; } = default!;

    /// <summary>
    /// 群成员人数
    /// </summary>
    public Expirable<int> MembersCount { get; protected set; } = default!;

    /// <summary>
    /// 群最大成员数
    /// </summary>
    public Expirable<int> MaxMembersCount { get; protected set; } = default!;

    /// <summary>
    /// 群成员
    /// </summary>
    public Expirable<IReadOnlyList<GroupUser>> Members { get; protected set; } = default!;

    protected Expirable<GetGroupMemberListActionResultData> GetMemberListApiResultData { get; set; } = default!;

    protected internal JoinedGroup(CqClient client, long groupId) : base(client, groupId)
    {
    }

    public Task<GroupMessage> SendMessageAsync(MessageEntity messageEntity)
        => Client.SendGroupMessageAsync(this.GroupId, messageEntity);

    public Task<GroupMessage> SendMessageAsync(string rawString)
        => Client.SendGroupMessageAsync(this.GroupId, rawString);

    #region CreateFrom / LoadFrom集合

    internal static new JoinedGroup CreateFromGroupId(CqClient client, long groupId)
        => new JoinedGroup(client, groupId)
                .LoadFromGroupId(groupId)
                .LoadMemberList(groupId);

    internal static new JoinedGroup CreateFromCqGroupMessagePost(CqClient client, CqGroupMessagePost post)
        => CreateFromGroupId(client, post.GroupId);

    protected internal new JoinedGroup LoadFromGroupId(long groupId)
    {
        var api = new GetGroupInfoAction() { GroupId = groupId };
        var d = ApiCallResultData = Client.MakeExpirableApiCallResultData<GetGroupInfoActionResultData>(api);
        Name = Client.MakeDependencyExpirable(d, d => d.GroupName);
        Remark = Client.MakeDependencyExpirable(d, d => d.GroupMemo);
        CreateTime = Client.MakeDependencyExpirable(d, d => DateTimeHelper.GetFromUnix(d.GroupCreateTime));
        GroupLevel = Client.MakeDependencyExpirable(d, d => d.GroupLevel);
        MembersCount = Client.MakeDependencyExpirable(d, d => d.MemberCount);
        MaxMembersCount = Client.MakeDependencyExpirable(d, d => d.MaxMemberCount);

        this.LoadMemberList(groupId);
        return this;
    }

    protected internal JoinedGroup LoadMemberList(long groupId)
    {
        var getListApi = new GetGroupMemberListAction() { GroupId = groupId };
        var d = GetMemberListApiResultData =
            Client.MakeExpirableApiCallResultData<GetGroupMemberListActionResultData>(getListApi);
        Members = Client.MakeDependencyExpirable(d, DataToList);
        return this;
        IReadOnlyList<GroupUser> DataToList(GetGroupMemberListActionResultData data)
        {
            var rst = from subData in data.DataList
                      let groupUser = GroupUser.CreateFromGroupIdAndUserId(Client, groupId, subData.UserId)
                      select groupUser;
            return rst.ToList();
        }
    }

    #endregion

    #region IJoinedGroup

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    DateTime IJoinedGroup.CreateTime { get => CreateTime.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    uint IJoinedGroup.GroupLevel { get => GroupLevel.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int IJoinedGroup.MembersCount { get => MembersCount.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    int IJoinedGroup.MaxMembersCount { get => MaxMembersCount.Value; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IEnumerable<IGroupUser> IJoinedGroup.Members { get => Members.Value; }

    async Task<IMessage> IMessageWindow.SendMessageAsync(IMessageEntity messageEntity)
        => await Client.SendGroupMessageAsync(GroupId, new MessageEntity(Client, messageEntity));

    async Task<IMessage> IMessageWindow.SendMessageAsync(string rawString)
        => await Client.SendGroupMessageAsync(GroupId, rawString);

    async Task<Message> ICqMessageWindow.SendMessageAsync(MessageEntity messageEntity)
        => await SendMessageAsync(messageEntity);

    async Task<Message> ICqMessageWindow.SendMessageAsync(string rawString)
        => await SendMessageAsync(rawString);

    #endregion

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{Name} ({GroupId})";

    public override bool Equals(object? obj)
    {
        return obj is JoinedGroup group &&
               base.Equals(obj) &&
               this.GroupId == group.GroupId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), this.GroupId);
    }

    public static bool operator ==(JoinedGroup? left, JoinedGroup? right)
    {
        return EqualityComparer<JoinedGroup>.Default.Equals(left!, right!);
    }

    public static bool operator !=(JoinedGroup? left, JoinedGroup? right)
    {
        return !(left == right);
    }
}