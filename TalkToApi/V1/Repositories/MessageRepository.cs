using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.Repositories.Context;
using TalkToApi.V1.Models;
using TalkToApi.V1.Repositories.Contracts;

namespace TalkToApi.V1.Repositories
{
    public class MessageRepository : IMessageRepository
    {

        private readonly TalkToApiContext _banco;
        public MessageRepository(TalkToApiContext banco)
        {
            _banco = banco;
        }

        public List<Message> GetMessages(string userIdOne, string userIdTwo)
        {
            return _banco.Message.Where(a => (a.FromId == userIdOne || a.FromId == userIdTwo) && (a.FromId == userIdTwo || a.ToId == userIdTwo)).ToList();
        }

        public void Add(Message message)
        {
            _banco.Message.Add(message);
            _banco.SaveChanges();
        }

        
    }
}
