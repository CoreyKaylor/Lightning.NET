using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LightningDB;
using System.IO;

namespace LightningDB.Tests
{
    [TestClass]
    public class EnvironmentTests
    {
        private string _path,  _pathCopy;
        private LightningEnvironment _env;

        public EnvironmentTests()
        {
            var location = typeof(EnvironmentTests).Assembly.Location;
            var dir = Path.GetDirectoryName(location);

            _path = Path.Combine(dir, "TestDb");
            _pathCopy = _path + "Copy";
        }

        [TestInitialize]
        public void Init()
        {
            Directory.CreateDirectory(_path);
            Directory.CreateDirectory(_pathCopy);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_env != null && _env.IsOpened)
                _env.Close();

            _env = null;

            if (Directory.Exists(_path))
                Directory.Delete(_path, true);

            if (Directory.Exists(_pathCopy))
                Directory.Delete(_pathCopy, true);
        }

        [TestMethod]
        public void EnvironmentShouldBeCreatedIfWithoutFlags()
        {
            //arrange

            //act
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);

            //assert
        }

        [TestMethod]
        public void EnvironmentShouldBeCreatedIfReadOnly()
        {
            //arrange

            //act
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.ReadOnly);

            //assert
        }

        [TestMethod]
        public void EnvironmentShouldBeOpened()
        {
            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);

            //act
            _env.Open();

            //assert
            Assert.AreEqual(true, _env.IsOpened);
        }

        [TestMethod]
        public void EnvironmentShouldBeClosed()
        {
            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open();

            //act
            _env.Close();

            //assert
            Assert.AreEqual(false, _env.IsOpened);
        }

        [TestMethod]
        public void EnvironmentShouldBeCopied()
        {
            //arrange
            _env = new LightningEnvironment(_path, EnvironmentOpenFlags.None);
            _env.Open(); 

            //act
            _env.CopyTo(_pathCopy);

            //assert
            if (Directory.GetFiles(_pathCopy).Length == 0)
                Assert.Fail("Copied files doesn't exist");
        }
    }
}
