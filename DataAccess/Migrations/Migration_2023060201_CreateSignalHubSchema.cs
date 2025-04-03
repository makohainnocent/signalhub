using FluentMigrator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DataAccess.Migrations
{
    [Migration(2023060201)]
    public class CreateSignalHubSchema : Migration
    {
        public override void Up()
        {
            // 1. Tenants table (multi-tenancy foundation)
            Create.Table("Tenants")
                .WithColumn("TenantId").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(100).NotNullable().Unique()
                .WithColumn("Slug").AsString(50).NotNullable().Unique()
                .WithColumn("Description").AsString(500).Nullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("OwnerUserId").AsInt32().NotNullable();

            // 2. Applications table
            Create.Table("Applications")
                .WithColumn("ApplicationId").AsInt32().PrimaryKey().Identity()
                .WithColumn("TenantId").AsInt32().NotNullable()
                .WithColumn("Name").AsString(100).NotNullable()
                .WithColumn("ApiKey").AsString(64).NotNullable().Unique()
                .WithColumn("Description").AsString(500).Nullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("RateLimit").AsInt32().NotNullable().WithDefaultValue(1000)
                .WithColumn("OwnerUserId").AsInt32().NotNullable();

            // 3. RecipientGroups table
            Create.Table("RecipientGroups")
                .WithColumn("GroupId").AsInt32().PrimaryKey().Identity()
                .WithColumn("TenantId").AsInt32().NotNullable()
                .WithColumn("Name").AsString(100).NotNullable()
                .WithColumn("Description").AsString(500).Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("CreatedByUserId").AsInt32().NotNullable();

            // 4. Recipients table
            Create.Table("Recipients")
                .WithColumn("RecipientId").AsInt32().PrimaryKey().Identity()
                .WithColumn("TenantId").AsInt32().NotNullable()
                .WithColumn("ExternalId").AsString(100).Nullable()
                .WithColumn("Email").AsString(100).Nullable()
                .WithColumn("PhoneNumber").AsString(20).Nullable()
                .WithColumn("DeviceToken").AsString(255).Nullable()
                .WithColumn("FullName").AsString(100).Nullable()
                .WithColumn("Preferences").AsCustom("nvarchar(max)").Nullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("UserId").AsInt32().Nullable();

            // 5. RecipientGroupMembers junction table
            Create.Table("RecipientGroupMembers")
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("RecipientId").AsInt32().NotNullable()
                .WithColumn("AddedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("AddedByUserId").AsInt32().NotNullable();

            // 6. NotificationTemplates table
            Create.Table("NotificationTemplates")
                .WithColumn("TemplateId").AsInt32().PrimaryKey().Identity()
                .WithColumn("ApplicationId").AsInt32().NotNullable()
                .WithColumn("Name").AsString(100).NotNullable()
                .WithColumn("Description").AsString(500).Nullable()
                .WithColumn("Content").AsCustom("nvarchar(max)").NotNullable()
                .WithColumn("VariablesSchema").AsCustom("nvarchar(max)").Nullable()
                .WithColumn("Version").AsInt32().NotNullable().WithDefaultValue(1)
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("CreatedByUserId").AsInt32().NotNullable()
                .WithColumn("ApprovedByUserId").AsInt32().Nullable()
                .WithColumn("ApprovalStatus").AsString(20).NotNullable().WithDefaultValue("Draft");

            // 7. TemplateChannels table
            Create.Table("TemplateChannels")
                .WithColumn("TemplateChannelId").AsInt32().PrimaryKey().Identity()
                .WithColumn("TemplateId").AsInt32().NotNullable()
                .WithColumn("ChannelType").AsString(20).NotNullable()
                .WithColumn("ChannelSpecificContent").AsCustom("nvarchar(max)").Nullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedByUserId").AsInt32().NotNullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            // 8. ChannelProviders table
            Create.Table("ChannelProviders")
                .WithColumn("ProviderId").AsInt32().PrimaryKey().Identity()
                .WithColumn("TenantId").AsInt32().NotNullable()
                .WithColumn("Name").AsString(100).NotNullable()
                .WithColumn("ChannelType").AsString(20).NotNullable()
                .WithColumn("Configuration").AsCustom("nvarchar(max)").NotNullable()
                .WithColumn("IsDefault").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(1)
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("UpdatedAt").AsDateTime().Nullable()
                .WithColumn("CreatedByUserId").AsInt32().NotNullable();

            // 9. NotificationRequests table
            Create.Table("NotificationRequests")
                .WithColumn("RequestId").AsGuid().PrimaryKey()
                .WithColumn("ApplicationId").AsInt32().NotNullable()
                .WithColumn("TemplateId").AsInt32().NotNullable()
                .WithColumn("RequestData").AsCustom("nvarchar(max)").NotNullable()
                .WithColumn("Priority").AsString(20).NotNullable().WithDefaultValue("Normal")
                .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Pending")
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("ProcessedAt").AsDateTime().Nullable()
                .WithColumn("ExpirationAt").AsDateTime().Nullable()
                .WithColumn("CallbackUrl").AsString(500).Nullable()
                .WithColumn("RequestedByUserId").AsInt32().Nullable();

            // 10. MessageQueue table
            Create.Table("MessageQueue")
                .WithColumn("QueueId").AsInt64().PrimaryKey().Identity()
                .WithColumn("RequestId").AsGuid().NotNullable()
                .WithColumn("RecipientId").AsInt32().NotNullable()
                .WithColumn("ChannelType").AsString(20).NotNullable()
                .WithColumn("MessageContent").AsCustom("nvarchar(max)").NotNullable()
                .WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Queued")
                .WithColumn("ScheduledAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("ProcessedAt").AsDateTime().Nullable();

            // 11. MessageDeliveries table
            Create.Table("MessageDeliveries")
                .WithColumn("DeliveryId").AsInt32().PrimaryKey().Identity()
                .WithColumn("QueueId").AsInt64().Nullable()
                .WithColumn("RequestId").AsGuid().NotNullable()
                .WithColumn("RecipientId").AsInt32().NotNullable()
                .WithColumn("ProviderId").AsInt32().NotNullable()
                .WithColumn("ChannelType").AsString(20).NotNullable()
                .WithColumn("MessageContent").AsCustom("nvarchar(max)").NotNullable()
                .WithColumn("Status").AsString(20).NotNullable().WithDefaultValue("Queued")
                .WithColumn("AttemptCount").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("LastAttemptAt").AsDateTime().Nullable()
                .WithColumn("DeliveredAt").AsDateTime().Nullable()
                .WithColumn("ProviderResponse").AsString(500).Nullable()
                .WithColumn("ProviderMessageId").AsString(100).Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            // 12. DeliveryLogs table
            Create.Table("DeliveryLogs")
                .WithColumn("LogId").AsInt32().PrimaryKey().Identity()
                .WithColumn("DeliveryId").AsInt32().NotNullable()
                .WithColumn("EventType").AsString(50).NotNullable()
                .WithColumn("EventData").AsCustom("nvarchar(max)").Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            // 13. EventLogs table
            Create.Table("EventLogs")
                .WithColumn("EventId").AsInt32().PrimaryKey().Identity()
                .WithColumn("EntityType").AsString(50).NotNullable()
                .WithColumn("EntityId").AsString(50).NotNullable()
                .WithColumn("EventType").AsString(50).NotNullable()
                .WithColumn("EventData").AsCustom("nvarchar(max)").Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("CreatedByUserId").AsInt32().Nullable();

            // Create all foreign keys
            Create.ForeignKey("FK_Tenants_Users")
                .FromTable("Tenants").ForeignColumn("OwnerUserId")
                .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_Applications_Tenants")
                .FromTable("Applications").ForeignColumn("TenantId")
                .ToTable("Tenants").PrimaryColumn("TenantId");

            Create.ForeignKey("FK_Applications_Users")
                .FromTable("Applications").ForeignColumn("OwnerUserId")
                .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_RecipientGroups_Tenants")
                .FromTable("RecipientGroups").ForeignColumn("TenantId")
                .ToTable("Tenants").PrimaryColumn("TenantId");

            Create.ForeignKey("FK_RecipientGroups_Users")
                .FromTable("RecipientGroups").ForeignColumn("CreatedByUserId")
                .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_Recipients_Tenants")
                .FromTable("Recipients").ForeignColumn("TenantId")
                .ToTable("Tenants").PrimaryColumn("TenantId");

            Create.ForeignKey("FK_Recipients_Users")
                .FromTable("Recipients").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_GroupMembers_Groups")
                .FromTable("RecipientGroupMembers").ForeignColumn("GroupId")
                .ToTable("RecipientGroups").PrimaryColumn("GroupId");

            Create.ForeignKey("FK_GroupMembers_Recipients")
                .FromTable("RecipientGroupMembers").ForeignColumn("RecipientId")
                .ToTable("Recipients").PrimaryColumn("RecipientId");

            Create.ForeignKey("FK_GroupMembers_Users")
                .FromTable("RecipientGroupMembers").ForeignColumn("AddedByUserId")
                .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_Templates_Applications")
                .FromTable("NotificationTemplates").ForeignColumn("ApplicationId")
                .ToTable("Applications").PrimaryColumn("ApplicationId");

            Create.ForeignKey("FK_Templates_CreatedBy")
                .FromTable("NotificationTemplates").ForeignColumn("CreatedByUserId")
                .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_Templates_ApprovedBy")
                .FromTable("NotificationTemplates").ForeignColumn("ApprovedByUserId")
                .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_TemplateChannels_Templates")
                .FromTable("TemplateChannels").ForeignColumn("TemplateId")
                .ToTable("NotificationTemplates").PrimaryColumn("TemplateId");

            Create.ForeignKey("FK_TemplateChannels_Users")
                .FromTable("TemplateChannels").ForeignColumn("CreatedByUserId")
                .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_ChannelProviders_Tenants")
                .FromTable("ChannelProviders").ForeignColumn("TenantId")
                .ToTable("Tenants").PrimaryColumn("TenantId");

            Create.ForeignKey("FK_ChannelProviders_Users")
                .FromTable("ChannelProviders").ForeignColumn("CreatedByUserId")
            .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_NotificationRequests_Applications")
                .FromTable("NotificationRequests").ForeignColumn("ApplicationId")
            .ToTable("Applications").PrimaryColumn("ApplicationId");

            Create.ForeignKey("FK_NotificationRequests_Templates")
                .FromTable("NotificationRequests").ForeignColumn("TemplateId")
                .ToTable("NotificationTemplates").PrimaryColumn("TemplateId");

            Create.ForeignKey("FK_NotificationRequests_Users")
                .FromTable("NotificationRequests").ForeignColumn("RequestedByUserId")
            .ToTable("Users").PrimaryColumn("UserId");

            Create.ForeignKey("FK_MessageQueue_Requests")
                .FromTable("MessageQueue").ForeignColumn("RequestId")
                .ToTable("NotificationRequests").PrimaryColumn("RequestId");

            Create.ForeignKey("FK_MessageQueue_Recipients")
                .FromTable("MessageQueue").ForeignColumn("RecipientId")
            .ToTable("Recipients").PrimaryColumn("RecipientId");

            Create.ForeignKey("FK_MessageDeliveries_Queue")
                .FromTable("MessageDeliveries").ForeignColumn("QueueId")
            .ToTable("MessageQueue").PrimaryColumn("QueueId");

            Create.ForeignKey("FK_MessageDeliveries_Requests")
                .FromTable("MessageDeliveries").ForeignColumn("RequestId")
                .ToTable("NotificationRequests").PrimaryColumn("RequestId");

            Create.ForeignKey("FK_MessageDeliveries_Recipients")
                .FromTable("MessageDeliveries").ForeignColumn("RecipientId")
                .ToTable("Recipients").PrimaryColumn("RecipientId");

            Create.ForeignKey("FK_MessageDeliveries_Providers")
                .FromTable("MessageDeliveries").ForeignColumn("ProviderId")
                .ToTable("ChannelProviders").PrimaryColumn("ProviderId");

            Create.ForeignKey("FK_DeliveryLogs_Deliveries")
                .FromTable("DeliveryLogs").ForeignColumn("DeliveryId")
                .ToTable("MessageDeliveries").PrimaryColumn("DeliveryId");

            Create.ForeignKey("FK_EventLogs_Users")
                .FromTable("EventLogs").ForeignColumn("CreatedByUserId")
                .ToTable("Users").PrimaryColumn("UserId");

            // Create indexes
            Create.Index("IX_Applications_TenantId_Name")
                .OnTable("Applications")
                .OnColumn("TenantId").Ascending()
                .OnColumn("Name").Ascending()
            .WithOptions().Unique();

            Create.Index("IX_Recipients_TenantId_Email")
                .OnTable("Recipients")
                .OnColumn("TenantId").Ascending()
                .OnColumn("Email").Ascending()
            .WithOptions().Unique();

            Create.Index("IX_NotificationRequests_Status")
                .OnTable("NotificationRequests")
                .OnColumn("Status").Ascending()
            .OnColumn("CreatedAt").Ascending();

            Create.Index("IX_MessageQueue_Status_ScheduledAt")
                .OnTable("MessageQueue")
                .OnColumn("Status").Ascending()
                .OnColumn("ScheduledAt").Ascending();

            Create.Index("IX_MessageDeliveries_Status")
                .OnTable("MessageDeliveries")
                .OnColumn("Status").Ascending();
        }

        public override void Down()
        {
            // Drop in reverse order to respect foreign key constraints
            Delete.Table("EventLogs");
            Delete.Table("DeliveryLogs");
            Delete.Table("MessageDeliveries");
            Delete.Table("MessageQueue");
            Delete.Table("NotificationRequests");
            Delete.Table("TemplateChannels");
            Delete.Table("NotificationTemplates");
            Delete.Table("ChannelProviders");
            Delete.Table("RecipientGroupMembers");
            Delete.Table("Recipients");
            Delete.Table("RecipientGroups");
            Delete.Table("Applications");
            Delete.Table("Tenants");
        }
    }
}