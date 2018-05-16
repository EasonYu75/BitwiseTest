using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitwiseTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // 電池錯誤碼 bit array
            bool?[] request = new bool?[96];

            //AND(23, 
            //OR(1, 5, 6, 36, 37, 49, 91), 
            //AND(BITWISE-NOT(1:96 except 1, 5, 6, 23, 36, 37, 49, 91, No Issue, and Clearable)))

            request[23-1] = true;

            request[1-1] = false;
            request[5-1] = false;
            request[6-1] = false;
            request[36-1] = true;
            request[37-1] = false;
            request[49-1] = false;
            request[91-1] = false;

            request[96 - 1] = false;


            // 取得 Rule
            List<Rule> ruleList = GetRuleList();

            DateTime st = DateTime.Now;

            foreach(var rule in ruleList)
            {
                if (RoleAnalysis(rule, request))
                {
                    Console.WriteLine($"BP Error Display Name : {rule.ErrorDisplayName}");
                    break;
                }
            }

            DateTime et = DateTime.Now;

            Console.WriteLine("");
            Console.WriteLine(st.ToString("yyyy/MM/dd HH:mm:ss fff"));
            Console.WriteLine(et.ToString("yyyy/MM/dd HH:mm:ss fff"));
            Console.ReadLine();
        }



        /// <summary>
        /// 全部是True, 才是True
        /// </summary>
        /// <param name="batteryErrorCode">電池錯誤碼 bit array</param>
        /// <param name="valueGroup"></param>
        /// <returns></returns>
        static bool And(bool?[] batteryErrorCode, string valueGroup)
        {
            string[] c = valueGroup.Split(',');

            foreach (var item in c)
            {
                if (batteryErrorCode[Int32.Parse(item)-1].HasValue)
                {
                    if (!batteryErrorCode[Int32.Parse(item)-1].Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 只要一個是True, 就是True
        /// </summary>
        /// <param name="batteryErrorCode">電池錯誤碼 bit array</param>
        /// <param name="valueGroup"></param>
        /// <returns></returns>
        static bool Or(bool?[] batteryErrorCode, string valueGroup)
        {
            string[] c = valueGroup.Split(',');

            foreach (var item in c)
            {
                if (batteryErrorCode[Int32.Parse(item)-1].HasValue)
                {
                    if (batteryErrorCode[Int32.Parse(item)-1].Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 全部都要是False, 才是True
        /// </summary>
        /// <param name="batteryErrorCode">電池錯誤碼 bit array</param>
        /// <returns></returns>
        static bool BitwiseNot(bool?[] batteryErrorCode)
        {
            foreach(var item in batteryErrorCode)
            {
                if (item.HasValue)
                {
                    if (item.Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 排除項目
        /// </summary>
        /// <param name="batteryErrorCode">電池錯誤碼 bit array</param>
        /// <param name="valueGroup"></param>
        /// <returns></returns>
        static bool?[] Except(bool?[] batteryErrorCode, string valueGroup)
        {
            string[] c = valueGroup.Split(',');

            foreach (var item in c)
            {
                batteryErrorCode[Int32.Parse(item)-1] = null;
            }

            return batteryErrorCode;
        }



        /// <summary>
        /// 最終結果AND
        /// </summary>
        /// <param name="totalResultList"></param>
        /// <returns></returns>
        static bool GetFinalResult(List<bool> totalResultList)
        {
            bool result = true;

            foreach (var item in totalResultList)
            {
                result = result & item;
            }

            return result;
        }

        /// <summary>
        /// 電池錯誤分析
        /// </summary>
        /// <param name="rule">錯誤規則</param>
        /// <param name="batteryErrorCode">電池錯誤碼 bit array</param>
        /// <returns></returns>
        static bool RoleAnalysis(Rule rule, bool?[] batteryErrorCode)
        {
            // 每個 Condition 結果存下來
            List<bool> totalResultList = new List<bool>();

            // 執行相對應 Operation 邏輯
            foreach (var item in rule.Conditions)
            {
                switch (item.Operation)
                {
                    case BitwiseOperation.AND:
                        totalResultList.Add(And(batteryErrorCode, item.ValueGroup));
                        break;
                    case BitwiseOperation.OR:
                        totalResultList.Add(Or(batteryErrorCode, item.ValueGroup));
                        break;
                    case BitwiseOperation.BitwiseNot:
                        totalResultList.Add(BitwiseNot(batteryErrorCode));
                        break;
                    case BitwiseOperation.BitwiseNotExcept:
                        totalResultList.Add(BitwiseNot(Except(batteryErrorCode, item.ValueGroup)));
                        break;
                }
            }

            // 將每個 Condition 結果 AND 起來
            bool result = GetFinalResult(totalResultList);

            return result;
        }

        /// <summary>
        /// 取得所有規則
        /// </summary>
        /// <returns></returns>
        static List<Rule> GetRuleList()
        {
            Rule rule = new Rule();
            rule.Id = "1";
            rule.ErrorDisplayName = "FPC Connector Failure";
            rule.Action = "Display Known Failure Screen";
            rule.sort = 28;

            rule.Conditions.Add(new Condition()
            {
                Id = "1",
                RuleId = "1",
                Operation = BitwiseOperation.AND,
                ValueGroup = "23",
                Sort = 1
            });

            rule.Conditions.Add(new Condition()
            {
                Id = "3",
                RuleId = "1",
                Operation = BitwiseOperation.OR,
                ValueGroup = "1,5,6,36,37,49,91",
                Sort = 2
            });


            string noIssueErrors = "2,3,50,51,52,56,57,58,59,60,61,62,63,82";
            string clearableErrors = "7,8,9,25,26,27,32,39,44,75,95";
            rule.Conditions.Add(new Condition()
            {
                Id = "3",
                RuleId = "1",
                Operation = BitwiseOperation.BitwiseNotExcept,
                ValueGroup = $"1,5,6,23,36,37,49,91,{noIssueErrors},{clearableErrors}",
                Sort = 3
            });

            List<Rule> result = new List<Rule>();
            result.Add(rule);

            return result;
        }
    }


    class Condition
    {
        public string Id { get; set; }

        public string RuleId { get; set; }

        public BitwiseOperation Operation { get; set; }

        public string ValueGroup { get; set; }

        public int Sort { get; set; }
    }

    class Rule
    {
        public string Id { get; set; }

        public List<Condition> Conditions { get; set; }

        public int sort { get; set; }

        public string ErrorDisplayName { get; set; }

        public string Action { get; set; }

        public string SendPackTo { get; set; }

        

        public Rule()
        {
            this.Conditions = new List<Condition>();
        }
    }

    enum BitwiseOperation
    {
        AND = 1,
        OR = 2,
        BitwiseNot = 3,
        BitwiseNotExcept = 4
    }
}
