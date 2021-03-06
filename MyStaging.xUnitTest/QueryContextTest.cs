using System;
using Xunit;
using MyStaging.Helpers;
using Microsoft.Extensions.Logging;
using MyStaging.xUnitTest;
using MyStaging.xUnitTest.Model;
using MyStaging.xUnitTest.DAL;
using MyStaging.Common;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace MyStaging.xUnitTest
{
    public class QueryContextTest
    {
        private void Init()
        {
            LoggerFactory factory = new LoggerFactory();
            var log = factory.CreateLogger<PgSqlHelper>();
            _startup.Init(log, ConstantUtil.CONNECTIONSTRING);
        }
        private string Sha256Hash(string text)
        {
            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        static int num = 0;
        [Fact(Skip = "需要手动运行该测试")]
        public void InsertTest()
        {
            Init();
            for (int i = 0; i < 10; i++)
            {
                Thread thr = new Thread(new ThreadStart(() =>
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        UserModel user = new UserModel()
                        {
                            Age = 18,
                            Createtime = DateTime.Now,
                            Id = ObjectId.NewId().ToString(),
                            Loginname = Guid.NewGuid().ToString("N").Substring(0, 8),
                            Money = 0,
                            Nickname = "北极熊",
                            Password = Sha256Hash("123456"),
                            Sex = true
                        };
                        var result = User.Insert(user);
                        Assert.Equal(user.Id, result.Id);
                    }
                    num++;
                }))
                {
                    IsBackground = true
                };
                thr.Start();
            }
            while (num < 10)
            {
                Thread.Sleep(1000);
            }
        }

        [Fact]
        public void ToList()
        {
            Init();
            var list = User.Context.OrderByDescing(f => f.Createtime).Page(1, 10).ToList();

            Assert.Equal(10, list.Count);
        }

        [Fact]
        public void ToListValueType()
        {
            Init();
            var list = User.Context.OrderByDescing(f => f.Createtime).Page(1, 10).ToList<string>("id");

            Assert.Equal(10, list.Count);
        }

        [Fact]
        public void ToListValueTulpe()
        {
            Init();
            var list = User.Context.OrderByDescing(f => f.Createtime).Page(1, 10).ToList<(string id, string loginname, string nickname)>("id", "loginname", "nickname");

            Assert.Equal(10, list.Count);
        }

        [Fact]
        public void ToOne()
        {
            Init();
            string hash = Sha256Hash("123456");
            var user = User.Context.OrderBy(f => f.Createtime).ToOne();

            Assert.Equal(hash, user.Password);
        }

        [Fact]
        public void ToScalar()
        {
            Init();
            string hash = Sha256Hash("123456");
            var password = User.Context.Where(f => f.Password == hash).OrderBy(f => f.Createtime).ToScalar<string>("password");

            Assert.Equal(hash, password);
        }

        [Fact]
        public void Sum()
        {
            Init();
            int total = 360;
            // 先把数据库任意两条记录修改为 180 
            var age = User.Context.Where(f => f.Age == 180).Sum<long>("age");

            Assert.Equal(total, age);
        }

        [Fact]
        public void Avg()
        {
            Init();
            decimal avg = 180;
            // 先把数据库任意两条记录的 age 字段修改为 180 
            var age = User.Context.Where(f => f.Age == 180).Avg<decimal>("age");

            Assert.Equal(avg, age);
        }

        [Fact]
        public void Count()
        {
            Init();
            int count = 2;
            // 先把数据库任意两条记录的 age 字段修改为 180 
            var age = User.Context.Where(f => f.Age == 180).Count();

            Assert.Equal(count, age);
        }

        [Fact]
        public void Max()
        {
            Init();
            int max = 180;
            var age = User.Context.Max<int>("age");

            /// 上面插入数据库的 age 字段是 180
            Assert.Equal(max, age);
        }

        [Fact]
        public void Min()
        {
            Init();
            int min = 18;
            var age = User.Context.Min<int>("age");

            /// 上面插入数据库的 age 字段是 18
            Assert.Equal(min, age);
        }

        [Fact]
        public void Update()
        {
            Init();
            string userid = "5b1b54bfd86b1b3bb0000009";
            var rows = User.UpdateBuilder.Where(f => f.Id == userid).SetMoney(2000).SetSex(false).SaveChange();

            /// 上面插入数据库的 age 字段是 18
            Assert.Equal(1, rows);
        }

        [Fact]
        public void Deleted()
        {
            Init();
            string userid = "5b1b54bfd86b1b3bb0000009";
            var rows = User.DeleteBuilder.Where(f => f.Id == userid).SaveChange();

            /// 上面插入数据库的 age 字段是 18
            Assert.Equal(1, rows);
        }
    }
}
