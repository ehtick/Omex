﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.Omex.Extensions.Hosting.Services.Web.Middlewares
{
	internal class IpBasedUserIdentityProvider : IUserIdentityProvider
	{
		public UserIdentity GetUserIdentity(HttpContext httpContext)
		{
			IPAddress? remoteIpAddress = GetIpAddress(httpContext);

			return new UserIdentity
			{
				User = remoteIpAddress?.MapToIPv4()?.ToString() ?? string.Empty,
				UserHashType = UserIdentifierType.IpAddress
			};
		}

		public int MaxBytesInIdentity { get; } = 16; // IPv6 size, from here https://github.com/dotnet/runtime/blob/26a71f95b708721065f974fd43ba82a1dcb3e8f0/src/libraries/Common/src/System/Net/IPAddressParserStatics.cs#L9

		public bool TryWriteBytes(HttpContext context, Span<byte> span, out int bytesWritten)
		{
			bytesWritten = -1;
			IPAddress? remoteIpAddress = GetIpAddress(context);

			return remoteIpAddress != null
				&& remoteIpAddress.TryWriteBytes(span, out bytesWritten);
		}

		private IPAddress? GetIpAddress(HttpContext context)
		{
			IHttpConnectionFeature connection = context.Features.Get<IHttpConnectionFeature>();
			return connection.RemoteIpAddress;
		}
	}
}
