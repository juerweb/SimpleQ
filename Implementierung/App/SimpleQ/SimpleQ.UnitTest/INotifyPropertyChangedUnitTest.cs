using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleQ.PageModels;

namespace SimpleQ.UnitTest
{
    [TestClass]
    public class INotifyPropertyChangedUnitTest
    {
        [TestMethod]
        public void RegisterPageModelTest()
        {
            //Arrange
            bool invoked = false;

            RegisterPageModel pageModel = new RegisterPageModel();

            pageModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName.Equals("RegisterCode"))
                    invoked = true;
            };

            //Act

            pageModel.RegisterCode = 123456;

            //Assert
            Assert.IsTrue(invoked);
        }

        [TestMethod]
        public void QRCodeScannerPageModelTest()
        {
            //Arrange
            bool invoked = false;

            QRCodeScannerPageModel pageModel = new QRCodeScannerPageModel();

            pageModel.PropertyChanged += (sender, e) =>
            {
                Debug.WriteLine("TEST");
                if (e.PropertyName.Equals("IsScanning"))
                    invoked = true;
            };

            //Act

            pageModel.IsScanning = true;

            //Assert
            Assert.IsTrue(invoked);
        }
    }
}
