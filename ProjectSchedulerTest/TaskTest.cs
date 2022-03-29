using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProjectScheduler;
using System;
using System.Data;
using System.Xml;

using System.IO;



namespace ProjectSchedulerTest
{
    [TestClass]
    public class TaskTest
    {
        private readonly DateTime
            Xmas11h00 = new DateTime(2021, 12, 25, 11, 0, 0),
            Xmas12h30 = new DateTime(2021, 12, 25, 12, 30, 0),
            Xmas14h00 = new DateTime(2021, 12, 25, 14, 0, 0),
            Jan1st11h00 = new DateTime(2022, 1, 1, 11, 0, 0),
            Jan1st12h30 = new DateTime(2022, 1, 1, 12, 30, 0),

            Jan1st14h00 = new DateTime(2022, 1, 1, 14, 0, 0);
        [TestInitialize]
        public void initTest()
        {
            IClock.SetTestClock(null);
            IGuid.SetTestGuid(null);
        }
        #region Initialization
        [TestMethod]
        public void InitAllParameters()
        {
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);

            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);

            var test = new Task("foo", Xmas11h00, Xmas14h00);

            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas11h00, test.Start);
            Assert.AreEqual(Xmas14h00, test.End);
            Assert.AreEqual(TimeSpan.FromHours(3), test.Duration);
            Assert.AreEqual(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"), test.Id);
            Assert.IsTrue(test.IsStarted);
        }

        [TestMethod]
        public void InitAllParametersYearChange()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var test = new Task("foo", Xmas12h30, Jan1st12h30);

            Assert.AreEqual(TimeSpan.FromDays(7), test.Duration);
        }

        [TestMethod]
        public void InitWithDefaultStartDate()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);



            var test = new Task("foo");

            Assert.IsFalse(test.HasEnd);
            Assert.IsFalse(test.HasDuration);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas12h30, test.Start);
            Assert.IsFalse(test.IsStarted);
        }

        [TestMethod]
        public void InitNoEndDateWithPastStartDate()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var test = new Task("foo", Xmas11h00);

            Assert.IsFalse(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas11h00, test.Start);
            Assert.IsTrue(test.IsStarted);
        }

        [TestMethod]
        public void InitNoEndDateWithStartDateAtNow()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var test = new Task("foo", stubClock.Object.Now);

            Assert.IsFalse(test.HasEnd);
            Assert.IsFalse(test.HasDuration);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas12h30, test.Start);
            Assert.IsFalse(test.IsStarted);
        }
        [TestMethod]
        public void InitNoEndDateWithFutureStartDate()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas11h00);
            IClock.SetTestClock(stubClock.Object);
            var test = new Task("foo", Xmas12h30);

            Assert.IsFalse(test.HasEnd);
            Assert.IsFalse(test.HasDuration);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas12h30, test.Start);
            Assert.IsFalse(test.IsStarted);
        }

        [TestMethod]
        public void InitWithNullTitle()
        {
            Action act = () => new Task(null, Xmas12h30, Xmas14h00);

            Assert.ThrowsException<NullReferenceException>(act);
        }

        [TestMethod]
        public void InitWithBadEndDate()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            Action act = () => new Task("foo", Xmas12h30, Xmas11h00);

            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }
        #endregion

        #region Change Title
        [TestMethod]
        public void ChangeTitle()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var test = new Task("foo", Xmas12h30, Xmas14h00);

            test.Title = "bar";

            Assert.AreEqual("bar", test.Title);
        }
        [TestMethod]
        public void ChangeNullTitle()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var test = new Task("foo", Xmas12h30, Xmas14h00);

            Action act = () => test.Title = null;

            Assert.ThrowsException<NullReferenceException>(act);
        }
        #endregion

        #region Change Start
        [TestMethod]
        public void ChangeStartForEndedTask()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var test = new Task("foo", Xmas12h30, Xmas14h00);


            test.Start = Xmas11h00;

            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Xmas11h00, test.Start);
            Assert.AreEqual(Xmas12h30, test.End);
            Assert.AreEqual(TimeSpan.FromMinutes(90), test.Duration);
        }
        [TestMethod]
        public void ChangeStartForNotEndedTaskFromPastToPast()
        {
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas14h00);
            IClock.SetTestClock(stubClock.Object);
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var test = new Task("foo", Xmas12h30, Jan1st12h30);
            test.Start = Xmas11h00;

            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Xmas11h00, test.Start);
            Assert.AreEqual(Jan1st11h00, test.End);
            Assert.IsTrue(test.IsStarted);
            Assert.AreEqual(TimeSpan.FromHours(7 * 24), test.Duration);
        }
        [TestMethod]
        public void ChangeStartForNotEndedTaskFromFutureToPast()
        {
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var test = new Task("foo", Xmas14h00, Jan1st14h00);
            test.Start = Xmas11h00;

            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Xmas11h00, test.Start);
            Assert.AreEqual(Jan1st11h00, test.End);
            Assert.IsTrue(test.IsStarted);
            Assert.AreEqual(TimeSpan.FromHours((7 * 24)), test.Duration);
        }
        [TestMethod]
        public void ChangeStartForNotEndedTaskFromPastToFuture()
        {
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var test = new Task("foo", Xmas11h00, Jan1st11h00);
            test.Start = Xmas14h00;

            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Xmas14h00, test.Start);
            Assert.AreEqual(Jan1st14h00, test.End);
            Assert.IsFalse(test.IsStarted);
            Assert.AreEqual(TimeSpan.FromHours(7 * 24), test.Duration);
        }
        [TestMethod]
        public void ChangeStartForNotEndedTaskFromFutureToFuture()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas11h00);
            IClock.SetTestClock(stubClock.Object);
            var test = new Task("foo", Xmas12h30, Jan1st12h30);
            test.Start = Xmas14h00;

            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Xmas14h00, test.Start);
            Assert.AreEqual(Jan1st14h00, test.End);
            Assert.IsFalse(test.IsStarted);
            Assert.AreEqual(TimeSpan.FromHours(7 * 24), test.Duration);
        }
        #endregion

        #region Change End
        [TestMethod]
        public void ChangeEnd()
        {

            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var test = new Task("foo", Xmas11h00, Xmas14h00);




            test.End = Xmas12h30;

            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Xmas11h00, test.Start);
            Assert.AreEqual(Xmas12h30, test.End);
            Assert.AreEqual(TimeSpan.FromMinutes(90), test.Duration);
        }
        [TestMethod]
        public void ChangeEndBeforeStart()
        {
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var test = new Task("foo", Xmas12h30, Xmas14h00);

            Action act = () => test.End = Xmas11h00;

            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }
        #endregion

        #region FromDb
        [TestMethod]
        public void FromDbNullEndDate()
        {
            var stubDataReader = new Mock<IDataReader>();

            stubDataReader.Setup(reader => reader.GetGuid(0))
                .Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            stubDataReader.Setup(reader => reader.GetString(1))
                .Returns("foo");
            stubDataReader.Setup(reader => reader.GetDateTime(2))
                .Returns(Xmas12h30);
            stubDataReader.Setup(reader => reader.IsDBNull(3))
                .Returns(true);

            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas11h00);
            IClock.SetTestClock(stubClock.Object);
            var test = Task.FromDb(stubDataReader.Object);

            Assert.IsFalse(test.HasEnd);
            Assert.IsFalse(test.HasDuration);
            Assert.AreEqual(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"), test.Id);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas12h30, test.Start);
        }

        [TestMethod]
        public void FromDbValidEndDate()
        {
            var stubDataReader = new Mock<IDataReader>();

            stubDataReader.Setup(reader => reader.GetGuid(0))
                .Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            stubDataReader.Setup(reader => reader.GetString(1))
                .Returns("foo");
            stubDataReader.Setup(reader => reader.GetDateTime(2))
                .Returns(Xmas11h00);
            stubDataReader.Setup(reader => reader.GetDateTime(3))
                .Returns(Xmas14h00);

            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var test = Task.FromDb(stubDataReader.Object);
            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"), test.Id);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas11h00, test.Start);
            Assert.AreEqual(Xmas14h00, test.End);
            Assert.AreEqual(TimeSpan.FromHours(3), test.Duration);

        }

        [TestMethod]
        public void FromDbEndDateBeforeStartDate()
        {
            var stubDataReader = new Mock<IDataReader>();

            stubDataReader.Setup(reader => reader.GetGuid(0))
                .Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            stubDataReader.Setup(reader => reader.GetString(1))
                .Returns("foo");
            stubDataReader.Setup(reader => reader.GetDateTime(2))
                .Returns(Xmas14h00);
            stubDataReader.Setup(reader => reader.GetDateTime(3))
                .Returns(Xmas11h00);
            Action act = () => Task.FromDb(stubDataReader.Object);
            Assert.ThrowsException<ArgumentOutOfRangeException>(act);
        }
        #endregion

        #region FromXml
        [TestMethod]
        public void FromXmlWithEndDate()
        {
            var stubXmlReader = new Mock<XmlReader>();
            stubXmlReader.Setup(reader => reader.NodeType)
                    .Returns(XmlNodeType.Element);
            stubXmlReader.Setup(reader => reader.Name)
                    .Returns("task");
            stubXmlReader.Setup(reader => reader.GetAttribute("id"))
                    .Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1").ToString());
            stubXmlReader.Setup(reader => reader.GetAttribute("title"))
                    .Returns("foo");
            stubXmlReader.Setup(reader => reader.GetAttribute("start"))
                    .Returns("12/25/2021 11:00:00");
            stubXmlReader.Setup(reader => reader.GetAttribute("end"))
                    .Returns("12/25/2021 14:00:00");
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var test = Task.FromXml(stubXmlReader.Object);

            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"), test.Id);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas11h00, test.Start);
            Assert.AreEqual(Xmas14h00, test.End);

        }
        [TestMethod]
        public void FromXmlNoEndDate()
        {
            var stubXmlReader = new Mock<XmlReader>();
            stubXmlReader.Setup(reader => reader.NodeType)
                    .Returns(XmlNodeType.Element);
            stubXmlReader.Setup(reader => reader.Name)
                    .Returns("task");
            stubXmlReader.Setup(reader => reader.GetAttribute("id"))
                    .Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1").ToString());
            stubXmlReader.Setup(reader => reader.GetAttribute("title"))
                    .Returns("foo");
            stubXmlReader.Setup(reader => reader.GetAttribute("start"))
                    .Returns("12/25/2021 11:00:00");
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var test = Task.FromXml(stubXmlReader.Object);

            Assert.IsFalse(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"), test.Id);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas11h00, test.Start);

        }

        [TestMethod]
        public void FromXmlNoTitle()
        {
            var stubXmlReader = new Mock<XmlReader>();
            stubXmlReader.Setup(reader => reader.NodeType)
                    .Returns(XmlNodeType.Element);
            stubXmlReader.Setup(reader => reader.Name)
                    .Returns("task");
            stubXmlReader.Setup(reader => reader.GetAttribute("title"))
                    .Returns("foo");
            stubXmlReader.Setup(reader => reader.GetAttribute("start"))
                    .Returns("12/25/2021 11:00:00");



            Action act = () => Task.FromXml(stubXmlReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        [TestMethod]
        public void FromXmlNoStartDate()
        {
            var stubXmlReader = new Mock<XmlReader>();
            stubXmlReader.Setup(reader => reader.NodeType)
                    .Returns(XmlNodeType.Element);
            stubXmlReader.Setup(reader => reader.Name)
                    .Returns("task");
            stubXmlReader.Setup(reader => reader.GetAttribute("id"))
                    .Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1").ToString());
            stubXmlReader.Setup(reader => reader.GetAttribute("title"))
                    .Returns("foo");
            stubXmlReader.Setup(reader => reader.GetAttribute("END"))
                    .Returns("12/25/2021 11:00:00");
            Action act = () => Task.FromXml(stubXmlReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        [TestMethod]
        public void FromXmlNoGuid()
        {
            var stubXmlReader = new Mock<XmlReader>();
            stubXmlReader.Setup(reader => reader.NodeType)
                    .Returns(XmlNodeType.Element);
            stubXmlReader.Setup(reader => reader.Name)
                    .Returns("task");

            stubXmlReader.Setup(reader => reader.GetAttribute("title"))
                    .Returns("foo");
            stubXmlReader.Setup(reader => reader.GetAttribute("start"))
                    .Returns("12/25/2021 11:00:00");
            stubXmlReader.Setup(reader => reader.GetAttribute("end"))
                    .Returns("12/25/2021 14:00:00");
            Action act = () => Task.FromXml(stubXmlReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        [TestMethod]
        public void FromXmlInvalidGuid()
        {
            var stubXmlReader = new Mock<XmlReader>();
            stubXmlReader.Setup(reader => reader.NodeType)
                    .Returns(XmlNodeType.Element);
            stubXmlReader.Setup(reader => reader.Name)
                    .Returns("task");
            stubXmlReader.Setup(reader => reader.GetAttribute("id"))
                    .Returns("okkkk");
            stubXmlReader.Setup(reader => reader.GetAttribute("title"))
                    .Returns("foo");
            stubXmlReader.Setup(reader => reader.GetAttribute("start"))
                    .Returns("12/25/2021 11:00:00");
            stubXmlReader.Setup(reader => reader.GetAttribute("end"))
                    .Returns("12/25/2021 14:00:00");
            Action act = () => Task.FromXml(stubXmlReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        [TestMethod]
        public void FromXmlInvalidStartDate()
        {
            var stubXmlReader = new Mock<XmlReader>();
            stubXmlReader.Setup(reader => reader.NodeType)
                    .Returns(XmlNodeType.Element);
            stubXmlReader.Setup(reader => reader.Name)
                    .Returns("task");
            stubXmlReader.Setup(reader => reader.GetAttribute("id"))
                    .Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1").ToString());
            stubXmlReader.Setup(reader => reader.GetAttribute("title"))
                    .Returns("foo");
            stubXmlReader.Setup(reader => reader.GetAttribute("start"))
                    .Returns("12/21 11:00");
            stubXmlReader.Setup(reader => reader.GetAttribute("end"))
                    .Returns("12/25/2021 14:00:00");
            Action act = () => Task.FromXml(stubXmlReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        [TestMethod]
        public void FromXmlInvalidEndDate()
        {
            var stubXmlReader = new Mock<XmlReader>();
            stubXmlReader.Setup(reader => reader.NodeType)
                    .Returns(XmlNodeType.Element);
            stubXmlReader.Setup(reader => reader.Name)
                    .Returns("task");
            stubXmlReader.Setup(reader => reader.GetAttribute("id"))
                    .Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1").ToString());
            stubXmlReader.Setup(reader => reader.GetAttribute("title"))
                    .Returns("foo");
            stubXmlReader.Setup(reader => reader.GetAttribute("start"))
                    .Returns("12/25/2021 11:00:00");
            stubXmlReader.Setup(reader => reader.GetAttribute("end"))
                    .Returns("12//2021 10:00");

            Action act = () => Task.FromXml(stubXmlReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        #endregion

        #region ToXml
        [TestMethod]
        public void ToXmlWithDate()
        {
            var mockWriter = new Mock<IXmlWriter>();
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);

            var test = new Task("foo", Xmas11h00, Xmas14h00);

            test.ToXml(mockWriter.Object);

            mockWriter.Verify(writer => writer.WriteStartElement("task"), Times.Once);
            mockWriter.Verify(writer => writer.WriteAttributeString("id", Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1").ToString()), Times.Once);
            mockWriter.Verify(writer => writer.WriteAttributeString("title", "foo"), Times.Once);
            mockWriter.Verify(writer => writer.WriteAttributeString("start", "12/25/2021 11:00:00"), Times.Once);
            mockWriter.Verify(writer => writer.WriteAttributeString("end", "12/25/2021 14:00:00"), Times.Once);
            mockWriter.Verify(writer => writer.WriteEndElement(), Times.Once);
        }
        [TestMethod]
        public void ToXmlWithNoDate()
        {
            var mockWriter = new Mock<IXmlWriter>();
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);

            var test = new Task("foo");

            test.ToXml(mockWriter.Object);

            mockWriter.Verify(writer => writer.WriteStartElement("task"), Times.Once);
            mockWriter.Verify(writer => writer.WriteAttributeString("id", Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1").ToString()), Times.Once);
            mockWriter.Verify(writer => writer.WriteAttributeString("title", "foo"), Times.Once);
            mockWriter.Verify(writer => writer.WriteAttributeString("start", "12/25/2021 12:30:00"), Times.Once);
            mockWriter.Verify(writer => writer.WriteAttributeString("end", null), Times.Once);
            mockWriter.Verify(writer => writer.WriteEndElement(), Times.Once);
        }
        #endregion

        #region FromCsv
        [TestMethod]
        public void FromCsvWithEndDate()
        {
            var stubTextReader = new Mock<TextReader>();
            stubTextReader.Setup(reader => reader.ReadLine())
             .Returns("88f76ba1-c8A3-4261-ba44-76611dd4ebc1;foo;12/25/2021 11:00:00;12/25/2021 14:00:00");

            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);
            var test = Task.FromCsv(stubTextReader.Object);

            Assert.IsTrue(test.HasEnd);
            Assert.IsTrue(test.HasDuration);
            Assert.AreEqual(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"), test.Id);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas11h00, test.Start);
            Assert.AreEqual(Xmas14h00, test.End);

        }
        [TestMethod]
        public void FromCsvNoEndDate()
        {
            var stubTextReader = new Mock<TextReader>();
            stubTextReader.Setup(reader => reader.ReadLine())
             .Returns("88f76ba1-c8A3-4261-ba44-76611dd4ebc1;foo;12/25/2021 14:00:00");
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);


            var test = Task.FromCsv(stubTextReader.Object);

            Assert.IsFalse(test.HasEnd);
            Assert.IsFalse(test.HasDuration);
            Assert.AreEqual(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"), test.Id);
            Assert.AreEqual("foo", test.Title);
            Assert.AreEqual(Xmas14h00, test.Start);
        }
        [TestMethod]
        public void FromCsvNoStartDate()
        {
            var stubTextReader = new Mock<TextReader>();
            stubTextReader.Setup(reader => reader.ReadLine())
             .Returns("88f76ba1-c8A3-4261-ba44-76611dd4ebc1;foo");


            Action act = () => Task.FromCsv(stubTextReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        [TestMethod]
        public void FromCsvInvalidGuid()
        {
            var stubTextReader = new Mock<TextReader>();
            stubTextReader.Setup(reader => reader.ReadLine())
             .Returns("OUI;foo;12/25/2021 11:00:00;12/25/2021 14:00:00");


            Action act = () => Task.FromCsv(stubTextReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        [TestMethod]
        public void FromCsvInvalidStartDate()
        {
            var stubTextReader = new Mock<TextReader>();
            stubTextReader.Setup(reader => reader.ReadLine())
             .Returns("88f76ba1-c8A3-4261-ba44-76611dd4ebc1;foo;125/2021 11:00:00;12/25/2021 14:00:00");


            Action act = () => Task.FromCsv(stubTextReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        [TestMethod]
        public void FromCsvInvalidEndDate()
        {
            var stubTextReader = new Mock<TextReader>();
            stubTextReader.Setup(reader => reader.ReadLine())
             .Returns("88f76ba1-c8A3-4261-ba44-76611dd4ebc1;foo;2021:25:12 11:00:00;12/25/2021 14:00:00");



            Action act = () => Task.FromCsv(stubTextReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        [TestMethod]
        public void FromCsvInvalidCalendarDate()
        {
            var stubTextReader = new Mock<TextReader>();
            stubTextReader.Setup(reader => reader.ReadLine())
             .Returns("88f76ba1-c8A3-4261-ba44-76611dd4ebc1;foo;12/25/2021 11:00:00;2021:25:12 14:00:00");


            Action act = () => Task.FromCsv(stubTextReader.Object);

            Assert.ThrowsException<FormatException>(act);
        }
        #endregion

        #region ToCsv
        [TestMethod]
        public void ToCsvWithDate()
        {
            var mockWriter = new Mock<ITextWriter>();
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);

            var test = new Task("foo", Xmas11h00, Xmas14h00);

            test.ToCsv(mockWriter.Object);

            mockWriter.Verify(writer => writer.WriteLine("88f76ba1-c8a3-4261-ba44-76611dd4ebc1;foo;25/12/2021 11:00:00;25/12/2021 14:00:00"), Times.Once);
        }
        [TestMethod]
        public void ToCsvWithNoDate()
        {
            var mockWriter = new Mock<ITextWriter>();
            var stubGuid = new Mock<IGuid>();
            stubGuid.Setup(guid => guid.id).Returns(Guid.Parse("88F76BA1-C8A3-4261-BA44-76611DD4EBC1"));
            IGuid.SetTestGuid(stubGuid.Object);
            var stubClock = new Mock<IClock>();

            stubClock.Setup(clock => clock.Now).Returns(Xmas12h30);
            IClock.SetTestClock(stubClock.Object);

            var test = new Task("foo");

            test.ToCsv(mockWriter.Object);

            mockWriter.Verify(writer => writer.WriteLine("88f76ba1-c8a3-4261-ba44-76611dd4ebc1;foo;25/12/2021 12:30:00"), Times.Once);
        }
        #endregion
    }
}
