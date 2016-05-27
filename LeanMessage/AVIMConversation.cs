﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeanMessage.Internal;
using LeanCloud;
using LeanCloud.Internal;

namespace LeanMessage
{
    public class AVIMConversation
    {

        private DateTime? updatedAt;

        private DateTime? createdAt;

        internal readonly Object mutex = new Object();

        internal AVIMClient _currentClient;
        /// <summary>
        /// 当前的AVIMClient，一个对话理论上只存在一个AVIMClient。
        /// </summary>
        public AVIMClient CurrentClient
        {
            get
            {
                return _currentClient;
            }
            set
            {
                _currentClient = value;
            }
        }
        /// <summary>
        /// 对话的唯一ID
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// 对话在全局的唯一的名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 对话中存在的 Client 的 Id 列表
        /// </summary>
        public IList<string> MemberIds { get; set; }

        /// <summary>
        /// 对该对话静音的成员列表
        /// <remarks>
        /// 对该对话设置了静音的成员，将不会收到离线消息的推送。
        /// </remarks>
        /// </summary>
        public IList<string> MuteMemberIds { get; set; }

        /// <summary>
        /// 对话的创建者
        /// </summary>
        public string Creator { get; private set; }

        /// <summary>
        /// 是否为聊天室
        /// </summary>
        public bool IsTransient { get; set; }

        /// <summary>
        /// 对话创建的时间
        /// </summary>
        public DateTime? CreatedAt
        {
            get
            {
                DateTime? nullable;
                lock (this.mutex)
                {
                    nullable = this.createdAt;
                }
                return nullable;
            }
            private set
            {
                lock (this.mutex)
                {
                    this.createdAt = value;
                }
            }
        }

        /// <summary>
        /// 对话更新的时间
        /// </summary>
        public DateTime? UpdatedAt
        {
            get
            {
                DateTime? nullable;
                lock (this.mutex)
                {
                    nullable = this.updatedAt;
                }
                return nullable;
            }
            private set
            {
                lock (this.mutex)
                {
                    this.updatedAt = value;
                }
            }
        }
        /// <summary>
        /// 对话的自定义属性
        /// </summary>
        public IDictionary<string, object> Attributes
        {
            get
            {
                return fetchedAttributes.Merge(pendingAttributes);
            }
            private set
            {
                Attributes = value;
            }
        }
        internal IDictionary<string, object> fetchedAttributes;
        internal IDictionary<string, object> pendingAttributes;

        /// <summary>
        /// 创建一个默认的 AVIMConversation 对象
        /// </summary>
        public AVIMConversation()
        {

        }

        /// <summary>
        /// AVIMConversation Build 驱动器
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="members"></param>
        /// <param name="isTransient"></param>
        /// <param name="attributes"></param>
        internal AVIMConversation(AVIMConversation source = null,
            string name = null,
            IList<string> members = null,
            bool isTransient = false,
            IDictionary<string, object> attributes = null)
        {
            this.Name = source.Name;
            this.MemberIds = source.MemberIds;
            this.IsTransient = source.IsTransient;
            this.Attributes = source.Attributes;

            if (string.IsNullOrEmpty(name))
            {
                this.Name = name;
            }
            if (members != null)
            {
                this.MemberIds = members;
            }
            this.IsTransient = isTransient;
            if (attributes != null)
            {
                this.Attributes = attributes;
            }
        }

        ///// <summary>
        ///// 向该对话发送普通的文本消息。
        ///// </summary>
        ///// <param name="textContent">文本消息的内容，一般就是一个不超过5KB的字符串。</param>
        ///// <returns></returns>
        //public Task<Tuple<bool, AVIMTextMessage>> SendTextMessageAsync(AVIMTextMessage textMessage)
        //{
        //    return SendMessageAsync<AVIMTextMessage>(textMessage);
        //}

        /// <summary>
        /// 向该对话发送消息。
        /// </summary>
        /// <param name="avMessage"></param>
        /// <returns></returns>
        public Task<AVIMMessage> SendMessageAsync(AVIMMessage avMessage)
        {
            if (this.CurrentClient == null) throw new Exception("当前对话未指定有效 AVIMClient，无法发送消息。");
            if (this.CurrentClient.State != AVIMClient.Status.Connecting) throw new Exception("未能连接到服务器，无法发送消息。");
            return this.CurrentClient.SendMessageAsync(this, avMessage);
        }

        /// <summary>
        /// 从本地构建一个对话
        /// </summary>
        /// <param name="convId">对话的 objectId</param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static AVIMConversation CreateWithoutData(string convId, AVIMClient client)
        {
            return new AVIMConversation()
            {
                ConversationId = convId,
                CurrentClient = client
            };
        }

        /// <summary>
        /// 设置自定义属性
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Attribute(string key, object value)
        {
            if (pendingAttributes == null)
            {
                pendingAttributes = new Dictionary<string, object>();
            }
            pendingAttributes[key] = value;
        }


        #region 成员操作相关接口
        /// <summary>
        /// CurrentClient 主动加入到对话中
        /// <para>签名操作</para>
        /// </summary>
        /// <returns></returns>
        public Task<bool> JoinAsync()
        {
            return AddMembersAsync(CurrentClient.clientId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public Task<bool> AddMembersAsync(string clientId)
        {
            var cmd = new ConversationCommand()
                .ConId(this.ConversationId)
                .Member(clientId)
                .Option("add")
                .AppId(AVClient.ApplicationId)
                .PeerId(clientId);

            return AVIMClient.AVCommandRunner.RunCommandAsync(cmd).OnSuccess(_ =>
            {
                return _.Result.Item2.ContainsKey("added");
            });
        }

        #endregion
    }
}
