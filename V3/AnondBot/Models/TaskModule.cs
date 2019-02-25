using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnondBot.Models
{

    public class TaskModuleResponse
    {
        public TaskResponse Task { get; set; }
        public string ResponseType { get; set; }
    }

    public class TaskResponse
    {
        public Value Value { get; set; }
        public string Type { get; set; }
    }

    public class Value
    {
        public Attachment Card { get; set; }
    }
}