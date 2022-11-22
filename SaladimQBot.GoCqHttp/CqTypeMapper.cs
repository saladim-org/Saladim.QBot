﻿using SaladimQBot.GoCqHttp.Posts;

namespace SaladimQBot.GoCqHttp;

internal static class CqTypeMapper
{
    public static Type? FindClassForCqCodeType(CqCodeType cqCode) => cqCode switch
    {
        CqCodeType.Text => typeof(CqMessageTextNode),
        CqCodeType.At => typeof(CqMessageAtNode),
        CqCodeType.Image => typeof(CqMessageImageReceiveNode),
        CqCodeType.Reply => typeof(CqMessageReplyIdNode),
        CqCodeType.Face => typeof(CqMessageFaceNode),
        CqCodeType.Record or
        CqCodeType.Unimplemented or
        CqCodeType.Invalid or
        _ => typeof(CqMessageUnimplementedNode),
    };

    #region 上报四类型

    public static Type? FindCalssForPostType(CqPostType postType) => postType switch
    {
        CqPostType.Message => typeof(CqMessagePost),
        CqPostType.Request => null,
        CqPostType.Notice => typeof(CqNoticePost),
        CqPostType.MetaEvent => typeof(CqMetaEventPost),
        _ => null
    };

    public static Type? FindClassForCqNoticeType(CqNoticeType noticeType) => noticeType switch
    {
        CqNoticeType.GroupFileUploaded => typeof(CqGroupFileUploadedNoticePost),
        CqNoticeType.GroupAdminChanged => typeof(CqGroupAdminChangedNoticePost),
        CqNoticeType.GroupMemberDecreased => typeof(CqGroupMemberDecreaseNoticePost),
        CqNoticeType.GroupMemberIncreased => typeof(CqGroupMemberIncreaseNoticePost),
        CqNoticeType.GroupMemberBanned => typeof(CqGroupMemberBannedNoticePost),
        CqNoticeType.GroupMemberCardChanged => typeof(CqGroupMemberCardChangedNoticePost),
        CqNoticeType.GroupMessageRecalled => typeof(CqGroupMessageRecalledNoticePost),
        CqNoticeType.FriendRecalled => typeof(CqFriendMessageRecalledNoticePost),
        CqNoticeType.FriendAdded => typeof(CqFriendAddedNoticePost),
        CqNoticeType.SystemNotice => typeof(CqNotifyNoticePost),
        CqNoticeType.OfflineFileUploaded => typeof(CqOfflineFileUploadedNoticePost),
        CqNoticeType.GroupEssenceSet => typeof(CqGroupEssenceSetNoticePost),
        _ => null,
    };

    public static Type? FindClassForCqMessagePostType(CqMessageSubType messageSubType) => messageSubType switch
    {
        CqMessageSubType.TempFromGroup or
        CqMessageSubType.Friend => typeof(CqPrivateMessagePost),
        CqMessageSubType.Group or
        CqMessageSubType.Anonymous => typeof(CqGroupMessagePost),
        CqMessageSubType.Other => typeof(CqOtherMessagePost),
        CqMessageSubType.GroupFromSelf => throw new NotSupportedException(),
        _ => null
    };

    public static Type? FindCalssForCqNotifyNoticePostType(CqNotifySubType notifyType) => notifyType switch
    {
        CqNotifySubType.GroupHonorChanged => typeof(CqGroupHonorChangedNotifyNoticePost),
        CqNotifySubType.Poke => typeof(CqPokeNotifyNoticePost),
        CqNotifySubType.LuckyKing => typeof(CqLuckyKingNotifyNoticePost),
        CqNotifySubType.Title => typeof(CqTitleChangedNotifyNoticePost),
        _ => null
    };

    #endregion
}