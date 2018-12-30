using System;
using System.Collections.Generic;
using System.Text;

namespace Bit4j.Lambda.Core.Model
{
    public class UserAnswer
    {
        public string UserUUID { get; set; }

        public string Answer { get; set; }

        public string SelectedOption { get; set; }

        public string QuestionUUID { get; set; }
    }
}
