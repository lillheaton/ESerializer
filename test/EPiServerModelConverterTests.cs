using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using ESerializer.Attributes;
using ESerializer.Converters;
using ESerializer.Test.SetupHelpers;
using JsonContractSimplifier.Services.ConverterLocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ESerializer.Test
{
    [TestClass]
    public class EPiServerModelConverterTests
    {
        [TestMethod]
        public void Serialize_EPiServer_Content_Should_Only_Contain_ContentType_ExistOnModel()
        {
            // Arrange
            var target = new FakeSimplePage { Foo = "foo", Bar = "bar" };

            // Act
            var json = _contentSerializer.Serialize(target);
            var jsonObject = JObject.Parse(json);

            // Assert
            Assert.AreEqual(target.Foo, jsonObject["foo"]);
            Assert.AreEqual(target.Bar, jsonObject["bar"]);
            Assert.IsTrue(jsonObject["name"] == null); // Is a episerver property on base 
        }

        [TestMethod]
        public void Serialize_Own_Defined_Properties_That_Are_Not_Episerver()
        {
            // Arrange
            var target = new FakeSimplePage { Foo = "foo", Bar = "bar" };

            // Act
            var json = _contentSerializer.Serialize(target);
            var jsonObject = JObject.Parse(json);

            // Assert            
            Assert.AreEqual(target.Foo, jsonObject["foo"]);
            Assert.AreEqual(target.Bar, jsonObject["bar"]);
            Assert.AreEqual(target.NotPartOfContentType, jsonObject["notPartOfContentType"]);
            Assert.IsTrue(jsonObject["name"] == null); // Is a episerver property on base 
        }

        [TestMethod]
        public void Serialize_MemberSerialization_OptIn_Should_Allow_For_More_Attributes()
        {
            // Arrange
            var target = new FakeOptInPage { Foo = "foo", Bar = "bar" };
            _contentSerializer.AddExtraOptInAttribute<DisplayAttribute>();

            // Act
            var json = _contentSerializer.Serialize(target);
            var jsonObject = JObject.Parse(json);

            // Assert            
            Assert.AreEqual(target.Foo, jsonObject["foo"]);
            Assert.IsTrue(jsonObject["bar"] == null);
            Assert.IsTrue(jsonObject["name"] == null);
        }

        [TestMethod]
        public void PropertyConverter_Should_KickIn()
        {
            // Arrange
            var target = AddIContentToMockRepository(
                new FakeSimplePage(new PageReference(_fakeContent.Count + 1))
            );

            target.ContentArea = CreateContentArea(new[]
            {
                AddIContentToMockRepository(
                    new FakeOptInPage(new PageReference(_fakeContent.Count + 1))
                    {
                        Foo = "innerFoo",
                        Bar = "ignored"
                    }
                ).ContentLink
            });

            _contentSerializer.AddExtraOptInAttribute<DisplayAttribute>();

            // Act
            var json = _contentSerializer.Serialize(target);
            var jsonObject = JObject.Parse(json);

            // Assert
            //Assert.AreEqual(typeof(IFakePage).GetProperties().Length, jsonObject.Count);
            Assert.AreEqual("innerFoo", jsonObject["contentArea"][0]["foo"]);
            Assert.IsTrue(jsonObject["contentArea"][0]["bar"] == null);
        }

        public interface IFakePage
        {
            string Foo { get; set; }
            string Bar { get; set; }
            ContentArea ContentArea { get; set; }
        }

        public class FakeSimplePage : PageData, IFakePage
        {
            public FakeSimplePage() : base()
            {
            }
            public FakeSimplePage(PageReference pageLink) : base(pageLink)
            {
            }

            public string Foo { get; set; }
            public string Bar { get; set; }
            public ContentArea ContentArea { get; set; }

            [ESerializeProperty]
            public string NotPartOfContentType => "dummyData"; // A property that is not defined on ContentType nor a episerver property
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class FakeOptInPage : PageData, IFakePage
        {
            public FakeOptInPage() : base()
            {
            }
            public FakeOptInPage(PageReference pageLink) : base(pageLink)
            {
            }

            [JsonProperty]
            public string Foo { get; set; }

            public string Bar { get; set; }

            [Display]
            public ContentArea ContentArea { get; set; }
        }

        private readonly Mock<IServiceLocator> _serviceLocatorMock;
        private readonly Mock<IContentRepository> _contentRepositoryMock;
        private readonly Mock<IContentLoader> _contentLoaderMock;
        private readonly Mock<IContentTypeRepository> _contentTypeRepositoryMock;
        private readonly Mock<IConverterLocatorService> _converterLocatorServiceMock;
        private readonly IContentSerializer _contentSerializer;

        private readonly List<IContent> _fakeContent;

        private static ContentArea CreateContentArea(IEnumerable<ContentReference> content)
        {
            var contentAreaMock = new Mock<ContentArea>();
            var items = content.Select(x => new ContentAreaItem
            {
                ContentLink = x
            }).ToList();

            contentAreaMock.Setup(x => x.Items).Returns(items);
            contentAreaMock.Setup(x => x.Count).Returns(items.Count);

            return contentAreaMock.Object;
        }

        private TContent AddIContentToMockRepository<TContent>(TContent content) where TContent : IContent
        {
            _fakeContent.Add(content);

            _contentRepositoryMock.Setup(x => x.Get<IContent>(content.ContentLink, It.IsAny<LoaderOptions>())).Returns(content);
            _contentLoaderMock.Setup(x => x.Get<IContent>(content.ContentLink, It.IsAny<LoaderOptions>())).Returns(content);

            return content;
        }

        public EPiServerModelConverterTests()
        {
            _serviceLocatorMock = new Mock<IServiceLocator>();
            _contentRepositoryMock = new Mock<IContentRepository>();
            _contentLoaderMock = new Mock<IContentLoader>();
            _converterLocatorServiceMock = new Mock<IConverterLocatorService>();
            _fakeContent = new List<IContent>();

            _serviceLocatorMock.Setup(x => x.GetInstance<IContentRepository>()).Returns(_contentRepositoryMock.Object);
            _serviceLocatorMock.Setup(x => x.GetInstance<IContentLoader>()).Returns(_contentLoaderMock.Object);

            var contentAreaConverter = new ContentAreaPropertyConverter(_contentRepositoryMock.Object);
            IConverter converter = contentAreaConverter;

            _converterLocatorServiceMock
                .Setup(x => x.TryFindConverterFor(
                    It.Is<Type>(
                        type => type.BaseType == typeof(ContentArea) || 
                        type == typeof(ContentArea)), out converter)
                )
                .Returns(true);

            _converterLocatorServiceMock
                .Setup(x => x.Convert(
                    It.Is<object>(obj => 
                        obj.GetType() == typeof(ContentArea) || 
                        obj.GetType().BaseType == typeof(ContentArea)), 
                    It.IsAny<IConverter>())
                )
                .Returns((object obj, IConverter _) => contentAreaConverter.Convert(obj as ContentArea));

            ServiceLocator.SetLocator(_serviceLocatorMock.Object);

            _contentTypeRepositoryMock = new Mock<IContentTypeRepository>();
            _contentTypeRepositoryMock
                .Setup(x => x.List())
                .Returns(new[]
                {
                    ContentTypeSetupHelper.CreateContentType<FakeSimplePage>(typeof(IFakePage)),
                    ContentTypeSetupHelper.CreateContentType<FakeOptInPage>(typeof(IFakePage))
                });

            _contentSerializer = new ContentSerializer(_converterLocatorServiceMock.Object, null, _contentTypeRepositoryMock.Object);
        }
    }
}
