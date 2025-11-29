using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SignalWrapper
{
    public class SignalApiClient : IDisposable
    {
        private readonly RestClient _client;
        private bool _disposed = false;

        public SignalApiClient(string baseUrl, int timeoutMilliseconds = 30000)
        {
            var options = new RestClientOptions(baseUrl)
            {
                ThrowOnAnyError = false,
                Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds)
            };
            _client = new RestClient(options);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _client?.Dispose();
            }

            _disposed = true;
        }

        // General
        public async Task<About> GetAboutAsync() => await ExecuteAsync<About>(new RestRequest("/v1/about", Method.Get));
        public async Task<Configuration> GetConfigurationAsync() => await ExecuteAsync<Configuration>(new RestRequest("/v1/configuration", Method.Get));
        public async Task SetConfigurationAsync(Configuration config) => await ExecuteAsync(new RestRequest("/v1/configuration", Method.Post).AddJsonBody(config));

        // Accounts
        public async Task<List<string>> GetAccountsAsync() => await ExecuteAsync<List<string>>(new RestRequest("/v1/accounts", Method.Get));
        public async Task SetPinAsync(string number, string pin) => await ExecuteAsync(new RestRequest($"/v1/accounts/{number}/pin", Method.Post).AddJsonBody(new { pin }));
        public async Task RemovePinAsync(string number) => await ExecuteAsync(new RestRequest($"/v1/accounts/{number}/pin", Method.Delete));
        
        public async Task RateLimitChallengeAsync(string number, RateLimitChallengeRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/accounts/{number}/rate-limit-challenge", Method.Post).AddJsonBody(request));
        
        public async Task UpdateAccountSettingsAsync(string number, UpdateAccountSettingsRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/accounts/{number}/settings", Method.Put).AddJsonBody(request));
        
        public async Task<SetUsernameResponse> SetUsernameAsync(string number, SetUsernameRequest request) =>
            await ExecuteAsync<SetUsernameResponse>(new RestRequest($"/v1/accounts/{number}/username", Method.Post).AddJsonBody(request));
        
        public async Task RemoveUsernameAsync(string number) =>
            await ExecuteAsync(new RestRequest($"/v1/accounts/{number}/username", Method.Delete));

        // Devices
        public async Task RegisterNumberAsync(string number, RegisterNumberRequest request = null) =>
            await ExecuteAsync(new RestRequest($"/v1/register/{number}", Method.Post).AddJsonBody(request ?? new RegisterNumberRequest()));

        public async Task VerifyNumberAsync(string number, string token, VerifyNumberSettings settings = null) =>
            await ExecuteAsync(new RestRequest($"/v1/register/{number}/verify/{token}", Method.Post).AddJsonBody(settings ?? new VerifyNumberSettings()));

        // Messages
        [Obsolete("/v1/send is deprecated.")]
        public async Task<string> SendMessageLegacyAsync(SendMessageLegacy message) =>
            await ExecuteAsStringAsync(new RestRequest("/v1/send", Method.Post).AddJsonBody(message));

        public async Task<SendMessageResponse> SendMessageAsync(SendMessage message) =>
            await ExecuteAsync<SendMessageResponse>(new RestRequest("/v2/send", Method.Post).AddJsonBody(message));

        // Contacts
        public async Task<List<ListContactsResponse>> GetContactsAsync(string number) =>
            await ExecuteAsync<List<ListContactsResponse>>(new RestRequest($"/v1/contacts/{number}", Method.Get));

        public async Task UpdateContactAsync(string number, UpdateContactRequest contact) =>
            await ExecuteAsync(new RestRequest($"/v1/contacts/{number}", Method.Put).AddJsonBody(contact));

        public async Task<ListContactsResponse> GetContactAsync(string number, string uuid) =>
            await ExecuteAsync<ListContactsResponse>(new RestRequest($"/v1/contacts/{number}/{uuid}", Method.Get));

        public async Task<string?> GetContactAvatarAsync(string number, string uuid) =>
            await ExecuteAsStringAsync(new RestRequest($"/v1/contacts/{number}/{uuid}/avatar", Method.Get));
        
        public async Task SyncContactsAsync(string number) =>
            await ExecuteAsync(new RestRequest($"/v1/contacts/{number}/sync", Method.Post));

        // Attachments
        public async Task<List<string>> GetAttachmentsAsync() =>
            await ExecuteAsync<List<string>>(new RestRequest("/v1/attachments", Method.Get));
        
        public async Task<string?> GetAttachmentAsync(string attachmentId) =>
            await ExecuteAsStringAsync(new RestRequest($"/v1/attachments/{attachmentId}", Method.Get));
        
        public async Task DeleteAttachmentAsync(string attachmentId) =>
            await ExecuteAsync(new RestRequest($"/v1/attachments/{attachmentId}", Method.Delete));

        // Groups
        public async Task<List<GroupEntry>> GetGroupsAsync(string number) =>
            await ExecuteAsync<List<GroupEntry>>(new RestRequest($"/v1/groups/{number}", Method.Get));

        public async Task<CreateGroupResponse> CreateGroupAsync(string number, CreateGroupRequest request) =>
            await ExecuteAsync<CreateGroupResponse>(new RestRequest($"/v1/groups/{number}", Method.Post).AddJsonBody(request));

        public async Task<GroupEntry> GetGroupAsync(string number, string groupid) =>
            await ExecuteAsync<GroupEntry>(new RestRequest($"/v1/groups/{number}/{groupid}", Method.Get));

        public async Task UpdateGroupAsync(string number, string groupid, UpdateGroupRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/groups/{number}/{groupid}", Method.Put).AddJsonBody(request));

        public async Task DeleteGroupAsync(string number, string groupid) =>
            await ExecuteAsync(new RestRequest($"/v1/groups/{number}/{groupid}", Method.Delete));

        public async Task AddGroupAdminsAsync(string number, string groupid, ChangeGroupAdminsRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/groups/{number}/{groupid}/admins", Method.Post).AddJsonBody(request));

        public async Task RemoveGroupAdminsAsync(string number, string groupid, ChangeGroupAdminsRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/groups/{number}/{groupid}/admins", Method.Delete).AddJsonBody(request));

        public async Task<string?> GetGroupAvatarAsync(string number, string groupid) =>
            await ExecuteAsStringAsync(new RestRequest($"/v1/groups/{number}/{groupid}/avatar", Method.Get));

        public async Task BlockGroupAsync(string number, string groupid) =>
            await ExecuteAsync(new RestRequest($"/v1/groups/{number}/{groupid}/block", Method.Post));

        public async Task JoinGroupAsync(string number, string groupid) =>
            await ExecuteAsync(new RestRequest($"/v1/groups/{number}/{groupid}/join", Method.Post));

        public async Task AddGroupMembersAsync(string number, string groupid, ChangeGroupMembersRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/groups/{number}/{groupid}/members", Method.Post).AddJsonBody(request));

        public async Task RemoveGroupMembersAsync(string number, string groupid, ChangeGroupMembersRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/groups/{number}/{groupid}/members", Method.Delete).AddJsonBody(request));

        public async Task QuitGroupAsync(string number, string groupid) =>
            await ExecuteAsync(new RestRequest($"/v1/groups/{number}/{groupid}/quit", Method.Post));

        // Identities
        public async Task<List<IdentityEntry>> GetIdentitiesAsync(string number) =>
            await ExecuteAsync<List<IdentityEntry>>(new RestRequest($"/v1/identities/{number}", Method.Get));

        public async Task TrustIdentityAsync(string number, string numberToTrust, TrustIdentityRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/identities/{number}/trust/{numberToTrust}", Method.Put).AddJsonBody(request));

        // Profiles
        public async Task UpdateProfileAsync(string number, UpdateProfileRequest profile) =>
            await ExecuteAsync(new RestRequest($"/v1/profiles/{number}", Method.Put).AddJsonBody(profile));

        // Reactions
        public async Task SendReactionAsync(string number, ReactionModel reaction) =>
            await ExecuteAsync(new RestRequest($"/v1/reactions/{number}", Method.Post).AddJsonBody(reaction));

        public async Task RemoveReactionAsync(string number, ReactionModel reaction) =>
            await ExecuteAsync(new RestRequest($"/v1/reactions/{number}", Method.Delete).AddJsonBody(reaction));

        // Receipts
        public async Task SendReceiptAsync(string number, Receipt receipt) =>
            await ExecuteAsync(new RestRequest($"/v1/receipts/{number}", Method.Post).AddJsonBody(receipt));

        // Search
        public async Task<List<SearchResponse>> SearchAsync(string number, List<string> numbers) =>
            await ExecuteAsync<List<SearchResponse>>(new RestRequest($"/v1/search/{number}", Method.Get)
                .AddQueryParameter("numbers", string.Join(",", numbers)));

        // Sticker Packs
        public async Task<List<ListInstalledStickerPacksResponse>> GetStickerPacksAsync(string number) =>
            await ExecuteAsync<List<ListInstalledStickerPacksResponse>>(new RestRequest($"/v1/sticker-packs/{number}", Method.Get));

        public async Task AddStickerPackAsync(string number, AddStickerPackRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/sticker-packs/{number}", Method.Post).AddJsonBody(request));

        // Typing Indicators
        public async Task ShowTypingIndicatorAsync(string number, TypingIndicatorRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/typing-indicator/{number}", Method.Put).AddJsonBody(request));

        public async Task HideTypingIndicatorAsync(string number, TypingIndicatorRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/typing-indicator/{number}", Method.Delete).AddJsonBody(request));

        // QR Code Link
        public async Task<string?> GenerateQrCodeLinkAsync(string deviceName, int? qrcodeVersion = null) =>
            await ExecuteAsStringAsync(new RestRequest("/v1/qrcodelink", Method.Get)
                .AddQueryParameter("device_name", deviceName)
                .AddQueryParameter("qrcode_version", qrcodeVersion?.ToString()));

        // Health Check
        public async Task HealthCheckAsync() =>
            await ExecuteAsync(new RestRequest("/v1/health", Method.Get));

        // Devices (additional)
        public async Task<List<ListDevicesResponse>> GetDevicesAsync(string number) =>
            await ExecuteAsync<List<ListDevicesResponse>>(new RestRequest($"/v1/devices/{number}", Method.Get));

        public async Task LinkDeviceAsync(string number, AddDeviceRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/devices/{number}", Method.Post).AddJsonBody(request));

        // Unregister
        public async Task UnregisterNumberAsync(string number, UnregisterNumberRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/unregister/{number}", Method.Post).AddJsonBody(request));

        // Remote Delete
        public async Task<RemoteDeleteResponse> RemoteDeleteAsync(string number, RemoteDeleteRequest request) =>
            await ExecuteAsync<RemoteDeleteResponse>(new RestRequest($"/v1/remote-delete/{number}", Method.Delete).AddJsonBody(request));

        // Receive Messages
        public async Task<List<string>> ReceiveMessagesAsync(string number, int? timeout = null, bool? ignoreAttachments = null, 
            bool? ignoreStories = null, int? maxMessages = null, bool? sendReadReceipts = null)
        {
            var request = new RestRequest($"/v1/receive/{number}", Method.Get);
            
            if (timeout.HasValue)
                request.AddQueryParameter("timeout", timeout.Value.ToString());
            if (ignoreAttachments.HasValue)
                request.AddQueryParameter("ignore_attachments", ignoreAttachments.Value.ToString().ToLower());
            if (ignoreStories.HasValue)
                request.AddQueryParameter("ignore_stories", ignoreStories.Value.ToString().ToLower());
            if (maxMessages.HasValue)
                request.AddQueryParameter("max_messages", maxMessages.Value.ToString());
            if (sendReadReceipts.HasValue)
                request.AddQueryParameter("send_read_receipts", sendReadReceipts.Value.ToString().ToLower());
            
            return await ExecuteAsync<List<string>>(request);
        }

        // Configuration Settings
        public async Task<TrustModeResponse> GetAccountSettingsAsync(string number) =>
            await ExecuteAsync<TrustModeResponse>(new RestRequest($"/v1/configuration/{number}/settings", Method.Get));

        public async Task SetAccountSettingsAsync(string number, TrustModeRequest request) =>
            await ExecuteAsync(new RestRequest($"/v1/configuration/{number}/settings", Method.Post).AddJsonBody(request));

        // Private methods
        private async Task<T> ExecuteAsync<T>(RestRequest request) where T : class, new()
        {
            var response = await _client.ExecuteAsync<T>(request);
            HandleResponse(response);
            return response.Data;
        }

        private async Task ExecuteAsync(RestRequest request)
        {
            var response = await _client.ExecuteAsync(request);
            HandleResponse(response);
        }

        private async Task<string?> ExecuteAsStringAsync(RestRequest request)
        {
            var response = await _client.ExecuteAsync(request);
            HandleResponse(response);
            return response.Content ?? string.Empty;
        }

        private void HandleResponse(RestResponse response)
        {
            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.NoContent)
                return;

            Error? error = null;
            try
            {
                var content = response.Content ?? string.Empty;
                if (!string.IsNullOrEmpty(content))
                {
                    error = JsonConvert.DeserializeObject<Error>(content);
                }
            }
            catch
            {
                // If deserialization fails, fall back to content as string
                throw new Exception($"Request failed with status {response.StatusCode}: {response.Content ?? "No content"}");
            }
            throw new Exception($"API Error: {error?.Message ?? "Unknown error"}");
        }
    }

    // Core Models
    public class About
    {
        public int Build { get; set; }
        public string Mode { get; set; }
        public string Version { get; set; }
        public List<string> Versions { get; set; }
        public Dictionary<string, List<string>> Capabilities { get; set; }
    }

    public class Configuration
    {
        public LoggingConfiguration Logging { get; set; }
    }

    public class LoggingConfiguration
    {
        public string Level { get; set; }
    }

    public class Error
    {
        [JsonProperty("error")]
        public string Message { get; set; }
    }

    // Account Models
    public class RegisterNumberRequest
    {
        [JsonProperty("captcha")]
        public string Captcha { get; set; }
        
        [JsonProperty("use_voice")]
        public bool UseVoice { get; set; }
    }

    public class VerifyNumberSettings
    {
        [JsonProperty("pin")]
        public string Pin { get; set; }
    }

    // Message Models
    public class SendMessageLegacy
    {
        [JsonProperty("base64_attachment")]
        public string Base64Attachment { get; set; }
        
        [JsonProperty("is_group")]
        public bool IsGroup { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("number")]
        public string Number { get; set; }
        
        [JsonProperty("recipients")]
        public List<string> Recipients { get; set; }
    }

    public class SendMessage
    {
        [JsonProperty("base64_attachments")]
        public List<string> Base64Attachments { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("number")]
        public string Number { get; set; }
        
        [JsonProperty("recipients")]
        public List<string> Recipients { get; set; }
        
        [JsonProperty("text_mode")]
        public string TextMode { get; set; } = "normal";
        
        [JsonProperty("edit_timestamp")]
        public long? EditTimestamp { get; set; }
        
        [JsonProperty("link_preview")]
        public LinkPreviewType LinkPreview { get; set; }
        
        [JsonProperty("mentions")]
        public List<MessageMention> Mentions { get; set; }
        
        [JsonProperty("notify_self")]
        public bool? NotifySelf { get; set; }
        
        [JsonProperty("quote_author")]
        public string QuoteAuthor { get; set; }
        
        [JsonProperty("quote_mentions")]
        public List<MessageMention> QuoteMentions { get; set; }
        
        [JsonProperty("quote_message")]
        public string QuoteMessage { get; set; }
        
        [JsonProperty("quote_timestamp")]
        public long? QuoteTimestamp { get; set; }
        
        [JsonProperty("sticker")]
        public string Sticker { get; set; }
        
        [JsonProperty("view_once")]
        public bool? ViewOnce { get; set; }
    }

    public class LinkPreviewType
    {
        public string Base64Thumbnail { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public class MessageMention
    {
        public string Author { get; set; }
        public int Length { get; set; }
        public int Start { get; set; }
    }

    public class SendMessageResponse
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }

    // Contact Models
    public class ListContactsResponse
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string GivenName { get; set; }
        public ContactProfile Profile { get; set; }
        public bool Blocked { get; set; }
        public string Color { get; set; }
        public string MessageExpiration { get; set; }
        public Nickname Nickname { get; set; }
        public string Note { get; set; }
        public string ProfileName { get; set; }
        public string Username { get; set; }
        public string Uuid { get; set; }
    }

    public class ContactProfile
    {
        public string GivenName { get; set; }
        public string About { get; set; }
        public bool HasAvatar { get; set; }
        public long LastUpdatedTimestamp { get; set; }
        public string Lastname { get; set; }
    }

    public class Nickname
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }
    }

    // Group Models
    public class GroupEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Members { get; set; }
        public List<string> Admins { get; set; }
        public bool Blocked { get; set; }
        public string Description { get; set; }
        public string InternalId { get; set; }
        public string InviteLink { get; set; }
        public List<string> PendingInvites { get; set; }
        public List<string> PendingRequests { get; set; }
    }

    public class CreateGroupRequest
    {
        public string Name { get; set; }
        public List<string> Members { get; set; }
        public string Description { get; set; }
        public GroupPermissions Permissions { get; set; }
        public int? ExpirationTime { get; set; }
        public string GroupLink { get; set; }
    }

    public class GroupPermissions
    {
        public string AddMembers { get; set; } = "only-admins";
        public string EditGroup { get; set; } = "only-admins";
        public string SendMessages { get; set; } = "every-member";
    }

    public class CreateGroupResponse
    {
        public string Id { get; set; }
    }

    // Identity Models
    public class IdentityEntry
    {
        public string Number { get; set; }
        public string Uuid { get; set; }
        public string Fingerprint { get; set; }
        public string SafetyNumber { get; set; }
        public string Status { get; set; }
        public string Added { get; set; }
    }

    // Profile Models
    public class UpdateProfileRequest
    {
        public string Name { get; set; }
        public string About { get; set; }
        public string Base64Avatar { get; set; }
    }

    // Reaction Models
    public class ReactionModel
    {
        [JsonProperty("reaction")]
        public string Reaction { get; set; }
        
        [JsonProperty("recipient")]
        public string Recipient { get; set; }
        
        [JsonProperty("target_author")]
        public string TargetAuthor { get; set; }
        
        [JsonProperty("timestamp")]
        public long? Timestamp { get; set; }
    }

    // Receipt Models
    public class Receipt
    {
        [JsonProperty("receipt_type")]
        public string ReceiptType { get; set; }
        
        [JsonProperty("recipient")]
        public string Recipient { get; set; }
        
        [JsonProperty("timestamp")]
        public long? Timestamp { get; set; }
    }

    // Search Models
    public class SearchResponse
    {
        [JsonProperty("number")]
        public string Number { get; set; }
        
        [JsonProperty("registered")]
        public bool Registered { get; set; }
    }

    // Sticker Pack Models
    public class ListInstalledStickerPacksResponse
    {
        [JsonProperty("pack_id")]
        public string PackId { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("author")]
        public string Author { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("installed")]
        public bool Installed { get; set; }
    }

    public class AddStickerPackRequest
    {
        [JsonProperty("pack_id")]
        public string PackId { get; set; }
        
        [JsonProperty("pack_key")]
        public string PackKey { get; set; }
    }

    // Typing Indicator Models
    public class TypingIndicatorRequest
    {
        [JsonProperty("recipient")]
        public string Recipient { get; set; }
    }

    // Device Models
    public class ListDevicesResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("creation_timestamp")]
        public long CreationTimestamp { get; set; }
        
        [JsonProperty("last_seen_timestamp")]
        public long LastSeenTimestamp { get; set; }
    }

    public class AddDeviceRequest
    {
        [JsonProperty("uri")]
        public string Uri { get; set; } = string.Empty;
    }

    // Unregister Models
    public class UnregisterNumberRequest
    {
        [JsonProperty("delete_account")]
        public bool DeleteAccount { get; set; }
        
        [JsonProperty("delete_local_data")]
        public bool DeleteLocalData { get; set; }
    }

    // Remote Delete Models
    public class RemoteDeleteRequest
    {
        [JsonProperty("recipient")]
        public string Recipient { get; set; }
        
        [JsonProperty("timestamp")]
        public long? Timestamp { get; set; }
    }

    public class RemoteDeleteResponse
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }

    // Update Contact Models
    public class UpdateContactRequest
    {
        [JsonProperty("recipient")]
        public string Recipient { get; set; } = string.Empty;
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("expiration_in_seconds")]
        public int? ExpirationInSeconds { get; set; }
    }

    // Update Group Models
    public class UpdateGroupRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("base64_avatar")]
        public string Base64Avatar { get; set; }
        
        [JsonProperty("expiration_time")]
        public int? ExpirationTime { get; set; }
        
        [JsonProperty("group_link")]
        public string GroupLink { get; set; }
        
        [JsonProperty("permissions")]
        public GroupPermissions Permissions { get; set; }
    }

    // Trust Identity Models
    public class TrustIdentityRequest
    {
        [JsonProperty("trust_all_known_keys")]
        public bool TrustAllKnownKeys { get; set; }
        
        [JsonProperty("verified_safety_number")]
        public string VerifiedSafetyNumber { get; set; }
    }

    // Configuration Settings Models
    public class TrustModeRequest
    {
        [JsonProperty("trust_mode")]
        public string TrustMode { get; set; }
    }

    public class TrustModeResponse
    {
        [JsonProperty("trust_mode")]
        public string TrustMode { get; set; }
    }

    // Change Group Admins/Members Models
    public class ChangeGroupAdminsRequest
    {
        [JsonProperty("admins")]
        public List<string> Admins { get; set; }
    }

    public class ChangeGroupMembersRequest
    {
        [JsonProperty("members")]
        public List<string> Members { get; set; }
    }

    // Rate Limit Challenge Models
    public class RateLimitChallengeRequest
    {
        [JsonProperty("challenge_token")]
        public string ChallengeToken { get; set; }
        
        [JsonProperty("captcha")]
        public string Captcha { get; set; }
    }

    // Account Settings Models
    public class UpdateAccountSettingsRequest
    {
        [JsonProperty("discoverable_by_number")]
        public bool? DiscoverableByNumber { get; set; }
        
        [JsonProperty("share_number")]
        public bool? ShareNumber { get; set; }
    }

    // Username Models
    public class SetUsernameRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }
    }

    public class SetUsernameResponse
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty("username_link")]
        public string UsernameLink { get; set; }
    }
}
