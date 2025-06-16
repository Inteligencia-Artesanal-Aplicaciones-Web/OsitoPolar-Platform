using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OsitoPolarPlatform.API.bc_technicians.Domain.Model.Queries;
using OsitoPolarPlatform.API.bc_technicians.Domain.Services;
using OsitoPolarPlatform.API.bc_technicians.Interfaces.REST.Resources;
using OsitoPolarPlatform.API.bc_technicians.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace OsitoPolarPlatform.API.bc_technicians.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Category Endpoints")]

public class TechniciansController(ITechnicianCommandService techniciansService,
    ITechnicianQueryService technicianQueryService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create Technician",
        Description = "Creates a new technician in the system.",
        OperationId = "CreateTechnician")]
    [SwaggerResponse(StatusCodes.Status201Created, "Technician created", typeof(TechnicianResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "The technician could not be created")]
    public async Task<IActionResult> CreateTechnician([FromBody] CreateTechnicianResource resource)
    {
        var createTechnicianCommand = CreateTechnicianCommandFromResourceAssembler
            .ToCommandFromResource(resource);
        var technician = await techniciansService.Handle(createTechnicianCommand);
        if (technician is null)
        {
            return BadRequest("The technician could not be created");
        }
        var createdResource = TechnicianResourceFromEntityAssembler.ToResourceFromEntity(technician);
        return CreatedAtAction(nameof(GetTechnicianById), new { TechnicianId = createdResource.Id }, createdResource);
        
    }
    
    [HttpGet]
    [SwaggerOperation( 
        Summary = "Get All Technicians",
        Description = "Returns a list of all technicians in the system.",
        OperationId = "GetAllTechnicians")]
    [SwaggerResponse(StatusCodes.Status200OK, "List of technicians", typeof(IEnumerable<TechnicianResource>))]
    public async Task<IActionResult> GetAllTechnicians()
    {
        var getAllTechniciansQuery = new GetAllTechniciansQuery();
        var technicians = await technicianQueryService.Handle(getAllTechniciansQuery);
        var resources = technicians.Select(TechnicianResourceFromEntityAssembler.ToResourceFromEntity).ToList();
        return Ok(resources);
    }

    
    
    
    [HttpGet("{technicianId:int}")]
    [SwaggerOperation(
        Summary = "Get Technician by Id",
        Description = "Returns a technician by its unique identifier.",
        OperationId = "GetTechnicianById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Technician found", typeof(TechnicianResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Technician not found")]
    public async Task<IActionResult> GetTechnicianById(int technicianId)
    {
        var getTechnicianByIdQuery = new GetTechnicianByIdQuery(technicianId);
        var technician = await technicianQueryService.Handle(getTechnicianByIdQuery);
        if (technician is null)
        {
            return NotFound();
        }
        var resource = TechnicianResourceFromEntityAssembler.ToResourceFromEntity(technician);
        return Ok(resource);
    }
    
    
    
}