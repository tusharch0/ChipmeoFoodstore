namespace FoodstoreApi.Core.Configuration;

/// <summary>
/// IntaSend adapter configuration. Secrets are injected from the environment (<c>IntaSend__*</c>);
/// source control only carries placeholders.
/// </summary>
public class IntaSendOptions
{
    public const string SectionName = "IntaSend";

    /// <summary>Base API URL, e.g. sandbox <c>https://sandbox.intasend.com</c> or live <c>https://payment.intasend.com</c>.</summary>
    public string BaseUrl { get; set; } = "https://sandbox.intasend.com";

    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Shared challenge string configured on the IntaSend webhook, used to authenticate callbacks.</summary>
    public string WebhookChallenge { get; set; } = string.Empty;

    public string DefaultCurrency { get; set; } = "KES";

    /// <summary>Maximum age (seconds) a webhook timestamp may be before it is rejected as stale/replayed.</summary>
    public int WebhookMaxAgeSeconds { get; set; } = 300;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl)
        && !string.IsNullOrWhiteSpace(SecretKey)
        && SecretKey != "overridden_by_env";
}
