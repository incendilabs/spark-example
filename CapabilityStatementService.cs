using System;
using Hl7.Fhir.Model;
using Spark.Engine;
using Spark.Engine.Core;
using Spark.Engine.Service.FhirServiceExtensions;

namespace spark_example;

public class CapabilityStatementService : ICapabilityStatementService
{
    private readonly IFhirModel _fhirModel;
    private readonly ServerVersion _serverVersion;
    private readonly FHIRVersion _fhirVersion;

    public CapabilityStatementService(
        IFhirModel fhirModel,
        ServerVersion serverVersion,
        FHIRVersion fhirVersion)
    {
        _fhirModel = fhirModel;
        _serverVersion = serverVersion;
        _fhirVersion = fhirVersion;
    }

    public CapabilityStatement GetSparkCapabilityStatement()
    {
        return new CapabilityStatementBuilder()
            .WithName("Spark FHIR Server Example")
            .WithVersion(_serverVersion)
            .WithFhirVersion(_fhirVersion)
            .WithDate(DateTimeOffset.UtcNow)
            .WithStatus(PublicationStatus.Active)
            .WithExperimental(true)
            .WithKind(CapabilityStatementKind.Capability)
            .WithAcceptFormat(["json", "xml"])
            .WithRest(restBuilder =>
                {
                    restBuilder.WithMode(CapabilityStatement.RestfulCapabilityMode.Server);

                    foreach (var resourceType in _fhirModel.SupportedResources)
                    {
                        restBuilder.WithResource(resourceBuilder =>
                            {
                                resourceBuilder.WithType(resourceType)
                                    .WithVersioning(CapabilityStatement.ResourceVersionPolicy.NoVersion)
                                    .WithReadHistory(false)
                                    .WithUpdateCreate(true)
                                    .WithInteraction(CapabilityStatement.TypeRestfulInteraction.Create)
                                    .WithInteraction(CapabilityStatement.TypeRestfulInteraction.Read)
                                    .WithInteraction(CapabilityStatement.TypeRestfulInteraction.Update)
                                    .WithInteraction(CapabilityStatement.TypeRestfulInteraction.Delete)
                                    .WithInteraction(CapabilityStatement.TypeRestfulInteraction.SearchType);

                                foreach (var searchParameter in _fhirModel.FindSearchParameters(resourceType))
                                    resourceBuilder.WithSearchParam(
                                        searchParameter.Name,
                                        searchParameter.Type ?? SearchParamType.String,
                                        documentation: searchParameter.Description
                                    );

                                resourceBuilder.WithSearchParam(
                                    "_summary",
                                    SearchParamType.String,
                                    documentation: "Summary for resource"
                                );

                                return resourceBuilder;
                            }
                        );
                    }

                    return restBuilder;
                }
            )
            .Build();
    }
}
