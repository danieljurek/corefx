// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Net;

using Xunit;

namespace NetPrimitivesUnitTests
{
    public class CookieInternalTest
    {
        private const string SchemePrefix = "http://";
        private const string OriginalDomain = "contoso.com";
        private const string CookieName = "CookieName";
        private const string CookieValue = "CookieValue";

        [Fact]
        public void DomainImplicit_AddCookieWithImplicitDomain_DomainImplicitTrue()
        {
            var container = new CookieContainer();
            var cookie = new Cookie(CookieName, CookieValue);
            var uri = new Uri(SchemePrefix + OriginalDomain);
            container.Add(uri, cookie);

            var cookies = container.GetCookies(uri);
            Assert.Equal(1, cookies.Count);
            Assert.True(cookies[CookieName].DomainImplicit);
        }

        [Fact]
        public void DomainImplicit_AddCookieWithExplicitDomain_DomainImplicitFalse()
        {
            var container = new CookieContainer();
            var cookie1 = new Cookie(CookieName, CookieValue) { Domain = OriginalDomain };
            var uri = new Uri(SchemePrefix + OriginalDomain);
            container.Add(uri, cookie1);

            var cookies = container.GetCookies(uri);
            Assert.Equal(1, cookies.Count);
            Assert.False(cookies[CookieName].DomainImplicit);
        }

        [Theory]
        [InlineData("cookie_name=cookie_value", new[] { "cookie_name", "cookie_value" })]
        [InlineData("cookie_name=cookie_value;", new[] { "cookie_name", "cookie_value" })]
        [InlineData("cookie_name1=cookie_value1;cookie_name2=cookie_value2", new[] { "cookie_name1", "cookie_value1", "cookie_name2", "cookie_value2" })]
        [InlineData("cookie_name1=cookie_value1;cookie_name2=cookie_value2;", new[] { "cookie_name1", "cookie_value1", "cookie_name2", "cookie_value2" })]
        public void CookieParserGetServer_SetCookieHeaderValue_Success(string cookieString, string[] expectedStrings)
        {
            int index = 0;
            int cookieCount = 0;
            var parser = new CookieParser(cookieString);
            while (true)
            {
                Cookie cookie = parser.GetServer();
                if (cookie == null)
                {
                    break;
                }

                cookieCount++;
                Assert.Equal(expectedStrings[index++], cookie.Name);
                Assert.Equal(expectedStrings[index++], cookie.Value);
            }

            int expectedCookieCount = expectedStrings.Length >> 1;
            Assert.Equal(expectedCookieCount, cookieCount);
        }

        // This assumes that the default cookie behavior is RFC 6265, which it
        // is not ("Default = Rfc2109" in Cookie.cs:20)
        // TODO: Will the Default behavior change to RFC 6265? If not, specify appropriate CookieVariant
        [Theory]
        [InlineData("name=value; Domain=contoso.com", "contoso.com")]
        [InlineData("name=value; Domain=.contoso.com", "contoso.com")]
        [InlineData("name=value; Domain=.CONTOSO.COM", "contoso.com")]
        [InlineData("name=value; Domain=subdomain.contoso.com", "subdomain.contoso.com")]
        [InlineData("name=value; Domain=.subdomain.contoso.com", "subdomain.contoso.com")]
        public void Rfc6265_Domain(string cookieHeader, string expectedDomain)
        {
            var uri = new Uri("https://contoso.com/");
            var cc = new CookieContainer();

            // Assert.DoesNotThrow
            cc.SetCookies(uri, cookieHeader);

            var cookie = cc.GetCookies(uri);
            Assert.NotEmpty(cookie);
            Assert.Equal(expectedDomain, cookie["name"].Domain);

        }
    }
}
