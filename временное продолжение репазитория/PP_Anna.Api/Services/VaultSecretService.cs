using Microsoft.Extensions.Logging;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods;

namespace PP_Anna.Api.Services;

public class VaultSecretService : ISecretService
{
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<VaultSecretService> _logger;

    public VaultSecretService(ILogger<VaultSecretService> logger)
    {
        _logger = logger;
        
        // Настройка подключения к Vault
        var vaultAddress = "http://localhost:8200";
        var token = "root"; // совпадает с VAULT_DEV_ROOT_TOKEN_ID

        var authMethod = new TokenAuthMethodInfo(token);
        var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);
        _vaultClient = new VaultClient(vaultClientSettings);
    }

    public async Task<string?> GetSecretAsync(string key)
    {
        try
        {
            // В Vault секреты хранятся в виде ключ-значение в секретных движках.
            // Мы будем использовать движок "secret" (по умолчанию).
            // Путь: secret/data/{key} – в версии KV v2.
            var secretPath = $"secret/data/{key}";
            var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: secretPath);
            
            if (secret != null && secret.Data != null && secret.Data.Data != null)
            {
                // В Data.Data находится словарь с секретами.
                // Ожидаем, что секрет хранится по ключу "value" (или можно изменить по желанию)
                if (secret.Data.Data.TryGetValue("value", out var valueObj))
                {
                    var value = valueObj?.ToString();
                    _logger.LogInformation("[VAULT] Секрет {Key} получен", key);
                    return value;
                }
            }
            _logger.LogWarning("[VAULT] Секрет {Key} не найден", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[VAULT] Ошибка при получении секрета {Key}", key);
            return null;
        }
    }
}