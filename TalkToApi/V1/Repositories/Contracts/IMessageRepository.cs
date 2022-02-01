using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkToApi.V1.Models;

namespace TalkToApi.V1.Repositories.Contracts
{
    public interface IMessageRepository
    {
        List<Message> GetMessages(string userIdOne, string userIdTwo);

        void Add(Message message);


    }
}
