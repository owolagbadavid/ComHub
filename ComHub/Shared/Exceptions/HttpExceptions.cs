using System.Net;

namespace ComHub.Shared.Exceptions;

public class HttpException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}

public class BadRequestException(string message)
    : HttpException(HttpStatusCode.BadRequest, message) { }

public class NotFoundException(string message) : HttpException(HttpStatusCode.NotFound, message) { }

public class UnauthorizedException(string message)
    : HttpException(HttpStatusCode.Unauthorized, message) { }

public class ForbiddenException(string message)
    : HttpException(HttpStatusCode.Forbidden, message) { }

public class ConflictException(string message) : HttpException(HttpStatusCode.Conflict, message) { }

public class InternalServerErrorException(string message)
    : HttpException(HttpStatusCode.InternalServerError, message) { }
