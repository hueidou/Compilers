using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryLibrary
{
    public class Enquerying
    {
        protected Enquerying()
        { }

        protected Enquerying(int queryPage)
        {
            this._queryPage = queryPage;
        }

        private int _queryPage = 1;
        public virtual int QueryPage { get { return _queryPage; } }

        public static Enquerying Default { get { return new Enquerying(); } }

        public static Enquerying BAORPN { get { return new Enquerying(0); } }

        public static Enquerying BAO { get { return new Enquerying(1); } }

        public static Enquerying ISee { get { return new Enquerying(2); } }

        public static Enquerying SCIInfo { get { return new Enquerying(3); } }

        public static string Convert(Enquerying srcEnQuerying, Enquerying dstEnQuerying, string queryString)
        {
            // 作者 = '张三' AND (( 标题 = '钢铁' OR 标题 = '冷轧' ) AND ( 关键词 like '钢铁' AND 关键词 like '冷轧' ))

            // 关系运算符 Relation Operator
            // 关系表达式 Relation Expression

            // 逻辑运算符 Logic Operator
            // 逻辑表达式 Logic Expression
            // () > NOT > AND > OR

            // 分解逻辑表达式为，关系表达式 和 逻辑运算符

            queryString = queryString.Trim();
            RelationExpression relationExpression = new RelationExpression();


            int status = 0;
            List<LogicElement> logicElements = new List<LogicElement>();
            LogicElement logicOperator = new LogicElement { Type = "LO", Element = new StringBuilder() };

            for (int i = 0; i < queryString.Length; i++)
            {
                char c = queryString[i];

                switch (status)
                {
                    case 0:
                        if (c == '(')
                        {
                            status = 1;

                            // 逻辑运算符
                            logicElements.Add(new LogicElement { Type = "LO", Element = "(" });
                        }
                        else
                        {
                            status = 3;
                            relationExpression.Field.Append(c);
                        }
                        break;
                    case 1:
                        if (c == '(')
                        {
                            status = 1;

                            // 逻辑运算符
                            logicElements.Add(new LogicElement { Type = "LO", Element = "(" });
                        }
                        else if (c == ' ')
                        {
                            status = 2;
                        }
                        //else 
                        //{
                        //    status = 3;
                        //    relationExpression.Field.Append(c);
                        //}
                        break;
                    case 2:
                        if (c == ' ')
                        {
                            status = 2;
                        }
                        else
                        {
                            status = 3;
                            relationExpression.Field.Append(c);
                        }
                        break;
                    case 3:
                        if (c == ' ')
                        {
                            status = 4;
                        }
                        else
                        {
                            status = 3;
                            relationExpression.Field.Append(c);
                        }
                        break;
                    case 4:
                        if (c == ' ')
                        {
                            status = 4;
                        }
                        else
                        {
                            status = 5;
                            relationExpression.RelationOperator.Append(c);
                        }
                        break;
                    case 5:
                        if (c == ' ')
                        {
                            status = 6;
                        }
                        else
                        {
                            status = 5;
                            relationExpression.RelationOperator.Append(c);
                        }
                        break;
                    case 6:
                        if (c == ' ')
                        {
                            status = 6;
                        }
                        else
                        {
                            status = 7;
                            relationExpression.Value.Append(c);
                        }
                        break;
                    case 7:
                        if (c == ' ')
                        {
                            status = 8;

                            // 此处关系表达式结束，
                            logicElements.Add(new LogicElement { Type = "RE", Element = relationExpression.Copy() });
                            relationExpression = new RelationExpression();
                        }
                        else
                        {
                            status = 7;
                            relationExpression.Value.Append(c);
                        }
                        break;
                    case 8:
                        if (c == ' ')
                        {
                            status = 8;
                        }
                        else if (c == ')')
                        {
                            status = 10;

                            // 逻辑运算符
                            logicElements.Add(new LogicElement { Type = "LO", Element = ")" });
                        }
                        else
                        {
                            status = 9;
                            (logicOperator.Element as StringBuilder).Append(c);
                        }
                        break;
                    case 9:
                        if (c == ' ')
                        {
                            status = 12;
                            logicElements.Add(logicOperator);
                            logicOperator = new LogicElement { Type = "LO", Element = new StringBuilder() };
                        }
                        else
                        {
                            status = 9;
                            (logicOperator.Element as StringBuilder).Append(c);
                        }
                        break;
                    case 10:
                        if (c == ')')
                        {
                            status = 10;

                            // 逻辑运算符
                            logicElements.Add(new LogicElement { Type = "LO", Element = ")" });
                        }
                        else if (c == ' ')
                        {
                            status = 11;
                        }
                        break;
                    case 11:
                        if (c == ' ')
                        {
                            status = 11;
                        }
                        else
                        {
                            status = 9;
                            (logicOperator.Element as StringBuilder).Append(c);
                        }
                        break;
                    case 12:
                        if (c == ' ')
                        {
                            status = 12;
                        }
                        else
                        {
                            status = 0;
                            i--;
                        }
                        break;
                }
            }

            // 调度场算法，将中缀表达式转换为后缀表达式
            logicElements = ShuntingYard(logicElements);
            logicElements = SCIInfoConvert(logicElements);

            // 作者 = '张三' AND (( 标题 = '钢铁' OR 标题 = '冷轧' ) AND 关键词 like '钢铁' ))
            // 作者:("张三") 标题:("钢铁") 标题:("冷轧") + 关键词:(钢铁) * *
            // 标题:("钢铁") + 标题:("冷轧") * 关键词:(钢铁) * 作者:("张三")
            string str = string.Join(" ", logicElements.ConvertAll(l => l.ToString()).ToArray());

            for (int i = 0; i < logicElements.Count; i++)
            {
                LogicElement logicElement = logicElements[i];

                if (logicElement.Type == "LO")
                {
                    if (i > 1 && logicElements[i - 1].Type == "RE" && logicElements[i - 2].Type == "RE")
                    {
                        if (logicElements[i - 2].Handled < logicElements[i - 1].Handled)
                        {
                            LogicElement temp = logicElements[i - 2];
                            logicElements[i - 2] = logicElements[i - 1];
                            logicElements[i - 1] = temp;
                        }

                        logicElements[i - 2].Element = logicElements[i - 2].ToString() + " " + logicElements[i].ToString() + " " + logicElements[i - 1].ToString();
                        logicElements.RemoveAt(i);
                        logicElements.RemoveAt(i - 1);
                        i = i - 2;

                        logicElements[i].Handled = 1;
                    }
                    else
                    {
                        // 错误
                    }
                }
            }

            return logicElements[0].Element.ToString();
        }

        private static List<LogicElement> SCIInfoConvert(List<LogicElement> logicElements)
        {
            foreach (LogicElement logicElement in logicElements)
            {
                if (logicElement.Type == "RE")
                {
                    RelationExpression relationExpression = logicElement.Element as RelationExpression;
                    switch (relationExpression.RelationOperator.ToString())
                    {
                        case "=":
                            relationExpression.Value = new StringBuilder("(\"" + relationExpression.Value.ToString().Trim('\'') + "\")");
                            break;
                        case "!=":
                            break;
                        case ">":
                        case ">=":
                            relationExpression.Value = new StringBuilder(relationExpression.Value.ToString().Trim('\'') + "-");
                            break;
                        case "<":
                        case "<=":
                            relationExpression.Value = new StringBuilder("-" + relationExpression.Value.ToString().Trim('\''));
                            break;
                        case "like":
                            relationExpression.Value = new StringBuilder("(" + relationExpression.Value.ToString().Trim('\'') + ")");
                            break;
                    }

                    relationExpression.RelationOperator = new StringBuilder(":");
                }
                else // LO
                {
                    switch (logicElement.Element.ToString())
                    {
                        case "NOT":
                            logicElement.Element = "^";
                            break;
                        case "AND":
                            logicElement.Element = "*";
                            break;
                        case "OR":
                            logicElement.Element = "+";
                            break;
                    }
                }
            }

            return logicElements;
        }

        public static Dictionary<string, int> OperatorPriority = new Dictionary<string, int> 
        {
            {"OR", 1},
            {"AND", 2},
            {"NOT", 3},
            {")", 4},
            {"(", 5}
        };

        private static List<LogicElement> ShuntingYard(List<LogicElement> logicElements)
        {
            List<LogicElement> outElements = new List<LogicElement>();
            Stack<LogicElement> loElements = new Stack<LogicElement>();

            for (int i = 0; i < logicElements.Count; i++)
            {
                LogicElement logicElement = logicElements[i];

                if (logicElement.Type == "RE")
                {
                    outElements.Add(logicElement);
                }
                else // if (logicElement.Type == "LO")
                {
                    if (loElements.Count == 0)
                    {
                        if (logicElement.Element.ToString() != ")")
                        {
                            loElements.Push(logicElement);
                        }
                        else
                        {
                            // 如果执行到这里，代表表达式格式错误
                        }
                    }

                    // 左括号始终压入栈中
                    //  || loElements.Count == 0 || loElements.Peek().Element.ToString() == "("
                    else if (logicElement.Element.ToString() == "(")
                    {
                        loElements.Push(logicElement);
                    }
                    else if (logicElement.Element.ToString() == ")")
                    {
                        while (loElements.Peek().Element.ToString() != "(")
                        {
                            outElements.Add(loElements.Pop());
                            if (loElements.Count == 0)
                            {
                                // 如果执行到这里，代表表达式格式错误
                            }
                        }

                        loElements.Pop();
                    }
                    else if (loElements.Peek().Element.ToString() == "("
                        || OperatorPriority[logicElement.Element.ToString()] > OperatorPriority[loElements.Peek().Element.ToString()])
                    {
                        loElements.Push(logicElement);
                    }
                    else
                    {
                        // 优先级<=栈内运算符优先级，拿出RE，推入，再推入
                        while (OperatorPriority[logicElement.Element.ToString()] <= OperatorPriority[loElements.Peek().Element.ToString()])
                        {
                            outElements.Add(loElements.Pop());
                        }
                        loElements.Push(logicElement);
                    }
                }
            }

            while (loElements.Count != 0)
            {
                LogicElement loginElement = loElements.Pop();
                if (loginElement.Element.ToString() != "(")
                {
                    outElements.Add(loginElement);
                }
            }

            return outElements;
        }
    }

    public class RelationExpression
    {
        public RelationExpression()
        {
            Field = new StringBuilder();
            RelationOperator = new StringBuilder();
            Value = new StringBuilder();
        }

        public RelationExpression Copy()
        {
            return new RelationExpression { Field = Field, RelationOperator = RelationOperator, Value = Value };
        }

        public StringBuilder Field { get; set; }

        public StringBuilder RelationOperator { get; set; }

        public StringBuilder Value { get; set; }

        public string ToString()
        {
            return Field.ToString() + RelationOperator.ToString() + Value.ToString();
        }
    }

    public class LogicElement
    {
        public LogicElement()
        {
            Handled = 0;
        }

        /// <summary>
        /// RE Relation Expression
        /// LO Login Expression
        /// </summary>
        public string Type { get; set; }

        public object Element { get; set; }

        public LogicElement Copy()
        {
            return new LogicElement { Type = Type, Element = Element };
        }

        public string ToString()
        {
            if (Element is RelationExpression)
            {
                return (Element as RelationExpression).ToString();
            }
            else
            {
                return Element.ToString();
            }
        }

        public int Handled { get; set; }
    }
}
