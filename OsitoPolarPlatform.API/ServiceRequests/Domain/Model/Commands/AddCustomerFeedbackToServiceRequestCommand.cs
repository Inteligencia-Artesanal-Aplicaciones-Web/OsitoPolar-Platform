namespace OsitoPolarPlatform.API.ServiceRequests.Domain.Model.Commands;

public record AddCustomerFeedbackToServiceRequestCommand(
    int ServiceRequestId,
    int Rating
    );