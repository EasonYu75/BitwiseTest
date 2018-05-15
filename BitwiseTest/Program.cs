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
            bool?[] request = new bool?[96];

            //AND(23, OR(1, 5, 6, 36, 37, 49, 91), 
            //AND(BITWISE-NOT(1:96 except 1, 5, 6, 23, 36, 37, 49, 91, No Issue, and Clearable)))

            request[23] = true;

            request[1] = true;
            request[5] = true;
            request[6] = true;
            request[36] = true;
            request[37] = true;
            request[49] = true;   
            request[91] = false;

            //request[92] = true;


            // 取得 Rule
            Rule rule = GetRule();

            // 每個 Condition 結果存下來
            List<bool> totalResultList = new List<bool>();


            DateTime st = DateTime.Now;

            // 執行相對應 Operation 邏輯
            foreach (var item in rule.Conditions)
            {
                switch (item.Operation)
                {
                    case BitwiseOperation.AND:
                        totalResultList.Add(And(request, item.ValueGroup));
                        break;
                    case BitwiseOperation.OR:
                        totalResultList.Add(Or(request, item.ValueGroup));
                        break;
                    case BitwiseOperation.BitwiseNot:
                        totalResultList.Add(BitwiseNot(request));
                        break;
                    case BitwiseOperation.BitwiseNotExcept:
                        totalResultList.Add(BitwiseNot(Except(request, item.ValueGroup)));
                        break;
                }
            }

            // 將每個 Condition 結果 AND 起來
            bool result = GetFinalResult(totalResultList);

            DateTime et = DateTime.Now;

            Console.WriteLine(result);

            Console.WriteLine("");
            Console.WriteLine(st.ToString("yyyy/MM/dd HH:mm:ss fff"));
            Console.WriteLine(et.ToString("yyyy/MM/dd HH:mm:ss fff"));
            Console.ReadLine();
        }

        private static Rule GetRule()
        {
            Rule rule = new Rule();
            rule.Id = "1";
            rule.ErrorDisplayName = "FPC Connector Failure";
            rule.Action = "Display Known Failure Screen";

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

            rule.Conditions.Add(new Condition()
            {
                Id = "3",
                RuleId = "1",
                Operation = BitwiseOperation.BitwiseNotExcept,
                ValueGroup = "1,5,6,23,36,37,49,91",
                Sort = 3
            });

            return rule;
        }

        static bool And(bool?[] a, string b)
        {
            string[] c = b.Split(',');

            foreach (var item in c)
            {
                if (a[Int32.Parse(item)].HasValue)
                {
                    if (!a[Int32.Parse(item)].Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        static bool Or(bool?[] a, string b)
        {
            string[] c = b.Split(',');

            foreach (var item in c)
            {
                if (a[Int32.Parse(item)].HasValue)
                {
                    if (a[Int32.Parse(item)].Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool BitwiseNot(bool?[] a)
        {
            foreach(var item in a)
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

        static bool?[] Except(bool?[] a, string b)
        {
            string[] c = b.Split(',');

            foreach (var item in c)
            {
                a[Int32.Parse(item)] = null;
            }

            return a;
        }

        static bool GetFinalResult(List<bool> totalResultList)
        {
            bool result = true;

            foreach (var item in totalResultList)
            {
                result = result & item;
            }

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
