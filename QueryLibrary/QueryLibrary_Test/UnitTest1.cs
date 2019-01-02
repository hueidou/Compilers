using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryLibrary;

namespace QueryLibrary_Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string queryString1 = "作者 = '张三' AND (( 标题 = '钢铁' OR 标题 = '冷轧' ) AND 关键词 like '钢铁' )";
            string queryString3 = Enquerying.Convert(Enquerying.BAO, Enquerying.BAORPN, queryString1);
        }
    }
}
