using Moq;
using Oragon.Contexts.NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;


namespace Oragon.Context.Tests
{
    public class FluentNHibernateSessionFactoryBuilderTests
    {

        [Fact]
        public void Constructor1Test()
        {
            IDictionary<string, string> nh1 = (new FluentNHibernateSessionFactoryBuilderForSQLite()
            {
                InMemory = true
            }).BaseConfiguration.ToProperties();

            Assert.Contains("NHibernate.Driver.SQLite", nh1["connection.driver_class"]);
            Assert.Contains("NHibernate.Dialect.SQLiteDialect", nh1["hibernate.dialect"]);

            IDictionary<string, string> nh2 = (new FluentNHibernateSessionFactoryBuilderForSQLite()
            {
                FileName = "a"
            }).BaseConfiguration.ToProperties();

            Assert.Contains("NHibernate.Driver.SQLite", nh2["connection.driver_class"]);
            Assert.Contains("NHibernate.Dialect.SQLiteDialect", nh2["hibernate.dialect"]);
            IDictionary<string, string> nh3 = (new FluentNHibernateSessionFactoryBuilderForSQLite()
            {
                FileName = "a",
                Password = "a"
            }).BaseConfiguration.ToProperties();

            Assert.Contains("NHibernate.Driver.SQLite", nh2["connection.driver_class"]);
            Assert.Contains("NHibernate.Dialect.SQLiteDialect", nh2["hibernate.dialect"]);

        }


        [Fact]
        public void Constructor2Test()
        {
            Mock<Configuration.IConfigurationResolver> connectionStringDiscovererMock = new Mock<Configuration.IConfigurationResolver>();
            connectionStringDiscovererMock.Setup(it => it.GetConfiguration()).Returns("Data Source=:memory:;Version=3;New=True;");

            FluentNHibernateSessionFactoryBuilderForSQLite nh1 = (new FluentNHibernateSessionFactoryBuilderForSQLite()
            {
                InMemory = true,
                ConnectionStringDiscoverer = connectionStringDiscovererMock.Object,
                TypeNames = new List<string>() { this.GetType().AssemblyQualifiedName },
                EnabledDiagnostics = true,
                NHibernateRawConfigurationValues = new Dictionary<string, string>() { { "a", "b" } },
                MergeEventListeners = new NHibernate.Event.IMergeEventListener[] { new Mock<NHibernate.Event.IMergeEventListener>().Object }
            });


            using (NHibernate.ISessionFactory session = nh1.BuildSessionFactory())
            {

                session.Close();
            }
        }


        [Fact]
        public void BaseConfigurationTest()
        {
            Mock<Configuration.IConfigurationResolver> connectionStringDiscovererMock = new Mock<Configuration.IConfigurationResolver>();
            connectionStringDiscovererMock.Setup(it => it.GetConfiguration()).Returns("Data Source=:memory:;Version=3;New=True;");

            FNHSFB nh1 = new FNHSFB() { DefaultSchema = "ss" };
            nh1.BaseConfiguration = FluentNHibernate.Cfg.Db.SQLiteConfiguration.Standard.InMemory();
            nh1.BaseConfiguration.FormatSql();

        }


        public class FNHSFB : FluentNHibernateSessionFactoryBuilderForAnything<FluentNHibernate.Cfg.Db.SQLiteConfiguration, FluentNHibernate.Cfg.Db.ConnectionStringBuilder>
        {

            public FNHSFB()
            {
                this.OnExposeConfiguration = (it) => { };
            }
        }


        [Fact]
        public void TestEmptyTypes()
        {
            Assert.Throws<NullReferenceException>(() =>
                new FNHSFB().BuildSessionFactory()

            );
        }



        [Fact]
        public void EventListeners()
        {
            FNHSFB sfb = new FNHSFB()
            {
                TypeNames = new List<string>()
                {
                    "Oragon.Context.Tests.Integrated.AppSym.Domain.Classroom, Oragon.Context.Tests",
                    "Oragon.Context.Tests.Integrated.AppSym.Data.Mapping.ClassroomMapper, Oragon.Context.Tests"
                },
                BaseConfiguration = FluentNHibernate.Cfg.Db.SQLiteConfiguration.Standard.InMemory()
            };

            string[] propNames = typeof(NHibernate.Cfg.Configuration).GetProperties().Where(It => It.Name.EndsWith("EventListeners") && !It.Name.StartsWith("EventListeners")).Select(it => it.Name).ToArray();

            foreach (string propName in propNames)
            {
                Oragon.Spring.Expressions.ExpressionEvaluator.SetValue(sfb, propName, new object[] { null });
            }


            sfb.BuildSessionFactory();

        }

    }
}
