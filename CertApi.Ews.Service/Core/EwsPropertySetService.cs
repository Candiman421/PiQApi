// CertApi.Ews.Service/Core/EwsPropertySetService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Exchange.WebServices.Data;
using CertApi.Ews.Service.Core.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace CertApi.Ews.Service.Core
{
    /// <summary>
    /// Service for managing Exchange property sets
    /// </summary>
    public class EwsPropertySetService : IEwsPropertySetService
    {
        private readonly ILogger<EwsPropertySetService> _logger;

        // Common extended properties for all message types
        private static readonly ExtendedPropertyDefinition[] CommonExtendedProperties = new[]
        {
            // Internet message headers
            MapiPropertyDefinitions.PR_TRANSPORT_MESSAGE_HEADERS,
            // Message properties
            MapiPropertyDefinitions.PR_INTERNET_MESSAGE_ID,
            MapiPropertyDefinitions.PR_CONVERSATION_TOPIC,
            MapiPropertyDefinitions.PR_CONVERSATION_INDEX,
            MapiPropertyDefinitions.PR_IN_REPLY_TO_ID,
            MapiPropertyDefinitions.PR_INTERNET_REFERENCES,
            // Time properties
            MapiPropertyDefinitions.PR_CLIENT_SUBMIT_TIME,
            MapiPropertyDefinitions.PR_MESSAGE_DELIVERY_TIME
        };

        // Email-specific extended properties
        private static readonly ExtendedPropertyDefinition[] EmailExtendedProperties = new[]
        {
            MapiPropertyDefinitions.PR_SENDER_EMAIL_ADDRESS,
            MapiPropertyDefinitions.PR_SENDER_NAME,
            MapiPropertyDefinitions.PR_DISPLAY_TO,
            MapiPropertyDefinitions.PR_DISPLAY_CC,
            MapiPropertyDefinitions.PR_DISPLAY_BCC
        };

        // Calendar-specific extended properties
        private static readonly ExtendedPropertyDefinition[] CalendarExtendedProperties = new[]
        {
            MapiPropertyDefinitions.PidLidAppointmentStateFlags,
            MapiPropertyDefinitions.PidLidAutoResetFlag,
            MapiPropertyDefinitions.PidLidBusyStatus,
            MapiPropertyDefinitions.PidLidIntendedBusyStatus,
            MapiPropertyDefinitions.PidLidLocation,
            MapiPropertyDefinitions.PidLidMeetingType,
            MapiPropertyDefinitions.PidLidRecurring
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="EwsPropertySetService"/> class
        /// </summary>
        /// <param name="logger">Logger</param>
        public EwsPropertySetService(ILogger<EwsPropertySetService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a property set with email properties
        /// </summary>
        public Task<PropertySet> WithEmailPropertiesAsync(
            PropertySet? basePropertySet = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating property set with email properties");

            // Start with base property set or first class properties if not provided
            PropertySet propertySet = basePropertySet ?? new PropertySet(BasePropertySet.FirstClassProperties);

            // Add common message properties
            propertySet.Add(
                ItemSchema.Subject,
                ItemSchema.Body,
                ItemSchema.DateTimeCreated,
                ItemSchema.DateTimeReceived,
                ItemSchema.DateTimeSent,
                ItemSchema.Categories,
                ItemSchema.HasAttachments,
                ItemSchema.Importance);

            // Add email-specific properties
            propertySet.Add(
                EmailMessageSchema.From,
                EmailMessageSchema.ToRecipients,
                EmailMessageSchema.CcRecipients,
                EmailMessageSchema.BccRecipients,
                EmailMessageSchema.IsReadReceiptRequested,
                EmailMessageSchema.IsDeliveryReceiptRequested);

            // Add extended properties
            foreach (var property in CommonExtendedProperties)
            {
                propertySet.Add(property);
            }

            foreach (var property in EmailExtendedProperties)
            {
                propertySet.Add(property);
            }

            return Task.FromResult(propertySet);
        }

        /// <summary>
        /// Creates a property set with folder properties
        /// </summary>
        public Task<PropertySet> WithFolderPropertiesAsync(
            PropertySet? basePropertySet = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating property set with folder properties");

            // Start with base property set or first class properties if not provided
            PropertySet propertySet = basePropertySet ?? new PropertySet(BasePropertySet.FirstClassProperties);

            // Add folder-specific properties
            propertySet.Add(
                FolderSchema.DisplayName,
                FolderSchema.FolderClass,
                FolderSchema.TotalCount,
                FolderSchema.ChildFolderCount,
                FolderSchema.UnreadCount,
                FolderSchema.ParentFolderId);

            return Task.FromResult(propertySet);
        }

        /// <summary>
        /// Creates a property set with calendar properties
        /// </summary>
        public Task<PropertySet> WithCalendarPropertiesAsync(
            PropertySet? basePropertySet = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Creating property set with calendar properties");

            // Start with base property set or first class properties if not provided
            PropertySet propertySet = basePropertySet ?? new PropertySet(BasePropertySet.FirstClassProperties);

            // Add common item properties
            propertySet.Add(
                ItemSchema.Subject,
                ItemSchema.Body,
                ItemSchema.DateTimeCreated,
                ItemSchema.DateTimeReceived,
                ItemSchema.DateTimeSent,
                ItemSchema.Categories,
                ItemSchema.HasAttachments,
                ItemSchema.Importance);

            // Add appointment-specific properties
            propertySet.Add(
                AppointmentSchema.Start,
                AppointmentSchema.End,
                AppointmentSchema.IsAllDayEvent,
                AppointmentSchema.LegacyFreeBusyStatus,
                AppointmentSchema.Location,
                AppointmentSchema.Organizer,
                AppointmentSchema.RequiredAttendees,
                AppointmentSchema.OptionalAttendees,
                AppointmentSchema.Resources,
                AppointmentSchema.RecurrenceId,
                AppointmentSchema.AppointmentState,
                AppointmentSchema.IsRecurring,
                AppointmentSchema.IsCancelled);

            // Add extended properties
            foreach (var property in CommonExtendedProperties)
            {
                propertySet.Add(property);
            }

            foreach (var property in CalendarExtendedProperties)
            {
                propertySet.Add(property);
            }

            return Task.FromResult(propertySet);
        }

        /// <summary>
        /// Creates a property set with extended properties
        /// </summary>
        public Task<PropertySet> WithExtendedPropertiesAsync(
            PropertySet? basePropertySet,
            IEnumerable<ExtendedPropertyDefinition> extendedProperties,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(extendedProperties);

            _logger.LogDebug("Creating property set with extended properties");

            // Start with base property set or ID only if not provided
            PropertySet propertySet = basePropertySet ?? new PropertySet(BasePropertySet.IdOnly);

            // Add the extended properties
            foreach (var property in extendedProperties)
            {
                propertySet.Add(property);
            }

            return Task.FromResult(propertySet);
        }
    }
}
