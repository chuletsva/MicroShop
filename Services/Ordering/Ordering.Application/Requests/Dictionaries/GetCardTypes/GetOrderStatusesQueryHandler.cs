﻿using MediatR;
using System.Data.Common;
using Dapper;
using Ordering.Application.Models.Dictionaries;

namespace Ordering.Application.Requests.Dictionaries.GetCardTypes;

public class GetCardTypesQueryHandler : IRequestHandler<GetCardTypesQuery, IEnumerable<CardTypeDictDto>>
{
    private readonly DbConnection _connection;

    public GetCardTypesQueryHandler(DbConnection connection)
    {
        _connection = connection;
    }

    public Task<IEnumerable<CardTypeDictDto>> Handle(GetCardTypesQuery request, CancellationToken cancellationToken)
    {
        return _connection.QueryAsync<CardTypeDictDto>("SELECT * FROM dbo.CardTypesDict");
    }
}
