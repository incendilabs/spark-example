/*
 * Copyright (c) 2019-2023, Incendi (info@incendi.no)
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spark.Engine;
using Spark.Engine.Core;
using Spark.Engine.Extensions;
using Spark.Engine.Service;
using Spark.Engine.Utility;

namespace SparkFhirExample.Controllers
{
    [ApiController]
    public class FhirController : ControllerBase
    {
        private readonly IFhirService _fhirService;
        private readonly SparkSettings _settings;

        public FhirController(IFhirService fhirService, SparkSettings settings)
        {
            _fhirService = fhirService ?? throw new ArgumentNullException(nameof(fhirService));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        [HttpGet("{type}/{id}")]
        public async Task<FhirResponse> Read(string type, string id)
        {
            ConditionalHeaderParameters parameters = new ConditionalHeaderParameters(Request);
            Key key = Key.Create(type, id);
            return await _fhirService.ReadAsync(key, parameters);
        }

        [HttpPut("{type}/{id?}")]
        public async Task<FhirResponse> Update(string type, Resource resource, string id = null)
        {
            string versionId = Request.GetTypedHeaders().IfMatch?.FirstOrDefault()?.Tag.Buffer;
            Key key = Key.Create(type, id, versionId);
            if(key.HasResourceId())
            {
                Request.TransferResourceIdIfRawBinary(resource, id);

                return await _fhirService.UpdateAsync(key, resource);
            }
            else
            {
                return await _fhirService.ConditionalUpdateAsync(key, resource,
                    SearchParams.FromUriParamList(Request.TupledParameters()));
            }
        }

        [HttpPost("{type}")]
        public async Task<FhirResponse> Create(string type, Resource resource)
        {
            Key key = Key.Create(type, resource?.Id);

            if (Request.Headers.ContainsKey(FhirHttpHeaders.IfNoneExist))
            {
                NameValueCollection searchQueryString = HttpUtility.ParseQueryString(Request.GetTypedHeaders().IfNoneExist());
                IEnumerable<Tuple<string, string>> searchValues =
                    searchQueryString.Keys.Cast<string>()
                        .Select(k => new Tuple<string, string>(k, searchQueryString[k]));

                return await _fhirService.ConditionalCreateAsync(key, resource, SearchParams.FromUriParamList(searchValues));
            }

            return await _fhirService.CreateAsync(key, resource);
        }

        [HttpDelete("{type}/{id}")]
        public async Task<FhirResponse> Delete(string type, string id)
        {
            Key key = Key.Create(type, id);
            FhirResponse response = await _fhirService.DeleteAsync(key);
            return response;
        }

        [HttpGet("{type}")]
        public async Task<FhirResponse> Search(string type)
        {
            int start = FhirParameterParser.ParseIntParameter(Request.GetParameter(FhirParameter.SNAPSHOT_INDEX)) ?? 0;
            var searchparams = Request.GetSearchParams();

            return await _fhirService.SearchAsync(type, searchparams, start);
        }
    }
}
