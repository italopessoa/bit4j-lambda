using Neo4j.Map.Extension.Attributes;
using Neo4j.Map.Extension.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Bit4j.Lambda.Core.Model.Nodes
{
    [Neo4jLabel("Question")]
    public class QuestionNode : Neo4jNode
    {
        private string _title;
        private string _title_pt;
        private string _type;
        private object _correctAnswer;
        private object _correctAnswer_pt;
        private string _category;
        private string[] _incorrectAnswers;

        [JsonProperty("category")]
        [Neo4jProperty(Name = "category")]
        public string Category
        {
            get { return _category; }
            set
            {
                _category = HttpUtility.UrlDecode(value).Trim();
            }
        }

        [JsonProperty("type")]
        [Neo4jProperty(Name = "type")]
        public string Type { get; set; }

        [JsonProperty("difficulty")]
        [Neo4jProperty(Name = "difficulty")]
        public string Difficulty { get; set; }

        [JsonProperty("question")]
        [Neo4jProperty(Name = "title")]
        public string Title
        {
            get { return _title; }
            set
            {
                _title = HttpUtility.UrlDecode(value).Trim();
                _title_pt = _title;
            }
        }

        [JsonProperty("question_pt")]
        [Neo4jProperty(Name = "title_pt")]
        public string TitlePT
        {
            get { return _title_pt; }
            set
            {
                _title_pt = HttpUtility.UrlDecode(value).Trim();
            }
        }

        [JsonProperty("correct_answer")]
        [Neo4jProperty(Name = "correct_answer")]
        public object CorrectAnswer
        {
            get { return _correctAnswer; }
            set
            {
                if (value.GetType() == typeof(long))
                {
                    _correctAnswer = value;
                }
                else
                {
                    string aux = HttpUtility.UrlDecode(value.ToString().Replace("'", "\'")).Trim();
                    _correctAnswer = aux;
                    _correctAnswer_pt = aux;
                }
            }
        }

        [JsonProperty("correct_answer_pt")]
        [Neo4jProperty(Name = "correct_answer_pt")]
        public object CorrectAnswerPT
        {
            get { return _correctAnswer_pt; }
            set
            {
                if (value.GetType() == typeof(long))
                {
                    _correctAnswer_pt = value;
                }
                else
                {
                    string aux = HttpUtility.UrlDecode(value.ToString().Replace("'", "\'")).Trim();
                    _correctAnswer_pt = aux;
                }
            }
        }

        [JsonProperty("incorrect_answers")]
        [Neo4jProperty(Name = "incorrect_answers")]
        public string[] IncorrectAnswers
        {
            get { return _incorrectAnswers; }
            set
            {
                _incorrectAnswers = new string[value.Length];

                for (int i = 0; i < value.Length; i++)
                {
                    _incorrectAnswers[i] = HttpUtility.UrlDecode(value[i]).Trim();
                    //_incorrectAnswers[i] = _incorrectAnswers[i].Replace("'", "\'");
                }
            }
        }

        public override string ToString()
        {
            return $"Person {{UUID: '{UUID}', Title: '{Title}'}}";
        }
    }
}